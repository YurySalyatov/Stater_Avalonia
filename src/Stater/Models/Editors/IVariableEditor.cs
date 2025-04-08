using System;

namespace Stater.Models.Editors;

public interface IVariableEditor
{
    IObservable<Variable> Variable { get; }
    IObservable<int> StartSelectedPosName { get; }
    IObservable<int> EndSelectedPosName { get; }
    void DoSelectSubstring(Variable variable, int startPos, int endPos);
    
    void DoSelect(Variable transition);
    void Update(Variable transition);
}