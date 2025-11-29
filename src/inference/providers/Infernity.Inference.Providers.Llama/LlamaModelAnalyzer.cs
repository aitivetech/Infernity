using Infernity.Framework.Core.Io;
using Infernity.Inference.Abstractions.Models;
using Infernity.Inference.Abstractions.Models.Analysis;
using Infernity.Inference.Abstractions.Models.Manifest;

namespace Infernity.Inference.Providers.Llama;

public sealed class LlamaModelAnalyzer : IModelAnalyzer
{
    private readonly LLamaModelManifestHandler _lLamaModelManifestHandler;

    public LlamaModelAnalyzer(LLamaModelManifestHandler lLamaModelManifestHandler)
    {
        _lLamaModelManifestHandler = lLamaModelManifestHandler;
    }

    public bool AppliesTo(DirectoryInfo directoryInfo)
    {
        return directoryInfo.ContainsAnyFileWithExtension([".gguf"]);
    }

    public ModelManifest Analyze(
        DirectoryInfo directoryInfo,
        ModelInfo modelInfo)
    {
        return _lLamaModelManifestHandler.CreateDefault(modelInfo);
    }
}