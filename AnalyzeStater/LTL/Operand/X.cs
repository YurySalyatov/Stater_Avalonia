using LTLState = AnalyzeStater.LTL.Graph.State;

namespace AnalyzeStater.LTL.Operand;

public class X: Formula
{

    private Formula A;

    public X(Formula a)
    {
        A = a;
    }
    
    public bool Equals(object? obj)
    {
        return obj switch
        {
            null => false,
            X other => A.Equals(other.A),
            _ => false
        };
    }

    public bool Evaluate(LTLState state)
    {
        return state.Edges.Any(edge => A.Evaluate(edge.EndState));
    }
    
}