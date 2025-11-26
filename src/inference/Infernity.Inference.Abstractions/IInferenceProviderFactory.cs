using Infernity.Inference.Abstractions.Models.Analysis;
using Infernity.Inference.Abstractions.Models.Configuration;
using Infernity.Inference.Abstractions.Models.Manifest;

namespace Infernity.Inference.Abstractions;

public interface IInferenceProviderFactory
{
    InferenceProviderId Id { get; }
    
    IModelConfigurationHandler ConfigurationHandler { get; }
    
    IModelManifestHandler ManifestHandler { get; }
    
    IModelAnalyzer Analyzer { get; }
    
    IInferenceProvider CreateInferenceProvider(InferenceProviderConfiguration configuration);
}