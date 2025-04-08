using System;
using System.Reactive.Subjects;

namespace Stater.Models.Editors.impl;

public class TransitionEditor(
    IProjectManager projectManager
) : ITransitionEditor
{
    private readonly ReplaySubject<Transition> _transition = new();
    public IObservable<Transition> Transition => _transition;
    
    public void DoSelect(Transition transition)
    {
        _startSelectedPosName.OnNext(0);
        _endSelectedPosName.OnNext(0);
        _transition.OnNext(transition);
    }

    public void Update(Transition transition)
    {
        projectManager.UpdateTransition(transition);
    }
    
    public void DoSelectSubstring(Transition transition, int startPos, int endPos)
    {
        _startSelectedPosName.OnNext(startPos);
        _endSelectedPosName.OnNext(endPos);
        _transition.OnNext(transition);
    }
    
    private readonly ReplaySubject<int> _startSelectedPosName = new();
    private readonly ReplaySubject<int> _endSelectedPosName = new();

    public IObservable<int> StartSelectedPosName => _startSelectedPosName;
    public IObservable<int> EndSelectedPosName => _endSelectedPosName;
}