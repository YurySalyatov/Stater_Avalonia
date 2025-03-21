namespace AnalyzeStater.LTL.Operand;

public class G: Formula
{

    private Formula A;

    public G(Formula a)
    {
        A = a;
    }
    
    public bool Equals(object? obj)
    {
        return obj switch
        {
            null => false,
            G other => A.Equals(other.A),
            _ => false
        };
    }

    public bool Evaluate()
    {
        throw new NotImplementedException();
    }
}