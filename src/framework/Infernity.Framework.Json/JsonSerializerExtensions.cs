using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Infernity.Framework.Json;

public static class JsonSerializerExtensions
{
    extension(JsonSerializer)
    {
        public static object? DeserializeFromFile(
            string path,
            Type type,
            JsonSerializerOptions? options = null)
        {
            using var stream = File.OpenRead(path);
            
            return JsonSerializer.Deserialize(stream,
                type,
                options);
        }
        
        public static T? DeserializeFromFile<T>(
            string path,
            JsonSerializerOptions? options = null)
        {
            using var stream = File.OpenRead(path);
            
            return JsonSerializer.Deserialize<T>(stream,
                options);
        }
        
        public static void SerializeToFile(
            string path,
            object value,
            Type? type = null,
            JsonSerializerOptions? options = null)
        {
            using var stream = File.OpenWrite(path);

            var finalType = type ?? value.GetType();
            
            JsonSerializer.Serialize(stream,
                value,
                finalType,
                options);
        }
        
        public static void SerializeToFile<T>(
            string path,
            T value,
            JsonSerializerOptions? options = null)
        {
            using var stream = File.OpenWrite(path);

            JsonSerializer.Serialize<T>(stream,
                value,
                options);
        }
    }
}