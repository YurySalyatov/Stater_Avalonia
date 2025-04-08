using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Stater.Models;
using Stater.Models.Editors;

namespace Stater.ViewModels.Execution;

public class VariablesViewModel : ReactiveObject
{
    public VariablesViewModel(IProjectManager projectManager, IEditorManager editorManager)
    {
        projectManager
            .StateMachine
            .Subscribe(x =>
            {
                if (Variables != null &&
                    Variables.Count == x.Variables.Count && 
                    Variables.All(x.Variables.Contains) &&
                    x.Variables.All(Variables.Contains)) return;
                Variables = x.Variables.ToList();
            });
        _projectManager = projectManager;
        _editorManager = editorManager;

        CreateVariableCommand = ReactiveCommand.Create(CreateVariable);
    }

    private IProjectManager _projectManager;
    private IEditorManager _editorManager;

    [Reactive] public List<Variable> Variables { get; set; }

    private Variable _variable;
    private StateMachine? stateMachine;

    public Variable Variable
    {
        get => _variable;
        set
        {
            this.RaiseAndSetIfChanged(ref _variable, value);
            _editorManager.DoSelectVariable(value);
        }
    }

    public ICommand CreateVariableCommand { get; }

    private void CreateVariable() => _projectManager.CreateVariable();
}