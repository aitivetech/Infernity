using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infernity.Framework.Json.Converters;

public class FlagsEnumArrayConverter<T>(T defaultValue = default,bool ignoreCase = true) : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var flagValue = reader.GetString();

            if (flagValue != null)
            {
                return (T)Enum.Parse(typeof(T), flagValue, ignoreCase: ignoreCase);
            }
        }
        
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var flags = new List<string>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                var value = reader.GetString();

                if (value != null)
                {
                    flags.Add(value);
                }
            }
            
            return (T)Enum.Parse(typeof(T), string.Join(", ", flags),ignoreCase:ignoreCase);
        }
        
        throw new JsonException("Enum flag values must be stored as string or array of strings");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var flagValues = value.ToString().Split([", "], StringSplitOptions.RemoveEmptyEntries);

        if (flagValues.Length == 0)
        {
            writer.WriteStringValue(defaultValue.ToString());
        }
        else if (flagValues.Length == 1)
        {
            writer.WriteStringValue(flagValues[0]);
        }
        else
        {
            writer.WriteStartArray();
            foreach (var flag in flagValues)
            {
                writer.WriteStringValue(flag.Trim());
            }

            writer.WriteEndArray();
        }
    }
}
