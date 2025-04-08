using System;
using Avalonia.Animation;

namespace Stater.Models.Editors;

public interface ITransitionEditor
{
    IObservable<Transition> Transition { get; }
    
    IObservable<int> StartSelectedPosName { get; }
    IObservable<int> EndSelectedPosName { get; }
    void DoSelectSubstring(Transition transition, int startPos, int endPos);

    void DoSelect(Transition transition);
    void Update(Transition transition);
}