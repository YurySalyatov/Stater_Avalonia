namespace AnalyzeStater.LTL.Graph;

public interface State
{
    List<Transition> Edges { get; set; }
    List<Atom> Atoms { get; set; }
}