using Infernity.GeneratedCode;

namespace Infernity.Inference.Abstractions.Models;

[TypedId]
public readonly partial record struct ModelFamilyId(string Value)
{
    public static readonly ModelFamilyId Unknown = new("unknown");
}