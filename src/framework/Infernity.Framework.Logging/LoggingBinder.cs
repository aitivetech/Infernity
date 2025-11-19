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

internal sealed class LoggingBinder : Disposable,ILoggingBinder
{
    private readonly string _applicationId;

    internal LoggingBinder(string applicationId)
    {
        _applicationId = applicationId;
        Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .WriteTo.Console(new RenderedCompactJsonFormatter())
            .CreateBootstrapLogger();
        Log.Logger = Logger;
        
        GlobalsRegistry.Register<ILoggerFactory>(new SerilogLoggerFactory(Logger));
    }
    
    public ILogger Logger { get; }

    public void Apply(
        IConfiguration configuration,
        IServiceCollection services)
    {
        services.AddSerilog((sp,lc) =>
        {
            lc.ReadFrom.Services(sp)
                .ReadFrom.Configuration(configuration,new ConfigurationReaderOptions() { SectionName =  "Logging" })
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console(new RenderedCompactJsonFormatter());
            
            GlobalsRegistry.Remove<ILoggerFactory>();
        });
    }
    
    protected override void OnDispose()
    {
        Log.CloseAndFlush();
    }
}