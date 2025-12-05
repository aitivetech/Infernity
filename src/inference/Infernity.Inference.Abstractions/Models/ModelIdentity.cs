using Infernity.Framework.Core.Versioning;

namespace Infernity.Inference.Abstractions.Models;

public sealed record ModelIdentity(
    ModelId Id,
    ModelFamilyId Family,
    ModelFamilyId SubFamily,
    ModelArchitectureId Architecture,
    SemanticVersion Version)
{
    public static readonly ModelIdentity Unknown = new (
        ModelId.Unknown,
        ModelFamilyId.Unknown,
        ModelFamilyId.Unknown,
        ModelArchitectureId.Unknown,
        new SemanticVersion(1, 0, 0));
}