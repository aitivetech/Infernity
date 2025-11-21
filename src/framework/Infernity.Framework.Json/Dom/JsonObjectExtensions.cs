using System.Text.Json;
using System.Text.Json.Nodes;

namespace Infernity.Framework.Json.Dom;


[Flags]
public enum JsonMergeOptions
{
    None = 0x00,
    TreatNullAsNotSet = 0x01,
    TreatEmptyArrayAsNotSet = 0x02,
    Recursive = 0x04,

    Default = Recursive
}

public static class JsonObjectExtensions
{
    extension(JsonObject self)
    {
        public JsonElement ToElement()
        {
            return JsonSerializer.SerializeToElement(self);
        }
        
        public JsonObject MergeFrom(JsonObject source,JsonMergeOptions mergeOptions = JsonMergeOptions.Default)
        {
            var result = self.DeepClone().AsObject();

            static bool ShouldBeOverwritten(
                JsonNode? value,
                JsonNode? sourceValue,
                JsonMergeOptions mergeOptions)
            {
                if (value == null)
                {
                    return mergeOptions.HasFlag(JsonMergeOptions.TreatNullAsNotSet);
                }

                if (value.GetValueKind() == JsonValueKind.Array && value is JsonArray array && sourceValue is JsonArray)
                {
                    return array.Count == 0 && mergeOptions.HasFlag(JsonMergeOptions.TreatEmptyArrayAsNotSet);
                }

                return false;
            }
            
            foreach (var property in source)
            {
                if (result.TryGetPropertyValue(property.Key,
                        out var selfValue))
                {
                    if (ShouldBeOverwritten(selfValue,
                            property.Value,
                            mergeOptions))
                    {
                        // Overwrite completely
                        result[property.Key] = property.Value;
                    }
                    else
                    {
                        // Determine if we have to work recursively
                        if (selfValue is JsonObject r && property.Value is JsonObject s && mergeOptions.HasFlag(JsonMergeOptions.Recursive))
                        {
                            result[property.Key] =  r.MergeFrom(s, mergeOptions);
                        }
                    }
                }
                else
                {
                    result.Add(property.Key, property.Value);                 
                }
            }

            return result;
        }
    }
}