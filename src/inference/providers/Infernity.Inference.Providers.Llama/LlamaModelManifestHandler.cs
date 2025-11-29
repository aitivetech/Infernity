using Infernity.Inference.Abstractions;
using Infernity.Inference.Abstractions.Models;
using Infernity.Inference.Abstractions.Models.Manifest;

namespace Infernity.Inference.Providers.Llama;

public sealed class LLamaModelManifestHandler : IModelManifestHandler
{
    public Type GetType(ModelInfo modelInfo)
    {
        return typeof(LlamaModelManifest);
    }

    public Type GetTaskType(ModelInfo modelInfo,
        InferenceTaskType taskType)
    {
        return typeof(LlamaModelManifestTask);
    }

    public ModelManifest CreateDefault(ModelInfo modelInfo)
    {
        return new LlamaModelManifest
        {
            Provider = LlamaInferenceProvider.IdConst,
            Architecture = modelInfo.Architecture,
            Family = modelInfo.Family,
            SubFamily = modelInfo.SubFamily,
            Quantization = ModelQuantizationType.Q8K,
            Tasks = new Dictionary<InferenceTaskType, ModelManifestTask>()
        };
    }
}