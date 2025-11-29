using Infernity.Inference.Abstractions.Models.Manifest;

namespace Infernity.Inference.Abstractions.Models.Configuration;

public interface IModelConfigurationHandler
{
    Type GetConfigurationType(ModelManifest manifest);
    
    Type GetConfigurationTaskType(ModelManifest manifest,InferenceTaskType taskType);
    
    ModelConfiguration CreateDefault(ModelManifest manifest);
    
    ModelConfigurationTask CreateDefault(ModelManifest manifest, InferenceTaskType taskType);
}