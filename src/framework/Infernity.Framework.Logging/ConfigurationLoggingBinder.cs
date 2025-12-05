using Microsoft.Extensions.Configuration;

using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Settings.Configuration;

namespace Infernity.Framework.Logging;

public sealed class ConfigurationLoggingBinder : LoggingBinder
{
    public ConfigurationLoggingBinder(string applicationId,
        LogEventLevel minimumLevel) : base(applicationId,
        minimumLevel)
    {
        
    }


    protected override void OnConfigureFinalLogger(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        LoggerConfiguration lc,
        LogEventLevel minimumLevel)
    {
        lc.ReadFrom.Services(serviceProvider)
            .ReadFrom.Configuration(configuration,
                new ConfigurationReaderOptions() { SectionName = "Logging" })
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails();
    }
}