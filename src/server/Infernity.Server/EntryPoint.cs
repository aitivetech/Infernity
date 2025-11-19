using System.Reflection;

using Infernity.Framework.Core;
using Infernity.Framework.Plugins.Providers;
using Infernity.Framework.Web;

var builtInPluginProvider = new BuiltinPluginProvider([Assembly.GetExecutingAssembly()]);

using var host = new WebApplicationHost(ApplicationInfo.Id,
    [builtInPluginProvider]);

await host.Run(args,CancellationToken.None);