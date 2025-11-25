using System.Reflection;

using Infernity.Framework.Configuration;
using Infernity.Framework.Core.Collections;
using Infernity.Framework.Core.Reflection;

using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins.Host;

public class HostPluginBinder : IPluginBinder
{
    public HostPluginBinder(
        IHostApplicationBuilder applicationBuilder,
        IReadOnlyDictionary<PluginId, IPlugin> activePlugins,
        IReadOnlySet<Assembly> assemblies)
    {
        ActivePlugins = activePlugins;
        Assemblies = assemblies;
        ApplicationBuilder = applicationBuilder;
    }


    public IReadOnlyDictionary<PluginId, IPlugin> ActivePlugins { get; }
    public IReadOnlySet<Assembly> Assemblies { get; }

    protected IHostApplicationBuilder ApplicationBuilder { get; }

    public void Bind()
    {
        OnBind();
    }

    protected virtual void OnBind()
    {
        foreach (var plugin in Assemblies.SelectMany(a => a.CreateInstancesImplementing<IHostPlugin>())
                     .OrderOrderableOptionally())
        {
            plugin.ConfigureHost(ApplicationBuilder);
        }

        foreach (var assembly in Assemblies)
        {
            ApplicationBuilder.Services.AddAllConfigurationSections(assembly);
        }
    }
}