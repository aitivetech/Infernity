using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins.Activators;

public interface IServicePlugin
{
    void RegisterServices(IHostEnvironment environment, IServiceCollection services);
}