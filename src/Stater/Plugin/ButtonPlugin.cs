namespace Stater.Plugin;

public abstract class ButtonPlugin : IPlugin
{
    public abstract PluginOutput Start(PluginInput pluginInput);

    public virtual string Name => "ButtonPlugin";
}