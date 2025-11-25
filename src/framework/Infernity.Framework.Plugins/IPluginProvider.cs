using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins;

public interface IPluginProvider
{
    IReadOnlyDictionary<PluginId, PluginDescription> Descriptions { get; }
    
    IPlugin Load(IHostApplicationBuilder applicationBuilder,PluginDescription pluginDescription);
}