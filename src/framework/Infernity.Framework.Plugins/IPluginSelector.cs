namespace Infernity.Framework.Plugins;

public interface IPluginSelector
{
    IReadOnlySet<PluginId> SelectPluginsToLoad(IReadOnlyList<PluginDescription> descriptions);
}