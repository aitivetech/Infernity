using System.Text.Json;
using System.Text.Json.Serialization;

using Infernity.Framework.Core.Collections;

namespace Infernity.Framework.Json;

public static class JsonSerializerOptionsExtensions
{
    extension(JsonSerializerOptions options)
    {
        public JsonSerializerOptions WithConverters(JsonConverter converter,
            params JsonConverter[] converters)
        {
            IReadOnlyList<JsonConverter> list = [converter, ..converters];

            return options.WithConverters(list);
        }

        public JsonSerializerOptions WithConverters(IReadOnlyList<JsonConverter> converters)
        {
            var newOptions = new JsonSerializerOptions(options);
            newOptions.Converters.AddAll(converters);

            return newOptions;
        }

        public JsonSerializerOptions WithoutConverterInstance(JsonConverter converter)
        {
            return options.WithoutConverters(c => c != converter);
        }

        public JsonSerializerOptions WithoutConverters(Func<JsonConverter, bool> predicate)
        {
            var targetConverters = options.Converters.Where(predicate);

            var newOptions = new JsonSerializerOptions(options);
            newOptions.Converters.Clear();
            newOptions.Converters.AddAll(targetConverters);

            return newOptions;
        }

        public JsonSerializerOptions WithoutConverters()
        {
            var newOptions = new JsonSerializerOptions(options);
            newOptions.Converters.Clear();
            
            return newOptions;
        }
    }
}