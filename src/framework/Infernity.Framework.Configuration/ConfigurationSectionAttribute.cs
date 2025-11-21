namespace Infernity.Framework.Configuration;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ConfigurationSectionAttribute : System.Attribute
{
    public ConfigurationSectionAttribute(string id)
    {
        Id = id;
    }

    public string Id { get; }
}