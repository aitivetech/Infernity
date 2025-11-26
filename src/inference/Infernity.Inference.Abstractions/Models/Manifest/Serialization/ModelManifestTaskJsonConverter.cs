using System.Text.Json;

using Infernity.Framework.Core.Functional;
using Infernity.Framework.Json.Converters;

namespace Infernity.Inference.Abstractions.Models.Manifest.Serialization;

public sealed class ModelManifestTaskJsonConverter : PolymorphicDictionaryJsonConverter<InferenceTaskType,ModelManifestTask>
{
    private readonly ModelInfo _modelInfo;
    private readonly IModelManifestHandler _modelManifestHandler;

    public ModelManifestTaskJsonConverter(ModelInfo modelInfo,IModelManifestHandler modelManifestHandler)
    {
        _modelInfo = modelInfo;
        _modelManifestHandler = modelManifestHandler;
    }

    protected override Optional<Type> GetValueType(InferenceTaskType type,
        JsonElement data,
        JsonSerializerOptions options)
    {
        return _modelManifestHandler.GetTaskType(_modelInfo,
            type);
    }
}