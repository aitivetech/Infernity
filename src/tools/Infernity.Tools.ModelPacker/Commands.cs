using CommandDotNet;

using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Patterns;
using Infernity.Inference.Abstractions;
using Infernity.Inference.Packaging;
using Infernity.Inference.Packaging.Nuget;

using Microsoft.Extensions.DependencyInjection;

using Spectre.Console;

namespace Infernity.Tools.ModelPacker;

public sealed class RootCommands
{
    private readonly IServiceProvider _serviceProvider;

    public RootCommands(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /**
init
--inputDirectory
/media/bglueck/Data/work/lm_studio/models/lmstudio-community/gpt-oss-20b-GGUF/
--outputDirectory
/media/bglueck/Data/temp/modelpacks/test1
     */
    [Command(Description =
        "Takes an input model directory, packs it, and produces a manifest and nuspec file to be edited")]
    public async Task Init(IConsole console,
        DirectoryInfo inputDirectory,
        DirectoryInfo outputDirectory,
        int compressionLevel = 7,
        // Enable on new release of CommandDotNet
        //[DescriptionMethod(nameof(GetAvailableInferenceProviders))]
        InferenceProviderId? provider = null,
        CancellationToken cancellationToken = default)
    {
        var builder = _serviceProvider.GetRequiredService<ModelPackageBuilder>();
        
        await builder.Initialize(
            inputDirectory,
            outputDirectory,
            console.Out,
            provider.ToOptional(),
            compressionLevel,
            cancellationToken);
    }

    [Command(Description =
        "Takes an input directory containing a model manifest create the final package")]
    public async Task Pack(
        IConsole console,
        DirectoryInfo inputDirectory,
        DirectoryInfo? outputDirectory = null,
        CancellationToken cancellationToken = default)
    {
        var builder = _serviceProvider.GetRequiredService<ModelPackageBuilder>();

        await builder.CreateModelPackage(inputDirectory,
            outputDirectory.NullableAsOptional(),
            console.Out,
            cancellationToken);
    }

    public static string GetAvailableInferenceProviders()
    {
        return "Available inference providers: " + string.Join(", ",
            GlobalsRegistry.Resolve<IEnumerable<IInferenceProviderFactory>>().Select(i => i.Id.Value));
    }
}