using System.Reflection;

using Infernity.Framework.Core.Reflection;
using Infernity.Framework.Plugins.Host;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins.Web;

public sealed class WebPluginActivator : HostPluginActivator<IWebPluginBinder>
{
    protected override IWebPluginBinder OnActivate(IHostApplicationBuilder applicationBuilder,
        IReadOnlyDictionary<PluginId, IPlugin> plugins,
        IReadOnlySet<Assembly> assemblies)
    {
        return new WebPluginBinder(applicationBuilder, plugins, assemblies);
    }
}