namespace Infernity.Inference.Abstractions.Models;

public sealed record ModelInfo(
    ModelId Id,
    ModelFamilyId Family,
    ModelFamilyId SubFamily,
    ModelArchitectureId Architecture)
{
    public static readonly ModelInfo Unknown = new (
        ModelId.Unknown,
        ModelFamilyId.Unknown,
        ModelFamilyId.Unknown,
        ModelArchitectureId.Unknown);
}