using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins;

public interface IPluginProvider
{
    IReadOnlyDictionary<PluginId, PluginDescription> Descriptions { get; }
    
    IPlugin Load(IHostEnvironment environment,PluginId id,IPluginActivator activator);
}