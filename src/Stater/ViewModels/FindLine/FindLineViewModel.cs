using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Stater.Models;
using Stater.Models.Editors;
using Stater.Models.FindLine;
using Stater.Utils;

namespace Stater.ViewModels.FindLine;

public class FindLineViewModel : ReactiveObject
{
    public FindLineViewModel(IProjectManager projectManager, IEditorManager editorManager)
    {
        _projectManager = projectManager;
        AllPos = 0;
        SearchPos = 0;
        IsVisible = false;
        _projectManager
            .IsVisibleFindLine
            .Subscribe(x =>
            {
                IsVisible = x;
                if (!x)
                {
                    IsVisibleReplace = false;
                }
                // if (!x)
                // {
                //     _editorManager.DoUnLoadSearch();
                // }
            });
        _editorManager = editorManager;
        SearchCommand = ReactiveCommand.Create(Search);
        NextCommand = ReactiveCommand.Create(Next);
        PrevCommand = ReactiveCommand.Create(Prev);
        ReplaceCurrentCommand = ReactiveCommand.Create(ReplaceCurrent);
        ReplaceAllCommand = ReactiveCommand.Create(ReplaceAll);
        ChangeReplaceCommand = ReactiveCommand.Create(ChangeReplace);
        CloseFindLineCommand = ReactiveCommand.Create(CloseFindLine);
    }

    private IProjectManager _projectManager;
    private IEditorManager _editorManager;

    [Reactive] public string SearchText { get; set; } = "";
    [Reactive] public string ReplaceText { get; set; } = "";

    [Reactive] public bool IsVisible { get; private set; }
    [Reactive] public bool IsVisibleReplace { get; private set; }

    private List<SearchContainer> _searchContainers = [];

    [Reactive] public int AllPos { get; set; }
    [Reactive] public int SearchPos { get; set; }

    private int pos;

    public ICommand SearchCommand { get; set; }
    public ICommand NextCommand { get; set; }
    public ICommand PrevCommand { get; set; }
    
    public ICommand ChangeReplaceCommand { get; set; }

    public ICommand ReplaceCurrentCommand { get; set; }

    public ICommand ReplaceAllCommand { get; set; }
    
    public ICommand CloseFindLineCommand { get; set; }

    private void Search()
    {
        if (string.IsNullOrEmpty(SearchText)) return;
        _searchContainers = [];
        pos = 0;
        SearchPos = 0;
        AllPos = 0;
        foreach (var stateMachine in _projectManager.GetStateMachines())
        {
            AddContainers(stateMachine);
        }

        if (_searchContainers.Count != 0)
        {
            SearchPos = 1;
            AllPos = _searchContainers.Count;
        }
        // LoadContainers();
        ShowCurContainer();
    }

    private void LoadContainers()
    {
        _editorManager.DoLoadSearch(_searchContainers);
    }

    public void ChangeReplace()
    {
        IsVisibleReplace = !IsVisibleReplace;
    }
    
    private void AddOneInContainer(EditorTypeEnum editorType, Guid stateMachineGuid, Guid guid, List<List<int>> pairs,
        bool isDescription = false)
    {
        foreach (var pair in pairs)
        {
            _searchContainers.Add(new()
            {
                EditorType = editorType,
                StartPos = pair[0],
                EndPos = pair[1],
                Guid = guid,
                IsDescription = isDescription,
                StateMachineGuid = stateMachineGuid
            });
        }
    }

    private void AddContainers(StateMachine stateMachine)
    {
        var stateMachinePairs = FindUtils.FindAllSubstings(stateMachine.Name, SearchText);
        AddOneInContainer(EditorTypeEnum.StateMachine, stateMachine.Guid, stateMachine.Guid, stateMachinePairs);
        foreach (var state in stateMachine.States)
        {
            var statePair = FindUtils.FindAllSubstings(state.Name, SearchText);
            AddOneInContainer(EditorTypeEnum.State, stateMachine.Guid, state.Guid, statePair);
            statePair = FindUtils.FindAllSubstings(state.Description, SearchText);
            AddOneInContainer(EditorTypeEnum.State, stateMachine.Guid, state.Guid, statePair, true);
        }

        foreach (var transition in stateMachine.Transitions)
        {
            var transitionPair = FindUtils.FindAllSubstings(transition.Name, SearchText);
            AddOneInContainer(EditorTypeEnum.Transition, stateMachine.Guid, transition.Guid, transitionPair);
        }

        foreach (var variable in stateMachine.Variables)
        {
            var variablePair = FindUtils.FindAllSubstings(variable.Name, SearchText);
            AddOneInContainer(EditorTypeEnum.Variable, stateMachine.Guid, variable.Guid, variablePair);
        }
    }

    private void Next()
    {
        if (_searchContainers.Count == 0) return;
        if (pos >= _searchContainers.Count - 1) pos = 0;
        else pos += 1;
        SearchPos = pos + 1;
        ShowCurContainer();
    }

    private void Prev()
    {
        if (_searchContainers.Count == 0) return;
        if (pos <= 0) pos = _searchContainers.Count - 1;
        else pos -= 1;
        SearchPos = pos + 1;
        ShowCurContainer();
    }

