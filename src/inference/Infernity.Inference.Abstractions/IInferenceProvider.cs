namespace Infernity.Inference.Abstractions;

public interface IInferenceProvider
{
    InferenceProviderId Id { get; }
    
    Type ConfigurationSectionType { get; }
}