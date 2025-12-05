using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog.Events;
using Serilog.Exceptions;

using ILogger = Serilog.ILogger;

namespace Infernity.Framework.Logging;

public interface ILoggingBinder : IDisposable
{
    ILogger Logger { get; }
    
    void Apply(IConfiguration configuration,IServiceCollection service);
}