using System.Text.Json.Nodes;

using Infernity.Framework.Core.Functional;

namespace Infernity.Framework.Configuration;

public sealed class ConfigurationContext
{
    internal ConfigurationContext(string sectionId)
    {
        SectionId = sectionId;
    }
    
    public string SectionId { get; set; }
    
    public Optional<JsonObject> Data { get; set; }
}