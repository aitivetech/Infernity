using System.Reflection;

using Infernity.Framework.Core.Collections;

using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins.Providers;

public sealed class BuiltinPluginProvider : IPluginProvider
{
    private readonly IReadOnlyDictionary<PluginId, Assembly> _pluginAssemblies;

    private sealed class Plugin : IPlugin
    {
        internal Plugin(PluginDescription description)
        {
            Description = description;
        }

        public PluginDescription Description { get; }
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
        IHostEnvironment environment,
        PluginId id,
        IPluginActivator activator)

    {
        var assembly = _pluginAssemblies.GetOptional(id).OrThrow(() => new PluginException($"Unknown plugin {id}"));
        var description = Descriptions.GetOptional(id).OrThrow(() => new PluginException($"Unknown plugin {id}"));

        activator.OnActivate(environment,assembly);

        return new Plugin(description);
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