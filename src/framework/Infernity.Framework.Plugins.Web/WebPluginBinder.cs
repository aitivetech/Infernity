using System.Reflection;

using FastEndpoints;

using Infernity.Framework.Core.Collections;
using Infernity.Framework.Core.Reflection;
using Infernity.Framework.Plugins.Host;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins.Web;

internal sealed class WebPluginBinder : HostPluginBinder, IWebPluginBinder
{
    internal WebPluginBinder(IHostApplicationBuilder applicationBuilder,
        IReadOnlyDictionary<PluginId, IPlugin> activePlugins,
        IReadOnlySet<Assembly> assemblies) : base(applicationBuilder,
        activePlugins,
        assemblies)
    {
    }

    public IReadOnlySet<Assembly> BlazorAssemblies
    {
        get
        {
            return FilterAssemblies(a => a.EnableBlazor);
        }
    }

    public void Configure(IEndpointRouteBuilder routeBuilder)
    {
        foreach (var webPlugin in Assemblies.SelectMany(a => a.CreateInstancesImplementing<IWebPlugin>())
                     .OrderOrderableOptionally())
        {
            webPlugin.Configure(routeBuilder);
        }
    }

    public void Configure(ApplicationPartManager partManager)
    {
        foreach (var assembly in FilterAssemblies(a => a.EnableWebParts))
        {
            var partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
            foreach (var applicationPart in partFactory.GetApplicationParts(assembly))
            {
                partManager.ApplicationParts.Add(applicationPart);
            }
        }
    }

    public void Configure(EndpointDiscoveryOptions options)
    {
        options.DisableAutoDiscovery = true;
        options.Assemblies = FilterAssemblies(w => w.EnableEndpoints);
    }

    private IReadOnlySet<Assembly> FilterAssemblies(Func<WebPluginAttribute,bool> predicate)
    {
        return Assemblies.Where(a =>
        {
            var attribute = a.GetCustomAttribute<WebPluginAttribute>();

            if (attribute != null)
            {
                return predicate(attribute);
            }

            return false;
        }).ToHashSet();
    }
}