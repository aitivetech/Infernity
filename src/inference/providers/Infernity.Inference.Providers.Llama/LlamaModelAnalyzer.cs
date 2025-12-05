using Infernity.Framework.Core.Collections;
using Infernity.Framework.Core.Io;
using Infernity.GeneratedCode;
using Infernity.Inference.Abstractions.Models;
using Infernity.Inference.Abstractions.Models.Analysis;
using Infernity.Inference.Abstractions.Models.Manifest;

using LLama.Common;

using Microsoft.Extensions.Logging;

namespace Infernity.Inference.Providers.Llama;

[AddLogger]
public sealed partial class LlamaModelAnalyzer : IModelAnalyzer
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
        ModelIdentity modelIdentity)
    {
        var baseManifest = (LlamaModelManifest)_lLamaModelManifestHandler.CreateDefault(modelIdentity);
        
        var modelFile = directoryInfo.EnumerateFiles("*.gguf",
            SearchOption.AllDirectories).FirstOrNone();

        if (modelFile)
        {
           Logger.LogInformation("Trying to analyze: {modelFile}", modelFile.Value.FullName);

           var modelParams = new ModelParams(modelFile.Value.FullName);
           baseManifest.ContextSize = modelParams.ContextSize ?? 0;
           baseManifest.ParameterCount = modelFile.Value.Length;
        }
        
        return baseManifest;
    }
}