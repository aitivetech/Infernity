using System.Text.Json;

using Infernity.Framework.Core.Functional;
using Infernity.Framework.Json.Converters;

namespace Infernity.Inference.Abstractions.Models.Manifest.Serialization;

public sealed class ModelManifestTaskJsonConverter : PolymorphicDictionaryJsonConverter<InferenceTaskType,ModelManifestTask>
{
    private readonly ModelIdentity _modelIdentity;
    private readonly IModelManifestHandler _modelManifestHandler;

    public ModelManifestTaskJsonConverter(ModelIdentity modelIdentity,IModelManifestHandler modelManifestHandler)
    {
        _modelIdentity = modelIdentity;
        _modelManifestHandler = modelManifestHandler;
    }

    protected override Optional<Type> GetValueType(InferenceTaskType type,
        JsonElement data,
        JsonSerializerOptions options)
    {
        return _modelManifestHandler.GetTaskType(_modelIdentity,
            type);
    }
}