using Infernity.Inference.Abstractions;
using Infernity.Inference.Abstractions.Models.Configuration;
using Infernity.Inference.Abstractions.Models.Manifest;

using LLama.Native;

namespace Infernity.Inference.Providers.Llama;

public class LlamaModelConfigurationHandler : IModelConfigurationHandler
{
    public Type GetConfigurationType(ModelManifest manifest)
    {
        throw new NotImplementedException();
    }

    public Type GetConfigurationTaskType(ModelManifest manifest,
        InferenceTaskType taskType)
    {
        throw new NotImplementedException();
    }

    public ModelConfiguration CreateDefault(ModelManifest manifest)
    {
        throw new NotImplementedException();
    }

    public ModelConfigurationTask CreateDefault(ModelManifest manifest,
        InferenceTaskType taskType)
    {
        throw new NotImplementedException();
    }
}