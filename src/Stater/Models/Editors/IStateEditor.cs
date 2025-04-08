using System;
using System.Collections.Generic;
using Stater.Models.FindLine;
using Stater.ViewModels.FindLine;
using Stater.Views.FindLine;

namespace Stater.Models.Editors;

public interface IStateEditor
{
    IObservable<State> State { get; }

    void DoSelect(State state);
    void Update(State state);

    IObservable<int> StartSelectedPosName { get; }
    IObservable<int> EndSelectedPosName { get; }
    IObservable<int> StartSelectedPosDescription { get; }
    IObservable<int> EndSelectedPosDescription { get; }
    
    void DoSelectSubstring(State state, int startPos, int endPos, bool isDescription);
    void LoadPosition(List<SearchContainer> containers);
    void UnLoadPosition();
}