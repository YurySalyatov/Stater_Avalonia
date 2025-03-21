namespace AnalyzeStater.LTL;

public class Not(Formula a) : Formula
{

    private Formula A = a;

    public bool Equals(object? obj)
    {
        return obj switch
        {
            null => false,
            Not other => other.A.Equals(A),
            _ => false
        };
    }

    public bool Evaluate()
    {
        return !A.Evaluate();
    }
}