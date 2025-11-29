using Infernity.GeneratedCode;

namespace Infernity.Inference.Abstractions.Models;

[TypedId]
public readonly partial record struct ModelId(string Value)
{
    public static readonly ModelId Unknown = new("unknown");
}