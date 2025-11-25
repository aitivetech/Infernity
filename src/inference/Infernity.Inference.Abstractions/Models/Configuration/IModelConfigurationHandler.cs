using Infernity.Inference.Abstractions.Models.Manifest;

namespace Infernity.Inference.Abstractions.Models.Configuration;

public interface IModelConfigurationHandler
{
    Type GetConfigurationType(ModelManifest manifest);
}