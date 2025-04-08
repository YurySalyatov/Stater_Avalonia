using System;

namespace Stater.Models.Editors;

public interface IStateMachineEditor
{
    IObservable<StateMachine> StateMachine { get; }
    IObservable<int> StartSelectedPosName { get; }
    IObservable<int> EndSelectedPosName { get; }
    void DoSelectSubstring(StateMachine stateMachine, int startPos, int endPos);

    void DoSelect(StateMachine stateMachine);
    void Update(StateMachine stateMachine);
}