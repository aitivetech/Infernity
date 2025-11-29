using Infernity.GeneratedCode;

namespace Infernity.Inference.Abstractions.Models;

[TypedId]
public readonly partial record struct ModelArchitectureId(
    string Value)
{
    public static readonly ModelArchitectureId Unknown = new("unknown");
}