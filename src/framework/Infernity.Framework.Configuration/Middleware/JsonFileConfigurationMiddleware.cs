using System.Text.Json;
using System.Text.Json.Nodes;

using Infernity.Framework.Core.Collections;
using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Configuration.Middleware;

public sealed class JsonFileConfigurationMiddleware(string path,int order) : MergingConfigurationMiddleware,IOrderable
{
    public int Order => order;

    protected override Optional<JsonObject> OnGetData(ConfigurationContext context)
    {
        try
        {
            using var stream = File.OpenRead(path);

            var rootData = JsonSerializer.Deserialize<JsonObject>(stream);

            if (rootData != null)
            {
                if (rootData.TryGetPropertyValue(context.SectionId,
                        out var sectionNode) && sectionNode is JsonObject sectionObject)
                {
                    return sectionObject;
                }
            }
        }
        catch (FileNotFoundException)
        {
            return Optional.None<JsonObject>();
        }
        catch (IOException ex)
        {
            throw new ConfigurationException(
                $"Error reading configuration file: {path} for section {context.SectionId}",
                ex);
        }
        
        return Optional.None<JsonObject>();
    }
}