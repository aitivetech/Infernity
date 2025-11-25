using System.Reflection;

using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins.Host;

public abstract class HostPluginActivator<TBinder> : IPluginActivator<TBinder> where TBinder : IPluginBinder
{
    public TBinder Activate(IHostApplicationBuilder applicationBuilder,
        IReadOnlyDictionary<PluginId, IPlugin> plugins)
    {
        return OnActivate(
            applicationBuilder,
            plugins,
            plugins.SelectMany(p => p.Value.Assemblies).ToHashSet());
    }

    protected abstract TBinder OnActivate(IHostApplicationBuilder applicationBuilder,
        IReadOnlyDictionary<PluginId, IPlugin> plugins,
        IReadOnlySet<Assembly> assemblies);
}

public sealed class HostPluginActivator : HostPluginActivator<IPluginBinder>
{
    protected override IPluginBinder OnActivate(IHostApplicationBuilder applicationBuilder,
        IReadOnlyDictionary<PluginId, IPlugin> plugins,
        IReadOnlySet<Assembly> assemblies)
    {
        return new HostPluginBinder(applicationBuilder, plugins, assemblies);
    }
}