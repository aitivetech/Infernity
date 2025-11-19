using System.Reflection;

using FastEndpoints;

using Infernity.Framework.Configuration;
using Infernity.Framework.Core;
using Infernity.Framework.Core.Exceptions;
using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Patterns.Disposal;
using Infernity.Framework.Logging;
using Infernity.Framework.Plugins;
using Infernity.Framework.Plugins.Providers;
using Infernity.Framework.Plugins.Selectors;
using Infernity.Framework.Plugins.Web;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infernity.Framework.Web;

public class WebApplicationHost : Disposable,IDisposable
{
    private readonly string _applicationId;
    private readonly bool _enableMvc;
    private readonly bool _enableEndpoints;
    private readonly ILoggingBinder _loggingBinder;
    private readonly IReadOnlyList<IPluginProvider> _pluginProviders;
    private readonly IPluginSelector _pluginSelector;
    private Optional<IRootExceptionHandler> _rootExceptionHandler;
    
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
        _loggingBinder = ILoggingBinder.Create(applicationId);
        _rootExceptionHandler = Optional.None<IRootExceptionHandler>();
        _pluginProviders = pluginProviders;
        _pluginSelector = pluginSelector ?? new DelegatePluginSelector(t => true);
    }

    public async Task Run(string[] arguments,CancellationToken cancellationToken = default)
    {
        try
        {
            var builder = WebApplication.CreateBuilder();
            var application = OnConfigure(builder,out var pluginBinder);
            
            _rootExceptionHandler = Optional.Some(application.Services.GetRequiredService<IRootExceptionHandler>());
    
            OnConfigure(application,pluginBinder);

            await application.RunAsync();
        }
        catch (Exception exception)
        {
            var wasHandled = false;
            
            if (_rootExceptionHandler)
            {
                wasHandled = _rootExceptionHandler.Value.Handle(exception);
            }

            if (!wasHandled)
            {
                _loggingBinder.Logger.Fatal(exception,
                    exception.Message);
            }
        }
    }

    protected virtual void OnConfigure(WebApplication application,IWebPluginBinder pluginBinder)
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

    protected virtual WebApplication OnConfigure(WebApplicationBuilder builder,out IWebPluginBinder pluginBinder)
    {
        var configuration = builder.CreateDefaultConfiguration(_applicationId);
    
        _loggingBinder.Apply(configuration,builder.Services);

        var localPluginBinder = builder.AddPlugins(_pluginProviders, _pluginSelector);

        builder.Services.AddSingleton<IRootExceptionHandler, RootExceptionHandler>();
        
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