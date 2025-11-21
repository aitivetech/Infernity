using System.Text.Json;
using System.Text.Json.Nodes;

using Infernity.Framework.Core.Functional;
using Infernity.Framework.Core.Patterns;
using Infernity.Framework.Core.Pipelines;

namespace Infernity.Framework.Configuration.Default;

public sealed class DefaultConfigurationReader : IConfigurationReader
{
    private readonly Action<ConfigurationContext> _pipeline;
    private readonly JsonSerializerOptions _serializerOptions;

    public DefaultConfigurationReader(IEnumerable<IConfigurationMiddleware> configurationHandlers,
        JsonSerializerOptions? serializerOptions = null)
    {
        _pipeline = configurationHandlers.Compile();
        _serializerOptions = serializerOptions ?? GlobalsRegistry.Resolve<JsonSerializerOptions>();
    }

    public Optional<JsonObject> ReadData(string sectionId)
    {
        var context = new ConfigurationContext(sectionId);
        _pipeline(context);

        return context.Data;
    }

    public Optional<object> Read(string sectionId,
        Type type)
    {
        try
        {
            return ReadData(sectionId).Select(o => o.Deserialize(type,
                _serializerOptions).NullableAsOptional()).Flatten();
        }
        catch (JsonException ex)
        {
            throw new ConfigurationException($"Invalid configuration section data: {sectionId}, target {type.Name}",
                ex);
        }
    }
}