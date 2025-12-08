using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

using Infernity.Framework.Configuration;
using Infernity.Framework.Core.Exceptions;
using Infernity.Framework.Core.Exceptions.Default;
using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Io.Paths;
using Infernity.Framework.Core.Patterns;
using Infernity.Framework.Core.Patterns.Disposal;
using Infernity.Framework.Core.Startup;
using Infernity.Framework.Core.Threading;
using Infernity.Framework.Json;
using Infernity.Framework.Json.Converters;
using Infernity.Framework.Logging;
using Infernity.Framework.Plugins.Selectors;
using Infernity.Framework.Security.Hashing;
using Infernity.Framework.Security.Signatures;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IO;

using Semver;

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
        Func<string,LogEventLevel,ILoggingBinder> loggingBinderFactory,
        IPluginSelector? pluginSelector = null,
        bool useConfigurationFiles = true)
    {
        ApplicationId = applicationId;
        Builder = builder;

        _loggingBinder = loggingBinderFactory(applicationId,
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
            GlobalsRegistry.Register<IServiceProvider>(host.Services);
            
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

        OnRegisterSystemServices(builder,
            builder.Services);

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

    protected virtual void OnRegisterSystemServices(IHostApplicationBuilder builder,
        IServiceCollection services)
    {
        services.AddSingleton<IExceptionHandler, DefaultExceptionHandler>();

        services.AddSingleton<JsonSerializerOptions>(provider =>
            JsonSerializerOptions.CreateDefault(provider.GetServices<JsonConverter>()));

        services.AddSingleton<RecyclableMemoryStreamManager>(sp => new RecyclableMemoryStreamManager());
        services.AddSingleton<IHashAlgorithm<Sha256Value>, Sha256ClrHashAlgorithm>();
        services.AddSingleton<IHashAlgorithm<Sha1Value>, Sha1ClrHashAlgorithm>();

        services.AddSingleton<IHashProvider<Sha256Value>, HashProvider<Sha256Value>>();
        services.AddSingleton<IHashProvider<Sha1Value>, HashProvider<Sha1Value>>();

        OnRegisterJsonConverters(services);
    }

    protected virtual void OnRegisterJsonConverters(IServiceCollection services)
    {
        services.AddSingleton<JsonConverter>(new StringJsonConverter<Sha256Value>());
        services.AddSingleton<JsonConverter>(new StringJsonConverter<Sha1Value>());
        services.AddSingleton<JsonConverter>(new OptionalJsonConverterFactory());
        services.AddSingleton<JsonConverter>(new ErrorJsonConverterFactory());
        services.AddSingleton<JsonConverter>(new MimeTypeJsonConverter());
        services.AddSingleton<JsonConverter>(new ResultJsonConverterFactory());
        services.AddSingleton<JsonConverter>(new FlagsEnumArrayJsonConverterFactory());
        services.AddSingleton<JsonConverter>(new DelegateStringProxyJsonConverter<DirectoryInfo>(
            d => new DirectoryInfo(d),
            d => d.FullName));

        services.AddSingleton<JsonConverter>(new DelegateStringProxyJsonConverter<FileInfo>(
            d => new FileInfo(d),
            d => d.FullName));
        
        services.AddSingleton<JsonConverter>(new DelegateStringProxyJsonConverter<CultureInfo>(d =>  new CultureInfo(d),c => c.Name));
        services.AddSingleton<JsonConverter>(new DelegateStringProxyJsonConverter<RegionInfo>(d => new RegionInfo(d),d => d.TwoLetterISORegionName.ToLowerInvariant()));

        services.AddSingleton<JsonConverter>(new DelegateStringProxyJsonConverter<Semver.SemVersion>(d => SemVersion.Parse(d,SemVersionStyles.Any),d => d.ToString()));
        services.AddSingleton<JsonConverter>(new DelegateStringProxyJsonConverter<SemVersionRange>(d => SemVersionRange.Parse(d,SemVersionRangeOptions.Loose),d => d.ToString()));
        
        services.AddSingleton<JsonConverter>(new DelegateStringProxyJsonConverter<PurePosixPath>(d =>  new PurePosixPath(d),d => d.ToPosix()));

        services.AddSingleton<JsonConverter>(new SignatureJsonConverter());
        services.AddSingleton<JsonConverter>(new StringJsonConverter<ConcurrencyToken>());
    }
}