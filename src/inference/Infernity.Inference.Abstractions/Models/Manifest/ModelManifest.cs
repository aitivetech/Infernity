namespace Infernity.Inference.Abstractions.Models.Manifest;

public abstract class ModelManifest
{
    public required ModelId Id { get; init; }
    public required ModelFamilyId Family { get; init; }
    public required ModelArchitectureId Architecture { get; init; }
    public required InferenceProviderId Provider { get; init; }
}

public static class ModelManifestExtensions
{
    extension(ModelManifest manifest)
    {
        public ModelInfo Info => new(manifest.Id, manifest.Family, manifest.Architecture);
    }
}