namespace Infernity.Inference.Abstractions.Models.Manifest;

public interface IModelManifestHandler
{ 
   Type GetType(ModelIdentity modelIdentity);
   
   Type GetTaskType(ModelIdentity modelIdentity,InferenceTaskType taskType);
   
   ModelManifest CreateDefault(ModelIdentity modelIdentity);
}