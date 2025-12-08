using Infernity.Inference.Abstractions;
using Infernity.Inference.Abstractions.Models;
using Infernity.Inference.Abstractions.Models.Manifest;

namespace Infernity.Inference.Providers.Llama;

public sealed class LLamaModelManifestHandler : IModelManifestHandler
{
    public Type GetType(ModelIdentity modelIdentity)
    {
        return typeof(LlamaModelManifest);
    }

    public Type GetTaskType(ModelIdentity modelIdentity,
        InferenceTaskType taskType)
    {
        return typeof(LlamaModelManifestTask);
    }

    public ModelManifest CreateDefault(ModelIdentity modelIdentity)
    {
        return new LlamaModelManifest
        {
            Version = modelIdentity.Version,
            Provider = LlamaInferenceProvider.IdConst,
            Architecture = modelIdentity.Architecture,
            Family = modelIdentity.Family,
            SubFamily = modelIdentity.SubFamily,
            Quantization = ModelQuantizationType.Q8K,
            Tasks = new Dictionary<InferenceTaskType, ModelManifestTask>(),
            Signature = string.Empty,
        };
    }
}