using LTLState = AnalyzeStater.LTL.Graph.State;

namespace AnalyzeStater.LTL;

public class Atom: Formula
{
    private string Formula;
    
    public Atom(string formula)
    {
        Formula = formula;
    }

    public bool Equals(Object? obj)
    {
        return obj switch
        {
            null => false,
            Atom other => other.Formula == Formula,
            _ => false
        };
    }

    public bool Evaluate(LTLState state)
    {
        return state.Atoms.Contains(this);
    }

}