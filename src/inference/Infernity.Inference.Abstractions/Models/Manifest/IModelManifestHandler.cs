namespace Infernity.Inference.Abstractions.Models.Manifest;

public interface IModelManifestHandler
{ 
   Type GetType(ModelInfo modelInfo);
   
   Type GetTaskType(ModelInfo modelInfo,InferenceTaskType taskType);
   
   ModelManifest CreateDefault(ModelInfo modelInfo);
}