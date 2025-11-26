using System.Text.Json;
using System.Text.Json.Serialization;

using Infernity.Framework.Configuration;
using Infernity.Framework.Core.Exceptions;
using Infernity.Framework.Core.Exceptions.Default;
using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Patterns.Disposal;
using Infernity.Framework.Core.Startup;
using Infernity.Framework.Json;
using Infernity.Framework.Logging;
using Infernity.Framework.Plugins.Selectors;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog.Events;

namespace Infernity.Framework.Plugins;

public abstract class PluginApplicationHost<TBinder> : Disposable
    where TBinder : IPluginBinder
{
    private readonly ILoggingBinder _loggingBinder;
    private readonly IReadOnlyList<IPluginProvider> _pluginProviders;
    private readonly IPluginActivator<TBinder> _pluginActivator;
    private readonly IPluginSelector _pluginSelector;
    private Optional<IExceptionHandler> _exceptionHandler;

    protected PluginApplicationHost(
        string applicationId,
        IHostApplicationBuilder builder,
        IReadOnlyList<IPluginProvider> pluginProviders,
        IPluginActivator<TBinder> pluginActivator,
        IPluginSelector? pluginSelector = null,
        bool useConfigurationFiles = true)
    {
        ApplicationId = applicationId;
        Builder = builder;

        _loggingBinder = ILoggingBinder.Create(applicationId,
            Builder.Environment.IsDevelopment() ? LogEventLevel.Debug : LogEventLevel.Information);

        Configuration = Builder.CreateDefaultConfiguration(applicationId,
            useConfigurationFiles);
        _exceptionHandler = Optional.None<IExceptionHandler>();
        _pluginProviders = pluginProviders;
        _pluginActivator = pluginActivator;
        _pluginSelector = pluginSelector ?? new DelegatePluginSelector(t => true);
    }

    protected string ApplicationId { get; }

    protected IHostApplicationBuilder Builder { get; }

    protected IConfiguration Configuration { get; }

    public async Task Run(string[] arguments,
        CancellationToken cancellationToken)
    {
        try
        {
            var pluginBinder = OnConfigureHostBuilder(Builder,
                Configuration);

            using var host = OnBuildHost(Builder);

            OnConfigureHost(host,
                Configuration,
                pluginBinder);

            _exceptionHandler = Optional.Some(host.Services.GetRequiredService<IExceptionHandler>());

            await OnRunHost(host,
                arguments,
                cancellationToken);
        }
        catch (Exception exception)
        {
            OnException(exception);
        }
    }

    protected virtual async Task OnRunHost(
        IHost host,
        string[] arguments,
        CancellationToken cancellationToken)
    {
        await host.Services.ExecuteStartupTasks();
        await host.RunAsync(cancellationToken);
    }

    protected virtual TBinder OnConfigureHostBuilder(
        IHostApplicationBuilder builder,
        IConfiguration configuration)
    {
        _loggingBinder.Apply(configuration,
            builder.Services);

        var pluginManager = IPluginManager<TBinder>.Create(builder,
            _pluginProviders,
            _pluginActivator,
            _pluginSelector);

        builder.Services.AddSingleton(pluginManager);

        var pluginBinder = pluginManager.Build();
        pluginBinder.Bind();

        builder.Services.AddSingleton<IExceptionHandler, DefaultExceptionHandler>();

        builder.Services.AddSingleton<JsonSerializerOptions>(provider =>
            JsonSerializerOptions.CreateDefault(provider.GetServices<JsonConverter>()));

        return pluginBinder;
    }

    protected virtual void OnConfigureHost(
        IHost host,
        IConfiguration configuration,
        TBinder binder)
    {
    }

    protected abstract IHost OnBuildHost(IHostApplicationBuilder builder);

    protected virtual void OnException(Exception exception)
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

    protected override void OnDispose()
    {
        _loggingBinder.Dispose();
    }
}