namespace Infernity.Inference.Providers.Llama;

public sealed class LlamaInferenceProviderConfiguration
{
    public LlamaBackendType Backend { get; set; } = LlamaBackendType.Vulkan;
}