using Infernity.Inference.Abstractions;
using Infernity.Inference.Abstractions.Models.Analysis;
using Infernity.Inference.Abstractions.Models.Configuration;
using Infernity.Inference.Abstractions.Models.Manifest;

namespace Infernity.Inference.Providers.Llama;

public class LlamaInferenceProviderFactory : IInferenceProviderFactory
{
    public LlamaInferenceProviderFactory()
    {
        var manifestHandler = new LLamaModelManifestHandler();
        Analyzer = new LlamaModelAnalyzer(manifestHandler);
        ManifestHandler = manifestHandler;
    }

    public InferenceProviderId Id { get; } = LlamaInferenceProvider.IdConst;
    public Type ConfigurationSectionType { get; } = typeof(LlamaInferenceProviderConfiguration);
    
    public IModelConfigurationHandler ConfigurationHandler { get; } = new LlamaModelConfigurationHandler();
    public IModelManifestHandler ManifestHandler { get; }
    public IModelAnalyzer Analyzer { get; } 

    public IInferenceProvider CreateInferenceProvider(InferenceProviderConfiguration configuration)
    {
        throw new NotImplementedException();
    }
}