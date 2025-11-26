using System.Reflection;

using Infernity.Framework.Cli;
using Infernity.Framework.Core;
using Infernity.Framework.Plugins.Providers;
using Infernity.Tools.ModelPacker;

var builtInPluginProvider = new BuiltinPluginProvider([Assembly.GetExecutingAssembly()]);


using var host = new CliApplicationHost<RootCommands>(ApplicationInfo.Id,[builtInPluginProvider]);

