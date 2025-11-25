using System.Reflection;

using FastEndpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Infernity.Framework.Plugins.Web;

public interface IWebPluginBinder : IPluginBinder
{
    IReadOnlySet<Assembly> BlazorAssemblies { get; } 
    
    void Configure(IEndpointRouteBuilder routeBuilder);
    
    void Configure(ApplicationPartManager partManager);
    void Configure(EndpointDiscoveryOptions options);
}