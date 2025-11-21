using Infernity.GeneratedCode;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infernity.Framework.Plugins;

[AddLogger]
internal sealed partial class PluginManager : IPluginManager
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IReadOnlyList<IPluginProvider> _pluginProviders;
    private readonly IPluginActivator _pluginActivator;
    private readonly IPluginSelector _pluginSelector;

    public PluginManager(
        IHostEnvironment environment,
        IReadOnlyList<IPluginProvider> providers,
        IPluginActivator activator,
        IPluginSelector selector)
    {
        _hostEnvironment = environment;
        _pluginProviders = providers;
        _pluginActivator = activator;
        _pluginSelector = selector;

        var pluginProviderMapping =
            providers.SelectMany(p => p.Descriptions.Values.Select(v => (p, v))).ToDictionary(v => v.v.Id,
                v => v.p);

        var pluginDescriptions = providers.SelectMany(p => p.Descriptions.Values).ToList();

        foreach (var pluginDescription in pluginDescriptions)
        {
            LogAvailablePlugin(Logger, pluginDescription);
        }
        
        var pluginsToLoad = selector.SelectPluginsToLoad(pluginDescriptions);
        
        var loadedPlugins = new Dictionary<PluginId, IPlugin>();

        foreach (var pluginToLoad in pluginsToLoad)
        {
            LogLoadingPlugin(Logger, pluginToLoad);
            
            var provider = pluginProviderMapping[pluginToLoad];
            
            var plugin = provider.Load(_hostEnvironment,pluginToLoad,activator);
            
            loadedPlugins.Add(pluginToLoad,plugin);
        }
        
        ActivePlugins = loadedPlugins;
    }

    public IReadOnlyDictionary<PluginId, IPlugin> ActivePlugins { get; }
    
    
    [LoggerMessage(LogLevel.Debug, "Available plugin: {plugin}")]
    private static partial void LogAvailablePlugin(ILogger<PluginManager> logger,
        PluginDescription plugin);

    [LoggerMessage(LogLevel.Debug, "Loading plugin: {plugin}")]
    private static partial void LogLoadingPlugin(ILogger<PluginManager> logger,
        PluginId plugin);
}