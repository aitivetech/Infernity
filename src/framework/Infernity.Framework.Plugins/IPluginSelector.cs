using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins;

public interface IPluginSelector
{
    IReadOnlySet<PluginId> SelectPluginsToLoad(IHostApplicationBuilder applicationBuilder,
        IReadOnlyList<PluginDescription> descriptions);
}