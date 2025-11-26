using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Reflection;

namespace Infernity.Framework.Json.Converters;

public class PolymorphicResolverJsonConverter<TType, T>(ITypeResolver<TType> typeResolver) : PolymorphicJsonConverter<TType, T>
    where TType : notnull
    where T : notnull
{
    protected override Optional<Type> GetValueType(TType type,JsonElement data,JsonSerializerOptions options)
    {
        return typeResolver.Resolve(type);
    }

    protected override Optional<TType> GetTypeId(T value,JsonSerializerOptions options)
    {
        return typeResolver.Resolve(value.GetType());
    }
}

public abstract class PolymorphicJsonConverter<TType, T> : JsonConverter<T>
    where TType : notnull
{
    private readonly bool _flatten;
    private readonly string _typeDiscriminatorName;
    private readonly string _valueName;

    public PolymorphicJsonConverter(
        bool flatten = false,
        string typeDiscriminatorName = "type",
        string valueName = "value"
    )
    {
        _valueName = valueName;
        _flatten = flatten;
        _typeDiscriminatorName = typeDiscriminatorName;
    }

    public override T? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var element = JsonSerializer.Deserialize<JsonElement>(
            ref reader,
            options
        );

        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException($"Expected object, got {element.ValueKind}");
        }

        if (element.TryGetProperty(_typeDiscriminatorName,
                out var typeIdElement))
        {
            var typeId = typeIdElement.Deserialize<TType>(options);

            if (typeId == null)
            {
                throw new JsonException(
                    $"Invalid {_typeDiscriminatorName} value: {typeIdElement.GetRawText()}"
                );
            }

            var inputType = GetValueType(typeId,element,options)
                .OrThrow(() => new JsonException($"Unknown {_typeDiscriminatorName}: {typeId}"));

            if (_flatten)
            {
                return
                    (T?)element.Deserialize(inputType,
                        CreateValueOptions(options,
                            typeId,
                            inputType,
                            element));
            }

            if (element.TryGetProperty(_valueName,
                    out var valueElement))
            {
                return
                    (T?)valueElement.Deserialize(
                        inputType,
                        CreateValueOptions(options,
                            typeId,
                            inputType,
                            element)
                    );
            }
        }

        throw new JsonException(
            $"Invalid polymorphic object, {_typeDiscriminatorName} or value property missing"
        );
    }

    public override void Write(Utf8JsonWriter writer,
        T? value,
        JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var typeId = GetTypeId(value,options)
            .OrThrow(() => new JsonException($"Unknown type: {value.GetType()}"));

        writer.WriteStartObject();

        writer.WritePropertyName(_typeDiscriminatorName);
        JsonSerializer.Serialize(writer,
            typeId,
            options);

        if (_flatten)
        {
            var objectElement = JsonSerializer.SerializeToElement(
                value,
                value.GetType(),
                CreateValueOptions(options,
                    typeId,
                    value.GetType(),
                    value)
            );

            foreach (var property in objectElement.EnumerateObject())
            {
                property.WriteTo(writer);
            }
        }
        else
        {
            writer.WritePropertyName(_valueName);
            JsonSerializer.Serialize(
                writer,
                value,
                value.GetType(),
                CreateValueOptions(options,
                    typeId,
                    value.GetType(),
                    value)
            );
        }

        writer.WriteEndObject();
    }

    protected abstract Optional<Type> GetValueType(TType type,JsonElement data,JsonSerializerOptions options);
    
    protected abstract Optional<TType> GetTypeId(T value,JsonSerializerOptions options);

    protected virtual JsonSerializerOptions CreateValueOptions(JsonSerializerOptions options,
        TType discriminator,
        Type valueType,
        JsonElement data)
    {
        return CreateValueOptions(options, discriminator, valueType);
    }
    
    protected virtual JsonSerializerOptions CreateValueOptions(JsonSerializerOptions options,
        TType discriminator,
        Type valueType,
        T value)
    {
        return CreateValueOptions(options, discriminator, valueType);
    }
    
    protected virtual JsonSerializerOptions CreateValueOptions(JsonSerializerOptions options,
        TType discriminator,
        Type valueType)
    {
        return options.WithoutConverterInstance(this);
    }
}