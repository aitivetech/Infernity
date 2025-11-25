using Infernity.Inference.Abstractions.Models.Configuration;
using Infernity.Inference.Abstractions.Models.Manifest;

namespace Infernity.Inference.Abstractions;

public interface IInferenceProviderFactory
{
    InferenceProviderId Id { get; }
    
    Type ConfigurationSectionType { get; }
    
    IModelConfigurationHandler ConfigurationHandler { get; }
    
    IModelManifestHandler ManifestHandler { get; }
    
    IInferenceProvider CreateInferenceProvider(InferenceProviderConfiguration configuration);
}