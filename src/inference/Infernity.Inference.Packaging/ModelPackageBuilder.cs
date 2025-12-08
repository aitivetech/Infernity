using Humanizer;

using Infernity.Framework.Compression.Archives;
using Infernity.Framework.Core;
using Infernity.Framework.Core.Collections;
using Infernity.Framework.Core.Content;
using Infernity.Framework.Core.Functional;
using Infernity.Framework.Security.Hashing;
using Infernity.Framework.Security.Io;
using Infernity.Inference.Abstractions;
using Infernity.Inference.Abstractions.Models;
using Infernity.Inference.Abstractions.Models.Manifest;

using NuGet.Packaging;
using NuGet.Versioning;

using static Infernity.Inference.Packaging.Nuget.NugetModelPackageSchema;

namespace Infernity.Inference.Packaging.Nuget;

public sealed record NugetModelPackageCreationResult(
    DirectoryMetadata<Sha256Value> Metadata,
    FileInfo File,
    Sha256Value FileHash);

public sealed class ModelPackageBuilder
{
    private readonly IHashProvider<Sha256Value> _hashProvider;
    private readonly IReadOnlyDictionary<InferenceProviderId, IInferenceProviderFactory> _inferenceProviderFactories;

    public ModelPackageBuilder(
        IHashProvider<Sha256Value> hashProvider,
        IEnumerable<IInferenceProviderFactory> inferenceProviderFactories)
    {
        _hashProvider = hashProvider;
        _inferenceProviderFactories = inferenceProviderFactories.ToDictionary(i => i.Id);
    }

    public async Task Initialize(DirectoryInfo inputDirectory,
        DirectoryInfo outputDirectory,
        TextWriter outputWriter,
        Optional<InferenceProviderId> inferenceProviderId,
        int compressionLevel = 22,
        CancellationToken cancellationToken = default)
    {
        CoerceInputAndOutputDirectory(inputDirectory,
            outputDirectory);

        var analysisResult = await CreateModelDataPackage(
            inputDirectory,
            outputDirectory,
            outputWriter,
            compressionLevel,
            cancellationToken);

        var manifestFile = await CreateManifest(inputDirectory,
            outputDirectory,
            outputWriter,
            inferenceProviderId,
            Optional<ModelIdentity>.None,
            analysisResult,
            cancellationToken);
    }

    public async Task<FileInfo> CreateManifest(
        Optional<DirectoryInfo> inputDirectory,
        DirectoryInfo outputDirectory,
        TextWriter outputWriter,
        Optional<InferenceProviderId> inferenceProviderId,
        Optional<ModelIdentity> modelInfo,
        Optional<NugetModelPackageCreationResult> modelDataPackageInfo,
        CancellationToken cancellationToken = default)

    {
        CoerceInputAndOutputDirectory(inputDirectory,
            outputDirectory);

        if (!inferenceProviderId.HasValue && !inputDirectory.HasValue)
        {
            throw new ModelPackageException($"Either inference provider or model directory must be set");
        }

        Optional<IInferenceProviderFactory> inferenceProviderFactory = Optional<IInferenceProviderFactory>.None;

        var finalModelInfo = modelInfo.Or(() => ModelIdentity.Unknown)!;

        if (inferenceProviderId)
        {
            inferenceProviderFactory =
                Optional.Some(GetInferenceProviderFactory(inferenceProviderId.Value));
        }

        if (inputDirectory)
        {
            inferenceProviderFactory =
                Optional.Some(DetectInferenceProvider(inputDirectory.Value));
        }

        await outputWriter.WriteLineAsync($"Packing for inference provider: {inferenceProviderFactory.Value.Id}");

        Optional<ModelManifest> modelManifest = Optional.None<ModelManifest>();

        if (inputDirectory)
        {
            await outputWriter.WriteLineAsync($"Analyzing model files");
            modelManifest = inferenceProviderFactory.Value.Analyzer.Analyze(inputDirectory.Value,
                finalModelInfo);
        }
        else
        {
            modelManifest = inferenceProviderFactory.Value.ManifestHandler.CreateDefault(finalModelInfo);
        }

        modelManifest.Value.AssignGeneratedId();

        if (modelManifest.Value is LocalModelManifest localModelManifest && modelDataPackageInfo.HasValue)
        {
            localModelManifest.Hash = modelDataPackageInfo.Value.Metadata.Hash;
            localModelManifest.CompressedHash = modelDataPackageInfo.Value.FileHash;
            localModelManifest.Size = modelDataPackageInfo.Value.Metadata.Size;
            localModelManifest.CompressedSize = modelDataPackageInfo.Value.Metadata.Size;
        }

        var outputFile = new FileInfo(Path.Combine(outputDirectory.FullName,
           ModelFileLayout.ManifestFileName));

        await modelManifest.Value.WriteAsync(outputFile.FullName);

        await outputWriter.WriteLineAsync($"Model manifest written to: {outputFile.FullName}");

        return outputFile;
    }

