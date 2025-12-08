using Semver;

namespace Infernity.Inference.Abstractions.Models;

public sealed record ModelIdentity(
    ModelId Id,
    ModelFamilyId Family,
    ModelFamilyId SubFamily,
    ModelArchitectureId Architecture,
    SemVersion Version)
{
    public static readonly ModelIdentity Unknown = new (
        ModelId.Unknown,
        ModelFamilyId.Unknown,
        ModelFamilyId.Unknown,
        ModelArchitectureId.Unknown,
        SemVersion.ParsedFrom(1));
}