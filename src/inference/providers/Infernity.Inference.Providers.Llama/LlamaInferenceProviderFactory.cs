using Infernity.Framework.Core.Functional;
using Infernity.Inference.Abstractions;
using Infernity.Inference.Abstractions.Models.Analysis;
using Infernity.Inference.Abstractions.Models.Configuration;
using Infernity.Inference.Abstractions.Models.Manifest;

using LLama.Common;

namespace Infernity.Inference.Providers.Llama;

public sealed class LlamaInferenceProviderFactory : IInferenceProviderFactory
{
    private readonly Lazy<LlamaModelConfigurationHandler> _configurationHandler;
    private readonly Lazy<LlamaModelAnalyzer> _analyzer;
    private readonly Lazy<LLamaModelManifestHandler> _manifestHandler;
    
    public LlamaInferenceProviderFactory()
    {
        _manifestHandler = new Lazy<LLamaModelManifestHandler>(CreateManifestHandler);
        _configurationHandler = new Lazy<LlamaModelConfigurationHandler>(CreateConfigurationHandler);
        _analyzer = new Lazy<LlamaModelAnalyzer>(CreateAnalyzer);
    }

    public InferenceProviderId Id { get; } = LlamaInferenceProvider.IdConst;
    public Type ConfigurationSectionType { get; } = typeof(LlamaInferenceProviderConfiguration);
    
    public IModelConfigurationHandler ConfigurationHandler => _configurationHandler.Value;
    public IModelManifestHandler ManifestHandler => _manifestHandler.Value;
    public IModelAnalyzer Analyzer => _analyzer.Value;

    public IInferenceProvider CreateInferenceProvider(InferenceProviderConfiguration configuration)
    {
        // Load llama with configuration
        var loader = LlamaInferenceProviderLoader.GetOrCreate((LlamaInferenceProviderConfiguration)configuration);
        
        throw new NotImplementedException();
    }

    private LlamaModelConfigurationHandler CreateConfigurationHandler()
    {
        // Used without creating a provider so do not load.
        return new LlamaModelConfigurationHandler();
    }

    private LLamaModelManifestHandler CreateManifestHandler()
    {
        // Used without creating a provider so do not load
        return new LLamaModelManifestHandler();
    }

    private LlamaModelAnalyzer CreateAnalyzer()
    {
        // Needs llama working so load
        EnsureLLamaLoaded();
        
        return new LlamaModelAnalyzer(_manifestHandler.Value);
    }

    private static void EnsureLLamaLoaded()
    {
        // Load llama in cpu only mode.
        LlamaInferenceProviderLoader.GetOrCreate(Optional<LlamaInferenceProviderConfiguration>.None);
    }
}