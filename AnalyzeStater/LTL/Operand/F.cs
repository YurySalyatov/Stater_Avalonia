namespace AnalyzeStater.LTL.Operand;

public class F: Formula
{

    private Formula A;

    public F(Formula a)
    {
        A = a;
    }
    
    public bool Equals(object? obj)
    {
        return obj switch
        {
            null => false,
            F other => A.Equals(other.A),
            _ => false
        };
    }

    public bool Evaluate()
    {
        throw new NotImplementedException();
    }
}