using System;

namespace SLXParser;

internal class SLXProgram
{
    public static void Start(string[] args)
    {
        const string path = "/Users/vnazarov/PycharmProjects/ya-hakaton/BR_GATES_HDL.slx";

        var parser = new Parser(path);
        var stateflow = parser.Parse();
        var pluginStateflow = new Translator().Convert(stateflow);

        Console.WriteLine(stateflow.Machine.Chart.ChildrenState);
        Console.WriteLine(stateflow.Machine.Chart.ChildrenState.Count);
    }
}