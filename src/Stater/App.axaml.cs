using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Splat;
using Stater.Models;
using Stater.Models.Editors;
using Stater.Models.Editors.impl;
using Stater.Models.Executor;
using Stater.Models.Executor.impl;
using Stater.Models.impl;
using Stater.ViewModels;
using Stater.Views;

namespace Stater;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT

            InitDependency();

            var projectManager = Locator.Current.GetService<IProjectManager>()!;
            var editorManager = Locator.Current.GetService<IEditorManager>()!;


            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(projectManager, editorManager),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void InitDependency()
    {
        var projectManager = new ProjectManager();
        var stateMachineEditor = new StateMachineEditor(projectManager);
        var stateEditor = new StateEditor(projectManager);
        var transitionEditor = new TransitionEditor(projectManager);
        var variableEditor = new VariableEditor(projectManager);

        var execitor = new Executor(projectManager);
        
        Locator.CurrentMutable.RegisterConstant(projectManager, typeof(IProjectManager));

        Locator.CurrentMutable.RegisterConstant(
            new EditorManager(stateMachineEditor, stateEditor, transitionEditor, variableEditor),
            typeof(IEditorManager));
        Locator.CurrentMutable.RegisterConstant(stateMachineEditor, typeof(IStateMachineEditor));
        Locator.CurrentMutable.RegisterConstant(stateEditor, typeof(IStateEditor));
        Locator.CurrentMutable.RegisterConstant(transitionEditor, typeof(ITransitionEditor));
        Locator.CurrentMutable.RegisterConstant(variableEditor, typeof(IVariableEditor));
        Locator.CurrentMutable.RegisterConstant(execitor, typeof(IExecutor));
    }
}