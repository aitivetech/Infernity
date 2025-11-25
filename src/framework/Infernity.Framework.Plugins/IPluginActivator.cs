using System.Reflection;

using Infernity.Framework.Core.Functional;

using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins;

public interface IPluginActivator<out TBinder>
    where TBinder : IPluginBinder
{
    TBinder Activate(IHostApplicationBuilder applicationBuilder,
        IReadOnlyDictionary<PluginId, IPlugin> plugins);
}