    public async Task<FileInfo> CreateModelPackage(
        DirectoryInfo inputDirectory,
        Optional<DirectoryInfo> outputDirectory,
        TextWriter outputWriter,
        CancellationToken cancellationToken = default)
    {
        var actualOutputDirectory = outputDirectory.Or(inputDirectory);

        CoerceInputAndOutputDirectory(inputDirectory,
            actualOutputDirectory);

        var modelManifestFile = new FileInfo(Path.Combine(inputDirectory.FullName,
            ModelFileLayout.ManifestFileName));

        if (!modelManifestFile.Exists)
        {
            throw new FileNotFoundException($"Model manifest not found: {modelManifestFile.FullName}");
        }

        var modelManifest = await ModelManifest.ReadAsync(modelManifestFile.FullName);

        var outputPackagePath = Path.Combine(actualOutputDirectory.FullName,
            $"{modelManifest.VersionedId}.nupkg");

        await using var outputStream = File.OpenWrite(outputPackagePath);

        var packageBuilder = new PackageBuilder { Id = modelManifest.Id };

        packageBuilder.PackageTypes.Add(NugetModelPackageSchema.PackageType);
        packageBuilder.Version = new NuGetVersion(modelManifest.Version.ToString());
        packageBuilder.Authors.Add(ApplicationSuiteInfo.CompanyName);
        packageBuilder.Copyright = ApplicationSuiteInfo.Copyright;
        packageBuilder.Description = modelManifest.Description.Or(modelManifest.Id);
        packageBuilder.Title = modelManifest.Id;
        packageBuilder.Tags.AddAll<string>(NugetPackageTags.Encode(modelManifest));

        packageBuilder.AddFiles(modelManifestFile.DirectoryName,
            modelManifestFile.FullName,
            ModelFileLayout.ManifestFileName);
        
        packageBuilder.Save(outputStream);
        
        await outputWriter.WriteLineAsync($"Model package written to: {outputPackagePath}");

        return new FileInfo(outputPackagePath);
    }
    
    public async Task<NugetModelPackageCreationResult> CreateModelDataPackage(
        DirectoryInfo inputDirectory,
        DirectoryInfo outputDirectory,
        TextWriter outputWriter,
        int compressionLevel = 22,
        CancellationToken cancellationToken = default)
    {
        CoerceInputAndOutputDirectory(inputDirectory,
            outputDirectory);

        await outputWriter.WriteLineAsync($"Analyzing model files");

        var metadata = inputDirectory.CalculateHashAndMetadata(_hashProvider);

        await outputWriter.WriteLineAsync($"Model content {metadata}");
        await outputWriter.WriteLineAsync($"Writing model data package");

        var archiveOutputPath = Path.Combine(outputDirectory.FullName,
            metadata.Hash + ".temp");

        var lastSizeWritten = 0L;

        await TarZ.Create(inputDirectory.FullName,
            archiveOutputPath,
            ((_,
                total,
                _) =>
            {
                if (total - lastSizeWritten > 100 * 1024 * 1024)
                {
                    outputWriter.WriteLine($"{total.Bytes().Humanize()} written");
                    lastSizeWritten = total;
                }
            }),
            compressionLevel,
            cancellationToken);

        var outerHash = new FileInfo(archiveOutputPath).CalculateHash(_hashProvider);
        var archiveFinalPath = Path.Combine(outputDirectory.FullName,
            outerHash.ToString() + MimeTypes.InfernityModelDataPackage.Extensions[0]);

        if (!File.Exists(archiveFinalPath))
        {
            File.Move(archiveOutputPath,
                archiveFinalPath);
        }
        else
        {
            File.Delete(archiveOutputPath);
        }

        await outputWriter.WriteLineAsync($"Model data package written to: {archiveFinalPath}");

        return new NugetModelPackageCreationResult(metadata,
            new FileInfo(archiveFinalPath),
            outerHash);
    }
    
    private IInferenceProviderFactory DetectInferenceProvider(DirectoryInfo modelFilesDirectory)
    {
        foreach (var inferenceProviderFactory in _inferenceProviderFactories.Values)
        {
            if (inferenceProviderFactory.Analyzer.AppliesTo(modelFilesDirectory))
            {
                return inferenceProviderFactory;
            }
        }

        throw new ModelPackageException(
            $"No inference provider found for model files: {modelFilesDirectory.FullName}");
    }

    private IInferenceProviderFactory GetInferenceProviderFactory(InferenceProviderId inferenceProviderId)
    {
        return _inferenceProviderFactories.GetOptional(inferenceProviderId).OrThrow(() =>
            new ModelPackageException($"Unknown inference provider: {inferenceProviderId}"));
    }

    private void CoerceInputAndOutputDirectory(Optional<DirectoryInfo> inputDirectory,
        DirectoryInfo outputDirectory)
    {
        outputDirectory.Create();
        if (inputDirectory)
        {
            EnsureInputDirectory(inputDirectory.Value);
        }
    }

    private void EnsureInputDirectory(DirectoryInfo inputDirectory)
    {
        if (!inputDirectory.Exists)
        {
            throw new ModelPackageException($"Input directory does not exist: {inputDirectory.FullName}");
        }
    }
}