using System;
using System.Collections.Generic;
using Stater.Models.FindLine;

namespace Stater.Models.Editors;

public enum EditorTypeEnum
{
    Null,
    StateMachine,
    State,
    Transition,
    Variable
}

public interface IEditorManager
{
    IObservable<EditorTypeEnum> EditorType { get; }

    void DoSelectNull();
    
    void DoSelectStateMachine(StateMachine stateMachine);
    void DoSelectSubstringStateMachine(StateMachine stateMachine, int startIndex, int endIndex);
    void DoSelectState(State state);
    void DoSelectSubstringState(State state, int startIndex, int endIndex, bool isDescription);
    void DoSelectTransition(Transition transition);
    void DoSelectSubstringTransition(Transition transition, int startIndex, int endIndex);
    void DoSelectVariable(Variable variable);
    void DoSelectSubstringVariable(Variable variable, int startIndex, int endIndex);
    void DoLoadSearch(List<SearchContainer> containers);
    void DoUnLoadSearch();
}