namespace AnalyzeStater.LTL;

public class Or: Formula
{
    private Formula A;
    private Formula B;
    
    public Or(Formula a, Formula b)
    {
        A = a;
        B = b;
    }
    public bool Equals(object? obj)
    {
        return obj switch
        {
            null => false,
            Or other => (other.A.Equals(A) && other.B.Equals(B)) || (other.A.Equals(B) && other.B.Equals(A)),
            _ => false
        };
    }

    public bool Evaluate()
    {
        return A.Evaluate() || B.Evaluate();
    }
}