    private void ShowCurContainer()
    {
        if (_searchContainers.Count == 0)
        {
            _editorManager.DoSelectNull();
            return;
        }

        var curContainer = _searchContainers[pos];
        switch (curContainer.EditorType)
        {
            case EditorTypeEnum.StateMachine:
            {
                var stateMachine = _projectManager.GetStateMachineByGuid(curContainer.StateMachineGuid);
                if (stateMachine == null) return;
                _editorManager
                    .DoSelectSubstringStateMachine(stateMachine, curContainer.StartPos, curContainer.EndPos);
                break;
            }
            case EditorTypeEnum.State:
            {
                var state = _projectManager.GetStateMachineByGuid(curContainer.StateMachineGuid)?
                    .GetStateByGuid(curContainer.Guid);
                if (state == null) return;
                _editorManager
                    .DoSelectSubstringState(state, curContainer.StartPos, curContainer.EndPos,
                        curContainer.IsDescription);
                break;
            }
            case EditorTypeEnum.Variable:
            {
                var variable = _projectManager.GetStateMachineByGuid(curContainer.StateMachineGuid)?
                    .GetVariableByGuid(curContainer.Guid);
                if (variable == null) return;
                _editorManager.DoSelectSubstringVariable(variable, curContainer.StartPos, curContainer.EndPos);
                break;
            }
            case EditorTypeEnum.Transition:
            {
                var transition = _projectManager.GetStateMachineByGuid(curContainer.StateMachineGuid)?
                    .GetTransitionByGuid(curContainer.Guid);
                if (transition == null) return;
                _editorManager.DoSelectSubstringTransition(transition, curContainer.StartPos, curContainer.EndPos);
                break;
            }
            case EditorTypeEnum.Null:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void DoSelect(SearchContainer container)
    {
        var stateMachine = _projectManager.GetStateMachineByGuid(container.StateMachineGuid);
        if(stateMachine == null) return;
        switch (container.EditorType)
        {
            case EditorTypeEnum.StateMachine:
            {
                _editorManager.DoSelectStateMachine(stateMachine);
                break;
            }
            case EditorTypeEnum.State:
            {
                var state = stateMachine.GetStateByGuid(container.Guid);
                if(state == null) return;
                _editorManager.DoSelectState(state);
                break;
            }
            case EditorTypeEnum.Transition:
            {
                var transition = stateMachine.GetTransitionByGuid(container.Guid);
                if (transition == null) return;
                _editorManager.DoSelectTransition(transition);
                break;
            }
            case EditorTypeEnum.Variable:
            {
                var variable = stateMachine.GetVariableByGuid(container.Guid);
                if (variable == null) return;
                _editorManager.DoSelectVariable(variable);
                break;
            }
            case EditorTypeEnum.Null:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    private void ReplaceCurrent()
    {
        if (_searchContainers.Count == 0) return;
        if (string.IsNullOrEmpty(ReplaceText)) return;
        var curContainer = _searchContainers[pos];
        ReplaceOne(pos);
        Search();
        DoSelect(curContainer);
    }

    private void ReplaceAll()
    {
        if (_searchContainers.Count == 0) return;
        if (string.IsNullOrEmpty(ReplaceText)) return;
        var curContainer = _searchContainers[pos];
        for (var i = 0; i < _searchContainers.Count; i++)
        {
            ReplaceOne(i);
        }
        Search();
        DoSelect(curContainer);
    }

    private void ReplaceOne(int replacePos)
    {
        var curContainer = _searchContainers[replacePos];
        switch (curContainer.EditorType)
        {
            case EditorTypeEnum.StateMachine:
            {
                var stateMachine = _projectManager.GetStateMachineByGuid(curContainer.StateMachineGuid);
                if (stateMachine == null) return;
                var newStateMachine = stateMachine with
                {
                    Name = stateMachine.Name[..curContainer.StartPos] + ReplaceText +
                           stateMachine.Name[curContainer.EndPos..]
                };
                _projectManager.UpdateStateMachine(newStateMachine);
                break;
            }
            case EditorTypeEnum.State:
            {
                var stateMachine = _projectManager.GetStateMachineByGuid(curContainer.StateMachineGuid);
                var state = stateMachine?.States.Find(el => el.Guid == curContainer.Guid);
                if (state == null) return;
                if (!curContainer.IsDescription)
                {
                    var newState = state with
                    {
                        Name = state.Name[..curContainer.StartPos] + ReplaceText + state.Name[curContainer.EndPos..]
                    };
                    _projectManager.UpdateStateInStateMachine(newState, stateMachine!);
                }

                if (curContainer.IsDescription)
                {
                    var newState = state with
                    {
                        Description = state.Description[..curContainer.StartPos] + ReplaceText +
                                      state.Description[curContainer.EndPos..]
                    };
                    _projectManager.UpdateStateInStateMachine(newState, stateMachine!);
                }

                break;
            }
            case EditorTypeEnum.Transition:
            {
                var stateMachine = _projectManager.GetStateMachineByGuid(curContainer.StateMachineGuid);
                var transition = stateMachine?.Transitions.Find(el => el.Guid == curContainer.Guid);
                if (transition == null) return;
                var newTransition = transition with
                {
                    Name = transition.Name[..curContainer.StartPos] + ReplaceText + transition.Name[curContainer.EndPos..]
                };
                _projectManager.UpdateTransitionInStateMachine(newTransition, stateMachine!);
                break;
            }
            case EditorTypeEnum.Variable:
            {
                var stateMachine = _projectManager.GetStateMachineByGuid(curContainer.StateMachineGuid);
                var variable = stateMachine?.Variables.Find(el => el.Guid == curContainer.Guid);
                if (variable == null) return;
                var newVariable = variable with
                {
                    Name = variable.Name[..curContainer.StartPos] + ReplaceText + variable.Name[curContainer.EndPos..]
                };
                _projectManager.UpdateVariableInStateMachine(newVariable, stateMachine!);
                break;
            }
            case EditorTypeEnum.Null:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void CloseFindLine()
    {
        _projectManager.ChangeVisibleLineFindToFalse();
    }
}