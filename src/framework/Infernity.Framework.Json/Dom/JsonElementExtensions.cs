using System.Text.Json;
using System.Text.Json.Nodes;

using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Json.Dom;

[Flags]
public enum JsonMergeOptions
{
    None = 0x00,
    TreatNullAsNotSet = 0x01,
    TreatEmptyArrayAsNotSet = 0x02,

    Default = TreatNullAsNotSet | TreatEmptyArrayAsNotSet
}

public static class JsonElementExtensions
{
    public static JsonNode? MergeInto(this JsonElement source,
        JsonElement target,
        JsonMergeOptions mergeOptions = JsonMergeOptions.Default)
    {
        static bool IsNull(JsonNode? node)
        {
            return node == null || node.GetValueKind() == JsonValueKind.Null;
        }

        static bool IsEmptyArray(JsonNode? node)
        {
            if (node == null)
            {
                return false;
            }

            if (node.GetValueKind() == JsonValueKind.Array && node is JsonArray array)
            {
                return array.Count == 0;
            }

            return false;
        }

        var result = target.Deserialize<JsonNode>();

        if (result is not JsonObject jsonObject)
        {
            return result;
        }

        var resultObject = jsonObject;

        foreach (var property in source.EnumerateObject())
        {
            if (resultObject.TryGetPropertyValue(property.Name,
                    out var targetValue))
            {
                if ((IsNull(targetValue) && mergeOptions.HasFlag(JsonMergeOptions.TreatNullAsNotSet)) ||
                    (IsEmptyArray(targetValue) && mergeOptions.HasFlag(JsonMergeOptions.TreatEmptyArrayAsNotSet)))
                {
                    resultObject.Add(property.Name,
                        property.Value.Deserialize<JsonNode>());
                }
            }
            else
            {
                resultObject.Add(property.Name,
                    property.Value.Deserialize<JsonNode>());
            }
        }

        return resultObject;
    }

    extension(in JsonElement jsonElement)
    {
        public string ReadRequiredString()
        {
            if (jsonElement.ValueKind == JsonValueKind.String)
            {
                return jsonElement.GetString() ?? throw new JsonException("Expected json string value, got null");
            }

            throw new JsonException($"Expected json string value, got: {jsonElement.ValueKind}");
        }

        public JsonElement ReadRequiredObjectProperty(string propertyName)
        {
            if (jsonElement.TryGetProperty(propertyName,
                    out var value))
            {
                return value.ValueKind == JsonValueKind.Object
                    ? value
                    : throw new JsonException($"Expected json object, got {value.ValueKind}");
            }

            throw new JsonException($"Property {propertyName} not found");
        }

        public Optional<JsonElement> ReadOptionalObjectProperty(string propertyName)
        {
            if (jsonElement.TryGetProperty(propertyName,
                    out var value))
            {
                return value.ValueKind == JsonValueKind.Object
                    ? value
                    : throw new JsonException($"Expected json object, got {value.ValueKind}");
            }

            return Optional<JsonElement>.None;
        }

        public Utf8JsonReader CreateReader()
        {
            // TODO: Optimize this
            var bytes = JsonSerializer.SerializeToUtf8Bytes(jsonElement);
            return new Utf8JsonReader(bytes);
        }
    }
}