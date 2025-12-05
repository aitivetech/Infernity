using System.Globalization;
using System.Text.Json.Serialization;

using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Versioning;
using Infernity.Framework.Json.Dom;

namespace Infernity.Inference.Abstractions.Models.Manifest;

public abstract class ModelManifest : TypedJsonDocument<ModelManifest>
{
    public const int BasePropertyOrder = -1000;

    [JsonPropertyOrder(BasePropertyOrder + 1)]
    public ModelId Id { get; set; } = ModelId.Unknown;

    [JsonPropertyOrder(BasePropertyOrder + 2)]
    public SemanticVersion Version { get; set; } = SemanticVersion.Parse("1.0.0");

    [JsonPropertyOrder(BasePropertyOrder + 3)]
    public ModelFamilyId Family { get; set; } = ModelFamilyId.Unknown;

    [JsonPropertyOrder(BasePropertyOrder + 4)]
    public ModelFamilyId SubFamily { get; set; } = ModelFamilyId.Unknown;

    [JsonPropertyOrder(BasePropertyOrder + 5)]
    public ModelArchitectureId Architecture { get; set; } = ModelArchitectureId.Unknown;

    [JsonPropertyOrder(BasePropertyOrder + 6)]
    public InferenceProviderId Provider { get; set; } = InferenceProviderId.Unknown;

    [JsonPropertyOrder(BasePropertyOrder + 7)]
    public Optional<string> Description { get; set; }

    [JsonPropertyOrder(BasePropertyOrder + 8)]
    public long ContextSize { get; set; }
    
    [JsonPropertyOrder(BasePropertyOrder + 9)]
    public RegionInfo CountryOfOrigin { get; set; } = new RegionInfo("US");
    
    public required IReadOnlyDictionary<InferenceTaskType, ModelManifestTask> Tasks { get; set; }

    public abstract ModelId GenerateId();
}

public static class ModelManifestExtensions
{
    extension(ModelManifest manifest)
    {
        public ModelIdentity Identity => new(manifest.Id,
            manifest.Family,
            manifest.SubFamily,
            manifest.Architecture,
            manifest.Version);
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

        public VersionedModelId VersionedId => new (manifest.Id,manifest.Version);
    }
}