using System.Reflection;

using FastEndpoints;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace Infernity.Framework.Plugins.Web;

public interface IWebPluginBinder
{
    IReadOnlySet<Assembly> BlazorAssemblies { get; } 
    
    void Configure(WebApplication application);
    
    void Configure(ApplicationPartManager partManager);
    void Configure(EndpointDiscoveryOptions options);
}