namespace Infernity.Framework.Plugins.Web;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class WebPluginAttribute : System.Attribute
{
    public WebPluginAttribute(
        bool enableEndpoints = true,
        bool enableWebParts = true,
        bool enableBlazor = true)
    {
        EnableEndpoints = enableEndpoints;
        EnableWebParts = enableWebParts;
        EnableBlazor = enableBlazor;
    }

    public bool EnableEndpoints { get; }
    
    public bool EnableWebParts { get; }
    
    public bool EnableBlazor { get; }
}