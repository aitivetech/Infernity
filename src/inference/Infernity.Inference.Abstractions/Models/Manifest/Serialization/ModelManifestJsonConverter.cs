using System.Text.Json;

using Infernity.Framework.Core.Collections;
using Infernity.Framework.Core.Functional;
using Infernity.Framework.Json;
using Infernity.Framework.Json.Converters;

namespace Infernity.Inference.Abstractions.Models.Manifest.Serialization;

public sealed class ModelManifestJsonConverter : PolymorphicJsonConverter<InferenceProviderId, ModelManifest>
{
    private readonly IReadOnlyDictionary<InferenceProviderId, IModelManifestHandler> _modelManifestHandlers;

    public ModelManifestJsonConverter(IEnumerable<IInferenceProviderFactory> inferenceProviderFactories)
        : base(true,
            typeDiscriminatorName: "provider")
    {
        _modelManifestHandlers = inferenceProviderFactories.ToDictionary(i => i.Id,
            i => i.ManifestHandler);
    }

    protected override Optional<Type> GetValueType(InferenceProviderId type,
        JsonElement data,
        JsonSerializerOptions options)
    {
        var modelInfo = ReadModelInfo(data,
            options);

        return _modelManifestHandlers.GetOptional(type).Select(o => o.GetType(modelInfo));
    }

    protected override Optional<InferenceProviderId> GetTypeId(ModelManifest value,
        JsonSerializerOptions options)
    {
        return value.Provider;
    }

    protected override JsonSerializerOptions CreateValueOptions(JsonSerializerOptions options,
        InferenceProviderId discriminator,
        Type valueType,
        JsonElement data)
    {
        var modelInfo = ReadModelInfo(data,
            options);

        return options.WithConverters([
            new ModelManifestTaskJsonConverter(modelInfo,
                _modelManifestHandlers
                    [discriminator])
        ]);
    }

    protected override JsonSerializerOptions CreateValueOptions(JsonSerializerOptions options,
        InferenceProviderId discriminator,
        Type valueType,
        ModelManifest value)
    {
        return options.WithConverters([
            new ModelManifestTaskJsonConverter(value.Identity,
                _modelManifestHandlers[discriminator])
        ]);
    }

    private ModelIdentity ReadModelInfo(JsonElement data,
        JsonSerializerOptions options)
    {
        return data.Deserialize<ModelIdentity>(options) ?? throw new JsonException();
    }
}