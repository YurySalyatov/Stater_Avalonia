using AnalyzeStater.LTL.Graph;

using LTLState = AnalyzeStater.LTL.Graph.State;

public interface Formula
{
    public bool Equals(Object? other);

    public bool Evaluate(LTLState state);
}