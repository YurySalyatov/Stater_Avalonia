namespace AnalyzeStater.LTL.Operand;

public class R: Formula
{

    private Formula A;

    private Formula B;

    public R(Formula a, Formula b)
    {
        A = a;
        B = b;
    }
    
    public bool Equals(object? obj)
    {
        return obj switch
        {
            null => false,
            R other => A.Equals(other.A) && B.Equals(other.B),
            _ => false
        };
    }

    public bool Evaluate()
    {
        throw new NotImplementedException();
    }
}