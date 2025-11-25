using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins.Host;

public interface IHostPlugin
{
    void ConfigureHost(IHostApplicationBuilder applicationBuilder);
}