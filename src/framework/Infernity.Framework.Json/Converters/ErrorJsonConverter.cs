using System.Text.Json;
using System.Text.Json.Serialization;

using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Json.Converters;

public sealed class ErrorJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsAssignableTo(typeof(ErrorBase)) && !typeToConvert.IsAbstract;
    
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return Activator.CreateInstance(
                typeof(ErrorJsonConverter<>).MakeGenericType(typeToConvert)) as
            JsonConverter;
    }
}

public sealed class ErrorJsonConverter<T> : JsonConverter<T>
    where T : ErrorBase<T>,IErrorFactory<T>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var payload = JsonSerializer.Deserialize<ErrorPayload>(ref reader, options);

        if (payload != null)
        {
            return T.CreateInstance(payload.Id, payload.StatusCode, payload.Message);
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value != null)
        {
            var payload = value.Payload;
            JsonSerializer.Serialize(writer, payload, options);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
