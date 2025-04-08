namespace Stater.Plugin;

public abstract class ButtonFilePlugin : IPlugin
{
    public abstract PluginOutput Start(PluginInput pluginInput, string path);
    
    public virtual string Name => "ButtonFilePlugin";
}