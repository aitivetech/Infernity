namespace Infernity.Framework.Plugins;

public sealed record PluginDescription(
    PluginId Id, 
    string Version,
    bool IsBuiltin)
{
    
}