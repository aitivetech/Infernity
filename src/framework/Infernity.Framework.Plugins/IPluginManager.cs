using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins;

public interface IPluginManager
{
    IReadOnlyDictionary<PluginId,IPlugin> ActivePlugins { get; }

    static IPluginManager Create(
        IHostEnvironment environment,
        IReadOnlyList<IPluginProvider> providers,
        IPluginActivator activator,
        IPluginSelector selector)
    {
        return new PluginManager(environment,providers, activator, selector);
    }
}