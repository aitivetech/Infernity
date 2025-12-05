using Microsoft.Extensions.Configuration;

using Serilog;
using Serilog.Events;

namespace Infernity.Framework.Logging;

public sealed class DefaultLoggingBinder : LoggingBinder
{
    public DefaultLoggingBinder(string applicationId,
        LogEventLevel minimumLevel) : base(applicationId,
        minimumLevel)
    {
    }

    protected override void OnConfigureFinalLogger(IServiceProvider serviceProvider,
        IConfiguration configuration,
        LoggerConfiguration loggerConfiguration,
        LogEventLevel minimumLevel)
    {
        ApplyLoggerDefaults(loggerConfiguration,
            minimumLevel);
    }
}