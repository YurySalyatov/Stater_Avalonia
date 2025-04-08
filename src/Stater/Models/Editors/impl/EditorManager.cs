using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using Stater.Models.FindLine;

namespace Stater.Models.Editors.impl;

public class EditorManager : IEditorManager
{
    private readonly ReplaySubject<EditorTypeEnum> _editorType = new();
    public IObservable<EditorTypeEnum> EditorType => _editorType;

    private IStateMachineEditor _stateMachineEditor;
    private IStateEditor _stateEditor;
    private ITransitionEditor _transitionEditor;
    private IVariableEditor _variableEditor;

    public void DoSelectNull()
    {
        _editorType.OnNext(EditorTypeEnum.Null);
    }

    public EditorManager(
        IStateMachineEditor stateMachineEditor,
        IStateEditor stateEditor,
        ITransitionEditor transitionEditor,
        IVariableEditor variableEditor)
    {
        _editorType.OnNext(EditorTypeEnum.Null);
        _stateMachineEditor = stateMachineEditor;
        _stateEditor = stateEditor;
        _transitionEditor = transitionEditor;
        _variableEditor = variableEditor;
    }


    public void DoSelectStateMachine(StateMachine stateMachine)
    {
        _stateMachineEditor.DoSelect(stateMachine);
        _editorType.OnNext(EditorTypeEnum.StateMachine);
    }
    
    public void DoSelectSubstringStateMachine(StateMachine stateMachine, int startIndex, int endIndex)
    {
        _stateMachineEditor.DoSelectSubstring(stateMachine, startIndex, endIndex);
        _editorType.OnNext(EditorTypeEnum.StateMachine);
    }

    public void DoSelectState(State state)
    {
        _stateEditor.DoSelect(state);
        _editorType.OnNext(EditorTypeEnum.State);
    }

    public void DoSelectSubstringState(State state, int startIndex, int endIndex, bool isDescription)
    {
        _stateEditor.DoSelectSubstring(state, startIndex, endIndex, isDescription);
        _editorType.OnNext(EditorTypeEnum.State);
    }


    public void DoSelectTransition(Transition transition)
    {
        _transitionEditor.DoSelect(transition);
        _editorType.OnNext(EditorTypeEnum.Transition);
    }
    
    public void DoSelectSubstringTransition(Transition transition, int startIndex, int endIndex)
    {
        _transitionEditor.DoSelectSubstring(transition, startIndex, endIndex);
        _editorType.OnNext(EditorTypeEnum.Transition);
    }

    public void DoSelectVariable(Variable variable)
    {
        _variableEditor.DoSelect(variable);
        _editorType.OnNext(EditorTypeEnum.Variable);
    }
    public void DoSelectSubstringVariable(Variable variable, int startIndex, int endIndex)
    {
        _variableEditor.DoSelectSubstring(variable, startIndex, endIndex);
        _editorType.OnNext(EditorTypeEnum.Variable);
    }

    public void DoLoadSearch(List<SearchContainer> containers)
    {
        _stateEditor.LoadPosition(containers.Where(el => el.EditorType == EditorTypeEnum.State).ToList());
    }

    public void DoUnLoadSearch()
    {
        _stateEditor.UnLoadPosition();
    }
}