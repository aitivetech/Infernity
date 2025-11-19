namespace Infernity.Framework.Plugins;


public interface IPlugin
{
    PluginId Id => Description.Id;
    
    PluginDescription Description { get; }
}