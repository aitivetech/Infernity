using System.Reflection;

using Infernity.Framework.Core.Reflection;
using Infernity.Framework.Plugins.Activators;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins.Web;

internal class WebPluginActivator : ServicePluginActivator
{
    private readonly List<IWebPlugin> _webPlugins = new();
    private readonly HashSet<Assembly> _webPartAssemblies = new();
    private readonly HashSet<Assembly> _blazorAssemblies = new();
    private readonly HashSet<Assembly> _endpointsAssemblies = new();
 
    private bool _wasBuilt = false;

    internal WebPluginActivator(IServiceCollection serviceServices)
        : base(serviceServices)
    {
    }

    public override void OnActivate(IHostEnvironment environment,
        Assembly assembly)
    {
        base.OnActivate(environment, assembly);
        
        // Handles explict implementations of IWebPlugin, not IServicePlugin that additionally implements it. Allows us to skip double construction.
        foreach(var webPlugin in assembly.CreateInstancesImplementing<IWebPlugin>(_webPlugins.Select(p => p.GetType()).ToHashSet()))
        {
            _webPlugins.Add(webPlugin);
        }
        
        var attribute = assembly.GetCustomAttribute<WebPluginAttribute>();
        
        if (attribute?.EnableEndpoints ?? true)
        {
            _endpointsAssemblies.Add(assembly);
        }

        if (attribute?.EnableWebParts ?? true)
        {
            _webPartAssemblies.Add(assembly);
        }

        if (attribute?.EnableBlazor ?? true)
        {
            _blazorAssemblies.Add(assembly);
        }
    }

    public IWebPluginBinder Build()
    {
        if (_wasBuilt)
        {
            throw new InvalidOperationException("Web plugin binder was already built");
        }
        
        _wasBuilt = true;
        
        var result = new WebPluginBinder(_webPlugins, _webPartAssemblies, _blazorAssemblies, _endpointsAssemblies);

        Services.AddSingleton<IWebPluginBinder>(result);
        
        return result;
    }
    
    protected override void OnPostRegisterServices(
        IHostEnvironment environment,
        Assembly assembly,
        IServiceCollection serviceCollection,
        IServicePlugin servicePlugin)
    {
        // This allows us to not double construct plugins implementing multiple contracts.
        if (servicePlugin is IWebPlugin webPlugin)
        {
            _webPlugins.Add(webPlugin);
        }
    }
}