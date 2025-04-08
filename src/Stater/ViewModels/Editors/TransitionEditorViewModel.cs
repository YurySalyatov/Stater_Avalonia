using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Stater.Models;
using Stater.Models.Editors;

namespace Stater.ViewModels.Editors;

public class TransitionEditorViewModel : ReactiveObject
{
    public TransitionEditorViewModel(ITransitionEditor transitionEditor, IProjectManager projectManager)
    {
        _transitionEditor = transitionEditor;
        _projectManager = projectManager;

        SaveCommand = ReactiveCommand.Create(Save);

        StartSelectedPosName = 0;
        EndSelectedPosName = 0;
        
        
        _transitionEditor
            .StartSelectedPosName
            .Subscribe(x =>
            {
                StartSelectedPosName = x;
            });
        _transitionEditor
            .EndSelectedPosName
            .Subscribe(x =>
            {
                EndSelectedPosName = x;
            });
        _transitionEditor
            .Transition
            .Subscribe(x =>
            {
                try
                {
                    Transition = x;
                    Name = x.Name;

                    var condition = x.Condition;
                    Condition = null;
                    Variable = null;
                    Value = null;

                    VariableMath = null;
                    MathType = 0;
                    EventValue = null;

                    if (condition is Condition.VariableCondition variableCondition)
                    {
                        Condition = variableCondition.ConditionType switch
                        {
                            Models.Condition.VariableCondition.ConditionTypeEnum.Lt => 0,
                            Models.Condition.VariableCondition.ConditionTypeEnum.Le => 0,
                            Models.Condition.VariableCondition.ConditionTypeEnum.Ne => 1,
                            Models.Condition.VariableCondition.ConditionTypeEnum.Gt => 2,
                            Models.Condition.VariableCondition.ConditionTypeEnum.Ge => 3,
                            _ => 0,
                        };
                        Console.WriteLine(Condition);

                        Variable = variableCondition.VariableGuid.ToString();
                        Value = variableCondition.Value.ToString();
                    }

                    if (x.Event is Event.VariableMath math)
                    {
                        VariableMath = math.VariableGuid.ToString();
                        MathType = math.MathType switch
                        {
                            Event.VariableMath.MathTypeEnum.Div => 0,
                            Event.VariableMath.MathTypeEnum.Sub => 1,
                            Event.VariableMath.MathTypeEnum.Sum => 2,
                            _ => 3,
                        };
                        EventValue = math.Value.ToString();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

        projectManager
            .StateMachine
            .Subscribe(x => { Variables = x.Variables.ConvertAll(y => y.Guid.ToString()); }
            );
    }

    private ITransitionEditor _transitionEditor;
    private IProjectManager _projectManager;
    [Reactive] private Transition? Transition { get; set; }

    [Reactive] public string Name { get; set; }

    [Reactive] public List<string> Variables { get; set; }
    [Reactive] public string Variable { get; set; }

    [Reactive] public int? Condition { get; set; }

    [Reactive] public string Value { get; set; }
    
    [Reactive]
    public int StartSelectedPosName { get; set; }
    
    [Reactive]
    public int EndSelectedPosName { get; set; }


    [Reactive] public string VariableMath { get; set; }
    [Reactive] public int MathType { get; set; }
    [Reactive] public string EventValue { get; set; }

    public ICommand SaveCommand { get; }

    private void Save()
    {
        if (Transition == null) return;
        Condition? condition = null;
        Event? @event = null;

        if (Variable != null && Value != null)
        {
            Console.WriteLine(Condition);
            var type = Condition switch
            {
                4 => Models.Condition.VariableCondition.ConditionTypeEnum.Lt,
                5 => Models.Condition.VariableCondition.ConditionTypeEnum.Le,
                0 => Models.Condition.VariableCondition.ConditionTypeEnum.Eq,
                1 => Models.Condition.VariableCondition.ConditionTypeEnum.Ne,
                2 => Models.Condition.VariableCondition.ConditionTypeEnum.Gt,
                3 => Models.Condition.VariableCondition.ConditionTypeEnum.Ge,
                _ => Models.Condition.VariableCondition.ConditionTypeEnum.Ge,
            };
            condition = new Condition.VariableCondition(
                VariableGuid: Guid.Parse(Variable),
                ConditionType: type,
                Value: VariableValueBuilder.fromString(Value)
            );
        }

        if (VariableMath != null && EventValue != null)
        {
            var type = MathType switch
            {
                0 => Event.VariableMath.MathTypeEnum.Sum,
                1 => Event.VariableMath.MathTypeEnum.Sub,
                2 => Event.VariableMath.MathTypeEnum.Mul,
                _ => Event.VariableMath.MathTypeEnum.Div,
            };
            @event = new Event.VariableMath(
                VariableGuid: Guid.Parse(VariableMath),
                MathType: type,
                Value: VariableValueBuilder.fromString(EventValue)
            );
        }


        var newTransition = Transition with { Name = Name, Condition = condition, Event = @event };
        _transitionEditor.Update(newTransition);
    }
}