using System.Reflection;

using Infernity.Framework.Cli;
using Infernity.Framework.Core;
using Infernity.Framework.Plugins.Providers;
using Infernity.Inference.Abstractions;
using Infernity.Inference.Packaging;
using Infernity.Inference.Packaging.Nuget;
using Infernity.Inference.Providers.Llama;
using Infernity.Tools.ModelPacker;

var builtInPluginProvider = new BuiltinPluginProvider([
    Assembly.GetExecutingAssembly(), typeof(IInferenceProvider).Assembly, typeof(LLamaModelManifestHandler).Assembly,
    typeof(ModelPackageBuilder).Assembly
]);

using var host = new CliApplicationHost<RootCommands>(ApplicationSuiteInfo.Id,
    [builtInPluginProvider],
    ((host,
        builder) => builder.AppSettings.Help.UsageAppName = ApplicationInfo.Name));

await host.Run(args,
    CancellationToken.None);