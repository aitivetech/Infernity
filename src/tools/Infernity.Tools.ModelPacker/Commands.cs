using CommandDotNet;

using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Patterns;
using Infernity.Inference.Abstractions;
using Infernity.Inference.Packaging.Builder;

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

    [Command(Description =
        "Takes an input model directory, packs it, and produces a manifest and nuspec file to be edited")]
    public async Task Init(IConsole console,
        DirectoryInfo inputDirectory,
        DirectoryInfo outputDirectory,
        int compressionLevel = 22,
        // Enable on new release of CommandDotNet
        //[DescriptionMethod(nameof(GetAvailableInferenceProviders))]
        InferenceProviderId? provider = null,
        CancellationToken cancellationToken = default)
    {
        var builder = _serviceProvider.GetRequiredService<ModelPackageBuilder>();
        await builder.Initialize(inputDirectory,
            outputDirectory,
            console.Out,
            provider.ToOptional(),
            compressionLevel,
            cancellationToken);
    }

    [Command(Description =
        "Takes an input directory containing a model manifest and nuspec files and create the final package")]
    public async Task Pack(DirectoryInfo inputDirectory,
        DirectoryInfo outputDirectory)
    {
    }

    public static string GetAvailableInferenceProviders()
    {
        return "Available inference providers: " + string.Join(", ",
            GlobalsRegistry.Resolve<IEnumerable<IInferenceProviderFactory>>().Select(i => i.Id.Value));
    }
}