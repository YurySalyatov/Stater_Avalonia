using Stater.Plugin;

namespace SLXParser;

public class SLXPPlugin: ButtonFilePlugin
{
    public override PluginOutput Start(PluginInput pluginInput, string file)
    {
        // if (dialogResult != DialogResult.OK)
        // {
        // result = PluginOutput.From("DialogResult is not OK");
        // return result;
        // }
        var parser = new Parser(file);
        var stateflow = parser.Parse();
        var pluginStateflow = new Translator().Convert(stateflow);
        var result = PluginOutput.From("Ok");
        result.ChangedStateMachines.Add(pluginStateflow);
        return result;
    }
    
    public override string Name => "SLXParser";
}