using System.Numerics;
using System.Security.Cryptography;

using Humanizer;

using Infernity.Framework.Compression.Archives;
using Infernity.Framework.Core;
using Infernity.Framework.Core.Collections;
using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Io.Streams;
using Infernity.Framework.Security.Hashing;
using Infernity.Framework.Security.Io;
using Infernity.Inference.Abstractions;
using Infernity.Inference.Abstractions.Models;
using Infernity.Inference.Abstractions.Models.Manifest;

namespace Infernity.Inference.Packaging.Builder;

public sealed record ModelDataPackageCreationResult(
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

    public async Task Initialize(DirectoryInfo modelDirectory,
        DirectoryInfo outputDirectory,
        TextWriter outputWriter,
        Optional<InferenceProviderId> inferenceProviderId,
        int compressionLevel = 22,
        CancellationToken cancellationToken = default)
    {
        var analysisResult = await CreateModelDataPackage(modelDirectory,
            outputDirectory,
            outputWriter,
            compressionLevel,
            cancellationToken);

        var manifestFile = await CreateManifest(modelDirectory,
            outputDirectory,
            outputWriter,
            inferenceProviderId,
            Optional<ModelInfo>.None,
            analysisResult,
            cancellationToken);
        
        
    }
    
    public async Task<FileInfo> CreateManifest(
        Optional<DirectoryInfo> modelDirectory,
        DirectoryInfo outputDirectory,
        TextWriter outputWriter,
        Optional<InferenceProviderId> inferenceProviderId,
        Optional<ModelInfo> modelInfo,
        Optional<ModelDataPackageCreationResult> modelDataPackageInfo,
        CancellationToken cancellationToken = default)

    {
        if (!inferenceProviderId.HasValue && !modelDirectory.HasValue)
        {
            throw new ModelPackagingException($"Either inference provider or model directory must be set");
        }
        
        EnsureOutputDirectory(outputDirectory);
        
        Optional<IInferenceProviderFactory> inferenceProviderFactory = Optional<IInferenceProviderFactory>.None;

        var finalModelInfo = modelInfo.Or(() => ModelInfo.Unknown)!;
        
        if (inferenceProviderId.HasValue)
        {
            inferenceProviderFactory = Optional.Some<IInferenceProviderFactory>(GetInferenceProviderFactory(inferenceProviderId.Value));
        }

        if (modelDirectory.HasValue)
        {
            EnsureModelDirectory(modelDirectory.Value);
            inferenceProviderFactory = Optional.Some<IInferenceProviderFactory>(DetectInferenceProvider(modelDirectory.Value));
        }

        await outputWriter.WriteLineAsync($"Packing for inference provider: {inferenceProviderFactory.Value.Id}");
        
        Optional<ModelManifest> modelManifest = Optional.None<ModelManifest>();
        
        if (modelDirectory.HasValue)
        {
            await outputWriter.WriteLineAsync($"Analyzing model files");
            modelManifest = inferenceProviderFactory.Value.Analyzer.Analyze(modelDirectory.Value,finalModelInfo);
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

        var outputFilename = Path.Combine(outputDirectory.FullName,
            "manifest.json");
        
        await modelManifest.Value.WriteAsync(outputFilename);
        
        await outputWriter.WriteLineAsync($"Manifest written to: {outputFilename}");

        var testManifest = await ModelManifest.ReadAsync(outputFilename);
        await testManifest.WriteAsync(Path.Combine(outputDirectory.FullName,
            "roundtrip.json"));
        
        return new FileInfo(outputFilename);
    }
    
    public async Task<ModelDataPackageCreationResult> CreateModelDataPackage(
        DirectoryInfo modelDirectory,
        DirectoryInfo outputDirectory,
        TextWriter outputWriter,
        int compressionLevel = 22,
        CancellationToken cancellationToken = default)
    {
        EnsureModelDirectory(modelDirectory);
        EnsureOutputDirectory(outputDirectory);
        
        await outputWriter.WriteLineAsync($"Analyzing model files");

        var metadata = modelDirectory.CalculateHashAndMetadata(_hashProvider);

        await outputWriter.WriteLineAsync($"Model content {metadata}");
        await outputWriter.WriteLineAsync($"Writing model data package");

        var archiveOutputPath = Path.Combine(outputDirectory.FullName,
            metadata.Hash + ".temp");

        await TarZ.Create(modelDirectory.FullName,
            archiveOutputPath,
            ((stream,total,
                read) =>
            {
                outputWriter.WriteLine($"{total.Bytes().Humanize()} written");
            }),
            compressionLevel,
            cancellationToken);

        var outerHash = new FileInfo(archiveOutputPath).CalculateHash(_hashProvider);
        
        var archiveFinalPath = Path.Combine(outputDirectory.FullName,
            outerHash + ApplicationSuiteInfo.ModelDataPackageFileExtension);

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

        var extractionDirectoryPath = Path.Combine(outputDirectory.FullName,
            "test");
        
        Directory.CreateDirectory(extractionDirectoryPath);
        
        await TarZ.Extract(archiveFinalPath, extractionDirectoryPath);

        return new ModelDataPackageCreationResult(metadata,
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

        throw new ModelPackagingException(
            $"No inference provider found for model files: {modelFilesDirectory.FullName}");
    }

    private IInferenceProviderFactory GetInferenceProviderFactory(InferenceProviderId inferenceProviderId)
    {
        return _inferenceProviderFactories.GetOptional(inferenceProviderId).OrThrow(() =>
            new ModelPackagingException($"Unknown inference provider: {inferenceProviderId}"));
    }

    private void EnsureModelDirectory(DirectoryInfo modelDirectory)
    {
        if (!modelDirectory.Exists)
        {
            throw new ModelPackagingException($"Model files directory does not exist: {modelDirectory.FullName}");
        }
    }

    private void EnsureOutputDirectory(DirectoryInfo outputDirectory)
    {
        outputDirectory.Create();
    }
}