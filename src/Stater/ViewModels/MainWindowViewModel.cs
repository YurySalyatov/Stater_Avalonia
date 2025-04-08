using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Windows.Input;
using Avalonia;
using DynamicData;
using ReactiveUI.Fody.Helpers;
using Stater.Models;
using ReactiveUI;
using SLXParser;
using Stater.Models.Editors;
using Stater.Plugin;

namespace Stater.ViewModels;

public record PathPluginDto(
    ButtonFilePlugin Plugin, string Path
);

public class MainWindowViewModel : ReactiveObject
{
    public MainWindowViewModel(IProjectManager projectManager, IEditorManager editorManager)
    {
        _projectManager = projectManager;
        _editorManager = editorManager;

        projectManager
            .StateMachines
            .Bind(out _stateMachines)
            .Subscribe();

        projectManager
            .Project
            .Subscribe(x => Project = x);

        projectManager
            .StateMachine
            .Subscribe(x => { StateMachine = x; });

        projectManager
            .IsVisibleFindLine
            .Subscribe(x => IsVisibleFindLine = x);
        
        projectManager
            .IsAnalyze
            .Subscribe(x => IsAnalyze = x);

        NewCommand = ReactiveCommand.Create(NewProject);
        OpenCommand = ReactiveCommand.Create<StreamReader>(OpenProject);
        SaveCommand = ReactiveCommand.Create<StreamWriter>(SaveProject);
        NewStateMachineCommand = ReactiveCommand.Create(NewStateMachine);
        NewStateCommand = ReactiveCommand.Create(NewState);
        UndoCommand = ReactiveCommand.Create(Undo);
        RedoCommand = ReactiveCommand.Create(Redo);
        ReBuildGraphCommand = ReactiveCommand.Create(ReBuildGraph);
        // SimpleAnalyzeGraphCommand = ReactiveCommand.Create(SimpleAnalyzeGraph);
        PluginButtinCommand = ReactiveCommand.Create<PathPluginDto>(StartButtonFilePlugin);
        ShowFindCommand = ReactiveCommand.Create(ShowFind);
        HideFindCommand = ReactiveCommand.Create(HideFind);
        ActiveAnalyzeCommand = ReactiveCommand.Create(ActiveAnalyze);
        DisableAnalyzeCommand = ReactiveCommand.Create(DisableAnalyze);
        UpCommand = ReactiveCommand.Create(Up);
        DownCommand = ReactiveCommand.Create(Down);
        LeftCommand = ReactiveCommand.Create(Left);
        RightCommand = ReactiveCommand.Create(Right);
    }

    private readonly IProjectManager _projectManager;
    private readonly IEditorManager _editorManager;

    [Reactive] public Project Project { get; private set; }

    [Reactive] public bool IsVisibleFindLine { get; private set; }
    [Reactive] public bool IsAnalyze { get; private set; }

    public List<IPlugin> Plugins =>
        new()
        {
            new SLXPPlugin()
        };

    private StateMachine _stateMachine;

    public StateMachine? StateMachine
    {
        get => _stateMachine;
        set
        {
            if (value == null) return;
            this.RaiseAndSetIfChanged(ref _stateMachine, value);
            var openStateMachine = _projectManager.OpenStateMachine(value.Guid);
            if (openStateMachine != null)
                _editorManager.DoSelectStateMachine(openStateMachine);
        }
    }


    private readonly ReadOnlyObservableCollection<StateMachine> _stateMachines;
    public ReadOnlyObservableCollection<StateMachine> StateMachines => _stateMachines;

    public ICommand NewCommand { get; }
    public ReactiveCommand<StreamReader, Unit> OpenCommand { get; }
    public ReactiveCommand<StreamWriter, Unit> SaveCommand { get; }
    public ReactiveCommand<PathPluginDto, Unit> PluginButtinCommand { get; }
    public ICommand NewStateMachineCommand { get; }
    public ICommand NewStateCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand RedoCommand { get; }
    
    public ICommand ReBuildGraphCommand { get; }
    
    public ICommand ShowFindCommand { get; }
    public ICommand HideFindCommand { get; }
    
    public ICommand ActiveAnalyzeCommand { get; }
    public ICommand DisableAnalyzeCommand { get; }
    
    public ICommand UpCommand { get; }
    public ICommand DownCommand { get; }
    public ICommand LeftCommand { get; }
    public ICommand RightCommand { get; }
    
    // public ICommand SimpleAnalyzeGraphCommand { get; }

    private void OpenProject(StreamReader sr)
    {
        _projectManager.LoadProject(sr);
    }

    private void NewProject()
    {
        _projectManager.CreateProject("New Project");
    }

    private void SaveProject(StreamWriter sw)
    {
        _projectManager.SaveProject(sw);
    }

    private void NewStateMachine()
    {
        var stateMachine = _projectManager.CreateStateMachine();
        _editorManager.DoSelectStateMachine(stateMachine);
    }

    private void NewState()
    {
        var state = _projectManager.CreateState();
        if (state != null) _editorManager.DoSelectState(state);
    }

    private void ReBuildGraph() => _projectManager.ReBuildGraph();

    private void Undo() => _projectManager.Undo();
    private void Redo() => _projectManager.Redo();

    private void StartButtonFilePlugin(PathPluginDto pathPluginDto)
    {
        var input = new PluginInput(
            Project: _projectManager.GetProject(),
            StateMachine: _projectManager.GetStateMachine(),
            StateMachines: _projectManager.GetStateMachines(),
            ProjectManager: _projectManager
        );
        var res = pathPluginDto.Plugin.Start(input, pathPluginDto.Path);

        _projectManager.ChangeStateMachines(res.ChangedStateMachines);
    }

    private void ShowFind()
    {
        _projectManager.ChangeVisibleLineFindToTrue();
    }

    private void HideFind()
    {
        _projectManager.ChangeVisibleLineFindToFalse();
    }
    
    private void ActiveAnalyze()
    {
        _projectManager.ChangeAnalyzeToTrue();
    }

    private void DisableAnalyze()
    {
        _projectManager.ChangeAnalyzeToFalse();
    }

    private const double ConstShift = 50.0;

    private void Up()
    {
        _projectManager.ShiftStateMachine(new Point(0, -ConstShift));
    }

    private void Down()
    {
        _projectManager.ShiftStateMachine(new Point(0, ConstShift));
    }

    private void Right()
    {
        _projectManager.ShiftStateMachine(new Point(ConstShift, 0));
    }

    private void Left()
    {
        _projectManager.ShiftStateMachine(new Point(-ConstShift, 0));
    }

    // private void SimpleAnalyzeGraph()
    // {
    //     _projectManager.SimpleAnalyzeGraph();
    // }
}