using Infernity.GeneratedCode;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infernity.Framework.Plugins;

[AddLogger]
internal sealed partial class PluginManager<TBinder> : IPluginManager<TBinder>
    where TBinder : IPluginBinder
{
    private readonly IHostApplicationBuilder _applicationBuilder;
    private readonly IReadOnlyList<IPluginProvider> _pluginProviders;
    private readonly IPluginActivator<TBinder> _pluginActivator;
    private readonly IPluginSelector _pluginSelector;

    internal PluginManager(
        IHostApplicationBuilder applicationBuilder,
        IReadOnlyList<IPluginProvider> providers,
        IPluginActivator<TBinder> activator,
        IPluginSelector selector)
    {
        _applicationBuilder = applicationBuilder;
        _pluginProviders = providers;
        _pluginActivator = activator;
        _pluginSelector = selector;
    }

    public TBinder Build()
    {
        var pluginProviderMapping =
            _pluginProviders.SelectMany(p => p.Descriptions.Values.Select(v => (p, v))).ToDictionary(v => v.v.Id,
                v => v.p);

        var pluginDescriptions = _pluginProviders.SelectMany(p => p.Descriptions.Values).ToDictionary(v => v.Id);

        foreach (var pluginDescription in pluginDescriptions)
        {
            LogAvailablePlugin(Logger,
                pluginDescription.Value);
        }

        var pluginsToLoad = _pluginSelector.SelectPluginsToLoad(_applicationBuilder,
            pluginDescriptions.Select(v => v.Value).ToList());
        var loadedPlugins = new Dictionary<PluginId, IPlugin>();

        foreach (var pluginToLoad in pluginsToLoad)
        {
            LogLoadingPlugin(Logger,
                pluginToLoad);

            var provider = pluginProviderMapping[pluginToLoad];
            var description = pluginDescriptions[pluginToLoad];

            var plugin = provider.Load(_applicationBuilder,
                description);

            loadedPlugins.Add(pluginToLoad,
                plugin);
        }

        return _pluginActivator.Activate(_applicationBuilder,
            loadedPlugins);
    }

    [LoggerMessage(LogLevel.Debug,
        "Available plugin: {plugin}")]
    private static partial void LogAvailablePlugin(ILogger<PluginManager<TBinder>> logger,
        PluginDescription plugin);

    [LoggerMessage(LogLevel.Debug,
        "Loading plugin: {plugin}")]
    private static partial void LogLoadingPlugin(ILogger<PluginManager<TBinder>> logger,
        PluginId plugin);
}