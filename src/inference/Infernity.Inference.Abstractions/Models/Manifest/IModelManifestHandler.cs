namespace Infernity.Inference.Abstractions.Models.Manifest;

public interface IModelManifestHandler
{ 
   Type GetManifestType(ModelInfo modelInfo);
   
   
}