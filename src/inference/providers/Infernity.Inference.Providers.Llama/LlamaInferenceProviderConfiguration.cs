using Infernity.Inference.Abstractions;

namespace Infernity.Inference.Providers.Llama;

public sealed class LlamaInferenceProviderConfiguration : InferenceProviderConfiguration
{
    public LlamaBackendType Backend { get; set; } = LlamaBackendType.Vulkan;
}