using System;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Stater.Models;
using Stater.Models.Editors;

namespace Stater.ViewModels.Editors;

public class VariableEditorViewModel : ReactiveObject
{
    public VariableEditorViewModel(IVariableEditor variableEditor)
    {
        _variableEditor = variableEditor;
        Console.WriteLine(variableEditor);
        
        StartSelectedPosName = 0;
        EndSelectedPosName = 0;
        
        
        _variableEditor
            .StartSelectedPosName
            .Subscribe(x =>
            {
                StartSelectedPosName = x;
            });
        _variableEditor
            .EndSelectedPosName
            .Subscribe(x =>
            {
                EndSelectedPosName = x;
            });
        _variableEditor
            .Variable
            .Subscribe(x =>
            {
                try
                {
                    Variable = x;
                    Name = x.Name;
                    Value = x.StartValue.ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

        SaveCommand = ReactiveCommand.Create(Save);
    }

    private readonly IVariableEditor _variableEditor;

    public ICommand SaveCommand { get; }

    private void Save()
    {
        if (Variable == null) return;
        var variable = Variable with
        {
            Name = Name,
            StartValue = VariableValueBuilder.fromString(Value)
        };
        _variableEditor.Update(variable);
    }

    [Reactive]
    public int StartSelectedPosName { get; set; }
    
    [Reactive]
    public int EndSelectedPosName { get; set; }
    [Reactive] private Variable? Variable { get; set; }

    [Reactive] public string Name { get; set; }
    [Reactive] public string Value { get; set; }
}