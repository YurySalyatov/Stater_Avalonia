using Stater.Models;

namespace AnalyzeStater.PreparingStater;

public class UnionStater
{
    private StateMachine unionStateMachine;
    
    private List<StateMachine> stateMachines;
    
    public UnionStater(List<StateMachine> stateMachineList)
    {
        stateMachines = stateMachineList;
    }

    public void UnionStateMachines()
    {
        // TODO
        // unionStateMachine = ...
        unionStateMachine = new StateMachine();
    }

    public void TranslateToLTLLogic()
    {
        // TODO
        // translate to ltl
    }

    public void AnalyzeUnionStateMachine()
    {
        // TODO
    }

    public void SimplifyUnionStateMachine()
    {
        //TODO
    }

    public List<StateMachine> UnUnionStateMachines()
    {
        // TODO
        return stateMachines;
    }
    
}