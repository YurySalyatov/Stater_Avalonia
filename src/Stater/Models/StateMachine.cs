using System;
using System.Collections.Generic;

namespace Stater.Models;

public record StateMachine(
    Guid Guid,
    string Name,
    List<State> States,
    List<Transition> Transitions,
    List<Variable> Variables
)
{
    public StateMachine() : this(
        Guid.NewGuid(),
        "",
        new List<State>(),
        new List<Transition>(),
        new List<Variable>()
    )
    {
    }

    public State? StartState
    {
        get { return States.Find(x => x.Type == StateType.Start); }
    }

    public State? GetStateByGuid(Guid guid)
    {
        return States.Find(x => x.Guid == guid);
    }

    public Transition? GetTransitionByGuid(Guid guid)
    {
        return Transitions.Find(x => x.Guid == guid);
    }

    public Variable? GetVariableByGuid(Guid guid)
    {
        return Variables.Find(x => x.Guid == guid);
    }
}