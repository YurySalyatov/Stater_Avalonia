using System;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Stater.Models;
using Stater.Models.Editors;

namespace Stater.ViewModels.Editors;

public class StateMachineEditorViewModel : ReactiveObject
{
    public StateMachineEditorViewModel(IStateMachineEditor stateMachineEditor)
    {
        _stateMachineEditor = stateMachineEditor;

        SaveCommand = ReactiveCommand.Create(Save);
        StartSelectedPosName = 0;
        EndSelectedPosName = 0;
        
        
        _stateMachineEditor
            .StartSelectedPosName
            .Subscribe(x =>
            {
                StartSelectedPosName = x;
            });
        _stateMachineEditor
            .EndSelectedPosName
            .Subscribe(x =>
            {
                EndSelectedPosName = x;
            });
        stateMachineEditor
            .StateMachine
            .Subscribe(x =>
            {
                StateMachine = x;
                Name = x.Name;
            });
    }

    private readonly IStateMachineEditor _stateMachineEditor;

    public ICommand SaveCommand { get; }


    [Reactive] private StateMachine? StateMachine { get; set; }
    [Reactive] public string Name { get; set; }
    [Reactive]
    public int StartSelectedPosName { get; set; }
    
    [Reactive]
    public int EndSelectedPosName { get; set; }

    private void Save()
    {
        if (StateMachine == null) return;
        var newStateMachine = StateMachine with { Name = Name };
        _stateMachineEditor.Update(newStateMachine);
    }
}