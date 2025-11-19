namespace Infernity.Framework.Plugins.Selectors;

public sealed class DelegatePluginSelector(
    Func<PluginDescription, bool> predicate) : IPluginSelector
{
    public IReadOnlySet<PluginId> SelectPluginsToLoad(IReadOnlyList<PluginDescription> descriptions)
    {
        return descriptions.Where(predicate).Select(d => d.Id).ToHashSet();
    }
}