using System.Reflection;

namespace Infernity.Framework.Plugins;


public interface IPlugin
{
    PluginId Id => Description.Id;
    
    PluginDescription Description { get; }
    
    IReadOnlySet<Assembly> Assemblies { get; }
}