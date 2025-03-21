namespace AnalyzeStater.LTL.Graph;

public interface Transition
{
    State StartState { get; set; }
    State EndState { get; set; }
    string Event { get; set; }
}