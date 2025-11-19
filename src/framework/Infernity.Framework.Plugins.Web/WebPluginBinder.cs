using System.Reflection;

using FastEndpoints;

using Infernity.Framework.Core.Collections;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace Infernity.Framework.Plugins.Web;

internal sealed class WebPluginBinder : IWebPluginBinder
{
    private readonly IReadOnlyList<IWebPlugin> _webPlugins;
    private readonly IReadOnlySet<Assembly> _webPartAssemblies;
    private readonly IReadOnlySet<Assembly> _endpointsAssemblies;

    internal WebPluginBinder(
        IReadOnlyList<IWebPlugin> webPlugins,
        IReadOnlySet<Assembly> webPartAssemblies,
        IReadOnlySet<Assembly> endpointsAssemblies,
        IReadOnlySet<Assembly> blazorAssemblies)
    {
        _webPlugins = webPlugins;
        _webPartAssemblies = webPartAssemblies;
        _endpointsAssemblies = endpointsAssemblies;
        BlazorAssemblies = blazorAssemblies;
    }

    public IReadOnlySet<Assembly> BlazorAssemblies { get; }
    
    public void Configure(WebApplication application)
    {
        foreach (var webPlugin in _webPlugins.OrderOrderableOptionally())
        {
            webPlugin.Configure(application);
        }
    }

    public void Configure(ApplicationPartManager partManager)
    {
        foreach (var assembly in _webPartAssemblies)
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
        options.Assemblies = _endpointsAssemblies;
    }
}