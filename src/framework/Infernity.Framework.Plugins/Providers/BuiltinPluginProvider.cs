using System.Reflection;

using Infernity.Framework.Core.Collections;

using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins.Providers;

public sealed class BuiltinPluginProvider : IPluginProvider
{
    private readonly IReadOnlyDictionary<PluginId, Assembly> _pluginAssemblies;

    private sealed class Plugin : IPlugin
    {
        internal Plugin(PluginDescription description,
            Assembly assembly)
        {
            Description = description;
            Assemblies = new HashSet<Assembly>() { assembly };
        }

        public PluginDescription Description { get; }

        public IReadOnlySet<Assembly> Assemblies { get; }
    }

    public BuiltinPluginProvider(IReadOnlyList<Assembly> assemblies)
    {
        var pluginDescriptions = new Dictionary<PluginId, PluginDescription>();
        var pluginAssemblies = new Dictionary<PluginId, Assembly>();

        foreach (var assembly in assemblies.Distinct())
        {
            var pluginDescription = GetPluginDescription(assembly);

            pluginDescriptions.Add(pluginDescription.Id,
                pluginDescription);
            pluginAssemblies.Add(pluginDescription.Id,
                assembly);
        }

        Descriptions = pluginDescriptions;
        _pluginAssemblies = pluginAssemblies;
    }

    public IReadOnlyDictionary<PluginId, PluginDescription> Descriptions { get; }

    public IPlugin Load(
        IHostApplicationBuilder applicationBuilder,
        PluginDescription pluginDescription)

    {
        var assembly = _pluginAssemblies.GetOptional(pluginDescription.Id)
            .OrThrow(() => new PluginException($"Unknown plugin {pluginDescription.Id}"));
        var description = Descriptions.GetOptional(pluginDescription.Id)
            .OrThrow(() => new PluginException($"Unknown plugin {pluginDescription.Id}"));

        return new Plugin(description,
            assembly);
    }

    private PluginDescription GetPluginDescription(Assembly assembly)
    {
        var id = assembly.GetName().Name ??
                 throw new NotSupportedException(
                     $"Assembly has no name, and cannot be used as plugin assembly: {assembly}");

        var versionAttribute = assembly.GetCustomAttribute<AssemblyVersionAttribute>();

        var versionString = versionAttribute?.Version ?? "0.0.1";

        return new PluginDescription(id,
            versionString,
            true);
    }
}