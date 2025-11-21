using System.Reflection;

using FastEndpoints;

using Infernity.Framework.Configuration;
using Infernity.Framework.Core;
using Infernity.Framework.Core.Exceptions;
using Infernity.Framework.Core.Exceptions.Default;
using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Patterns.Disposal;
using Infernity.Framework.Logging;
using Infernity.Framework.Plugins;
using Infernity.Framework.Plugins.Providers;
using Infernity.Framework.Plugins.Selectors;
using Infernity.Framework.Plugins.Web;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog.Events;

namespace Infernity.Framework.Web;

public class WebApplicationHost : Disposable, IDisposable
{
    private readonly string _applicationId;
    private readonly bool _enableMvc;
    private readonly bool _enableEndpoints;
    private readonly ILoggingBinder _loggingBinder;
    private readonly IReadOnlyList<IPluginProvider> _pluginProviders;
    private readonly IPluginSelector _pluginSelector;
    private readonly WebApplicationBuilder _builder;
    private readonly IConfiguration _configuration;
    private Optional<IExceptionHandler> _exceptionHandler;

    public WebApplicationHost(
        string applicationId,
        IReadOnlyList<IPluginProvider> pluginProviders,
        IPluginSelector? pluginSelector = null,
        bool enableMvc = true,
        bool enableEndpoints = true)
    {
        _applicationId = applicationId;
        _enableMvc = enableMvc;
        _enableEndpoints = enableEndpoints;
        _builder = WebApplication.CreateBuilder();
        _loggingBinder = ILoggingBinder.Create(applicationId,
            _builder.Environment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Information);
        _configuration = _builder.CreateDefaultConfiguration(applicationId);
        _exceptionHandler = Optional.None<IExceptionHandler>();
        _pluginProviders = pluginProviders;
        _pluginSelector = pluginSelector ?? new DelegatePluginSelector(t => true);
    }

    public async Task Run(string[] arguments,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var application = OnConfigure(_builder,
                _configuration,
                out var pluginBinder);

            _exceptionHandler = Optional.Some(application.Services.GetRequiredService<IExceptionHandler>());

            OnConfigure(application,
                pluginBinder);

            await application.RunAsync();
        }
        catch (Exception exception)
        {
            var wasHandled = false;

            if (_exceptionHandler)
            {
                try
                {
                    wasHandled = _exceptionHandler.Value.Handle(exception);
                }
                catch (Exception innerException)
                {
                    _loggingBinder.Logger.Fatal(innerException,
                        exception.Message);
                }
            }

            if (!wasHandled)
            {
                _loggingBinder.Logger.Fatal(exception,
                    exception.Message);
            }
        }
    }

    protected virtual void OnConfigure(WebApplication application,
        IWebPluginBinder pluginBinder)
    {
        pluginBinder.Configure(application);

        if (_enableEndpoints)
        {
            application.UseFastEndpoints();
        }

        if (_enableMvc)
        {
            application.MapControllers();
        }
    }

    protected virtual WebApplication OnConfigure(WebApplicationBuilder builder,
        IConfiguration configuration,
        out IWebPluginBinder pluginBinder)
    {
        _loggingBinder.Apply(configuration,
            builder.Services);

        var localPluginBinder = builder.AddPlugins(_pluginProviders,
            _pluginSelector);

        builder.Services.AddSingleton<IExceptionHandler, DefaultExceptionHandler>();

        if (_enableEndpoints)
        {
            builder.Services.AddFastEndpoints(ep =>
            {
                localPluginBinder.Configure(ep);
            });
        }

        if (_enableMvc)
        {
            var mvcBuilder = builder.Services.AddControllersWithViews();

            mvcBuilder.ConfigureApplicationPartManager(partManager =>
            {
                localPluginBinder.Configure(partManager);
            });
        }

        pluginBinder = localPluginBinder;
        return builder.Build();
    }

    protected override void OnDispose()
    {
        _loggingBinder.Dispose();
    }
}