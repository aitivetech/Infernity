using System.Text.Json;

using Humanizer;

using Infernity.Framework.Core.Patterns;

namespace Infernity.Framework.Json.Dom;

public abstract class TypedJsonDocument<T> 
    where T : TypedJsonDocument<T>
{
    public static async Task<T> ReadAsync(Stream stream,JsonSerializerOptions? options = null)
    {
        return ThrowOnNull(await JsonSerializer.DeserializeAsync<T>(stream, GetSerializerOptions(options)));
    }

    public static T Read(Stream stream,
        JsonSerializerOptions? options = null)
    {
        return ThrowOnNull(JsonSerializer.Deserialize<T>(stream, GetSerializerOptions(options)));
    }

    public static async Task<T> ReadAsync(string path,
        JsonSerializerOptions? options = null)
    {
        await using var stream = File.OpenRead(path);
        
        return ThrowOnNull(await JsonSerializer.DeserializeAsync<T>(stream, GetSerializerOptions(options)));
    }

    public static T Read(string path,
        JsonSerializerOptions? options = null)
    {
        using var stream = File.OpenRead(path);
        return ThrowOnNull(JsonSerializer.Deserialize<T>(stream, GetSerializerOptions(options)));
    }

    public static T ReadFromString(string value,JsonSerializerOptions? options = null)
    {
        return ThrowOnNull(JsonSerializer.Deserialize<T>(value,
            GetSerializerOptions(options)));
    }

    public async Task WriteAsync(Stream stream,
        JsonSerializerOptions? options = null)
    {
        await JsonSerializer.SerializeAsync(stream, this,this.GetType(), GetSerializerOptions(options));
    }

    public void Write(Stream stream,
        JsonSerializerOptions? options = null)
    {
        JsonSerializer.Serialize(stream, this,this.GetType(), GetSerializerOptions(options));
    }

    public async Task WriteAsync(string path,
        JsonSerializerOptions? options = null)
    {
        await using var stream = File.OpenWrite(path);
        await JsonSerializer.SerializeAsync(stream, this,this.GetType(), GetSerializerOptions(options));
    }

    public void Write(string path,
        JsonSerializerOptions? options = null)
    {
        using var stream = File.OpenWrite(path);
        JsonSerializer.Serialize(stream, this, this.GetType(),GetSerializerOptions(options));
    }

    public string WriteToString(JsonSerializerOptions? options = null)
    {
        return JsonSerializer.Serialize(this, this.GetType(),GetSerializerOptions(options));
    }

    public override string ToString()
    {
        return WriteToString();
    }

    private static JsonSerializerOptions GetSerializerOptions(JsonSerializerOptions? options)
    {
        return options ?? GlobalsRegistry.Resolve<JsonSerializerOptions>();
    }

    private static T ThrowOnNull(T? value)
    {
        if (value == null)
        {
            throw new JsonException($"Deserialize returned null for {typeof(T).Name}");
        }

        return value;
    }
}