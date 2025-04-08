using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Subjects;
using Stater.Models.FindLine;
using Stater.Models.impl;
using Stater.ViewModels.FindLine;

namespace Stater.Models.Editors.impl;

public class StateEditor(IProjectManager projectManager): IStateEditor
{
    private readonly ReplaySubject<State> _state = new();
    public IObservable<State> State => _state;
    
    public void DoSelect(State state)
    {
        _startSelectedPosName.OnNext(0);
        _endSelectedPosName.OnNext(0);
        _startSelectedPosDescription.OnNext(0);
        _endSelectedPosDescription.OnNext(0);
        _state.OnNext(state);
    }

    public void Update(State state)
    {
        projectManager.UpdateState(state);
    }

    public void DoSelectSubstring(State state, int startPos, int endPos, bool isDescription)
    {
        if (isDescription)
        {
            _startSelectedPosDescription.OnNext(startPos);
            _endSelectedPosDescription.OnNext(endPos);
            _startSelectedPosName.OnNext(0);
            _endSelectedPosName.OnNext(0);
        }

        if (!isDescription)
        {
            _startSelectedPosDescription.OnNext(0);
            _endSelectedPosDescription.OnNext(0);
            _startSelectedPosName.OnNext(startPos);
            _endSelectedPosName.OnNext(endPos);
        }
        _state.OnNext(state);
    }

    public void LoadPosition(List<SearchContainer> containers)
    {
        foreach (var container in containers)
        {
            if (!container.IsDescription)
            {
                _stateGuidToPosName[container.Guid] = new KeyValuePair<int, int>(container.StartPos, container.EndPos);
            }

            if (container.IsDescription)
            {
                _stateGuidToPosDescription[container.Guid] = new KeyValuePair<int, int>(container.StartPos, container.EndPos);
            }
        }
    }

    public void UnLoadPosition()
    {
        _stateGuidToPosDescription.Clear();
        _stateGuidToPosDescription.Clear();
    }

    private readonly ReplaySubject<int> _startSelectedPosName = new();
    private readonly ReplaySubject<int> _endSelectedPosName = new();

    public IObservable<int> StartSelectedPosName => _startSelectedPosName;
    public IObservable<int> EndSelectedPosName => _endSelectedPosName;
    
    private readonly ReplaySubject<int> _startSelectedPosDescription = new();
    private readonly ReplaySubject<int> _endSelectedPosDescription = new();
    public IObservable<int> StartSelectedPosDescription => _startSelectedPosDescription;
    public IObservable<int> EndSelectedPosDescription => _endSelectedPosDescription;
    
    private Dictionary<Guid, KeyValuePair<int, int>> _stateGuidToPosName = new();

    private Dictionary<Guid, KeyValuePair<int, int>> _stateGuidToPosDescription = new();
}