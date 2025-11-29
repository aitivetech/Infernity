using System.Text.Json.Serialization;

using Infernity.Framework.Json.Dom;

namespace Infernity.Inference.Abstractions.Models.Manifest;

public abstract class ModelManifest : TypedJsonDocument<ModelManifest>
{
    public const int BasePropertyOrder = -1000;
    
    [JsonPropertyOrder(BasePropertyOrder +1)]
    public ModelId Id { get; set; } = ModelId.Unknown;
    
    [JsonPropertyOrder(BasePropertyOrder +2)]
    public ModelFamilyId Family { get; set; } = ModelFamilyId.Unknown;
    
    [JsonPropertyOrder(BasePropertyOrder +3)]
    public ModelFamilyId SubFamily { get; set; } = ModelFamilyId.Unknown;
    
    [JsonPropertyOrder(BasePropertyOrder +4)]
    public ModelArchitectureId Architecture { get; set; } = ModelArchitectureId.Unknown;
    [JsonPropertyOrder(BasePropertyOrder +5)]
    public InferenceProviderId Provider { get; set; } = InferenceProviderId.Unknown;

    public required IReadOnlyDictionary<InferenceTaskType, ModelManifestTask> Tasks { get; set; }

    public abstract ModelId GenerateId();
}

public static class ModelManifestExtensions
{
    extension(ModelManifest manifest)
    {
        public ModelInfo Info => new(manifest.Id,
            manifest.Family,
            manifest.SubFamily,
            manifest.Architecture);
    }

    extension<T>(T manifest)
        where T : ModelManifest
    {
        public T AssignGeneratedId(bool onlyIfCurrentlyUnknown = true)
        {
            var shouldAssign = manifest.Id == ModelId.Unknown || !onlyIfCurrentlyUnknown;

            if (shouldAssign)
            {
                manifest.Id = manifest.GenerateId();
            }

            return manifest;
        }
    }
}
