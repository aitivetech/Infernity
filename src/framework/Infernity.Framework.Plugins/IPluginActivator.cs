using System.Reflection;

using Infernity.Framework.Core.Functional;

using Microsoft.Extensions.Hosting;

namespace Infernity.Framework.Plugins;

public interface IPluginActivator 
{
    void OnActivate(IHostEnvironment environment,Assembly assembly);
}