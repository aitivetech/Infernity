using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins;

public interface IPluginManager<TBinder>
    where TBinder : IPluginBinder
{
    TBinder Build();
    
    static IPluginManager<TBinder> Create(
        IHostApplicationBuilder applicationBuilder,
        IReadOnlyList<IPluginProvider> providers,
        IPluginActivator<TBinder> activator,
        IPluginSelector selector)
    {
        return new PluginManager<TBinder>(applicationBuilder,providers, activator, selector);
    }
}