using System;
using System.Reactive.Subjects;

namespace Stater.Models.Editors.impl;

public class StateMachineEditor(
    IProjectManager projectManager
) : IStateMachineEditor
{
    private readonly ReplaySubject<StateMachine> _stateMachine = new();
    public IObservable<StateMachine> StateMachine => _stateMachine;

    public void DoSelect(StateMachine stateMachine)
    {
        _startSelectedPosName.OnNext(0);
        _endSelectedPosName.OnNext(0);
        _stateMachine.OnNext(stateMachine);
    }

    public void Update(StateMachine stateMachine)
    {
        projectManager.UpdateStateMachine(stateMachine);
    }
    
    public void DoSelectSubstring(StateMachine stateMachine, int startPos, int endPos)
    {
        _startSelectedPosName.OnNext(startPos);
        _endSelectedPosName.OnNext(endPos);
        _stateMachine.OnNext(stateMachine);
    }
    
    private readonly ReplaySubject<int> _startSelectedPosName = new();
    private readonly ReplaySubject<int> _endSelectedPosName = new();

    public IObservable<int> StartSelectedPosName => _startSelectedPosName;
    public IObservable<int> EndSelectedPosName => _endSelectedPosName;
}