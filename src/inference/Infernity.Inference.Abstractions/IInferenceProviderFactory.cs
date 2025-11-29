using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Reflection;
using Infernity.Inference.Abstractions.Models.Analysis;
using Infernity.Inference.Abstractions.Models.Configuration;
using Infernity.Inference.Abstractions.Models.Manifest;

namespace Infernity.Inference.Abstractions;

public interface IInferenceProviderFactory
{
    InferenceProviderId Id { get; }
    Type ConfigurationSectionType { get; }
    
    IModelConfigurationHandler ConfigurationHandler { get; }
    
    IModelManifestHandler ManifestHandler { get; }
    
    IModelAnalyzer Analyzer { get; }
    
    IInferenceProvider CreateInferenceProvider(InferenceProviderConfiguration configuration);
}
