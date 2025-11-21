using System.Collections.ObjectModel;
using System.Reflection;

using Infernity.Framework.Configuration;
using Infernity.Framework.Core.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins.Activators;

public class ServicePluginActivator : IPluginActivator
{
    public ServicePluginActivator(IServiceCollection serviceServices)
    {
        Services = serviceServices;
    }

    protected IServiceCollection Services { get; }
    
    public virtual void OnActivate(IHostEnvironment environment,Assembly assembly)
    {
        foreach (var servicePlugin in assembly.CreateInstancesImplementing<IServicePlugin>())
        {
            servicePlugin.RegisterServices(environment, Services);
        }

        Services.AddAllConfigurationSections(assembly);
    }

    protected virtual void OnPostRegisterServices(
        IHostEnvironment environment,
        Assembly assembly,
        IServiceCollection serviceCollection,
        IServicePlugin servicePlugin)
    {
    }
}