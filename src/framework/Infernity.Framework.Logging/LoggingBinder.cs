using Infernity.Framework.Core.Patterns;
using Infernity.Framework.Core.Patterns.Disposal;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Compact;
using Serilog.Settings.Configuration;

using ILogger = Serilog.ILogger;

namespace Infernity.Framework.Logging;

public abstract class LoggingBinder : Disposable, ILoggingBinder
{
    private readonly string _applicationId;
    private readonly LogEventLevel _minimumLevel;

    internal LoggingBinder(string applicationId,
        LogEventLevel minimumLevel)
    {
        _applicationId = applicationId;
        _minimumLevel = minimumLevel;

        var baseConfiguration = new LoggerConfiguration();
        ApplyLoggerDefaults(baseConfiguration,
            minimumLevel);
        Logger = baseConfiguration.CreateBootstrapLogger();
        Log.Logger = Logger;

        GlobalsRegistry.Register<ILoggerFactory>(new SerilogLoggerFactory(Logger));
    }

    public ILogger Logger { get; }

    public void Apply(
        IConfiguration configuration,
        IServiceCollection services)
    {
        services.AddSerilog((sp,
            lc) =>
        {
            OnConfigureFinalLogger(sp,
                configuration,
                lc,
                _minimumLevel);

            GlobalsRegistry.Remove<ILoggerFactory>();
        });
    }

    protected override void OnDispose()
    {
        Log.CloseAndFlush();
    }

    protected abstract void OnConfigureFinalLogger(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        LoggerConfiguration loggerConfiguration,
        LogEventLevel minimumLevel);

    protected static void ApplyLoggerDefaults(LoggerConfiguration loggerConfiguration,
        LogEventLevel minimumLevel)
    {
        loggerConfiguration.MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft",
                LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .WriteTo.Console(new RenderedCompactJsonFormatter());
    }
}