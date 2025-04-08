using System;
using System.Reactive.Subjects;

namespace Stater.Models.Editors.impl;

public class VariableEditor(
    IProjectManager projectManager
) : IVariableEditor
{
    private readonly ReplaySubject<Variable> _variable = new();
    public IObservable<Variable> Variable => _variable;
    
    public void DoSelect(Variable variable)
    {
        _variable.OnNext(variable);
    }

    public void Update(Variable variable)
    {
        projectManager.UpdateVariable(variable);
    }
    
    public void DoSelectSubstring(Variable variable, int startPos, int endPos)
    {
        _startSelectedPosName.OnNext(startPos);
        _endSelectedPosName.OnNext(endPos);
        _variable.OnNext(variable);
    }
    
    private readonly ReplaySubject<int> _startSelectedPosName = new();
    private readonly ReplaySubject<int> _endSelectedPosName = new();

    public IObservable<int> StartSelectedPosName => _startSelectedPosName;
    public IObservable<int> EndSelectedPosName => _endSelectedPosName;
}