using Infernity.Framework.Configuration;

namespace Infernity.Inference.Abstractions;

[ConfigurationSection("Inference")]
public abstract class InferenceProviderConfiguration
{
    public InferenceProviderId Provider { get; set; }
}