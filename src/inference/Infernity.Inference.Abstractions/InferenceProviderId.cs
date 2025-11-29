using Infernity.GeneratedCode;

namespace Infernity.Inference.Abstractions;

[TypedId]
public readonly partial record struct InferenceProviderId(string Value)
{
    public static readonly InferenceProviderId Unknown = new("Unknown");
}