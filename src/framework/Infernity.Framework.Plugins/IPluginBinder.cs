using System.Reflection;

using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins;

public interface IPluginBinder
{
    IReadOnlyDictionary<PluginId, IPlugin> ActivePlugins { get; }
    
    IReadOnlySet<Assembly> Assemblies { get; }
    
    void Bind();
}