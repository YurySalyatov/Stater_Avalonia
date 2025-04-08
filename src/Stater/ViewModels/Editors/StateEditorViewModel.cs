using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Windows.Input;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Platform;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Stater.Models;
using Stater.Models.Editors;
using Stater.Utils;

namespace Stater.ViewModels.Editors;

public class StateEditorViewModel : ReactiveObject
{
    public StateEditorViewModel(IStateEditor stateEditor, IProjectManager projectManager, IEditorManager editorManager)
    {
        _stateEditor = stateEditor;
        _projectManager = projectManager;
        _editorManager = editorManager;

        SaveCommand = ReactiveCommand.Create(Save);
        StartSelectedPosName = 0;
        EndSelectedPosName = 0;
        StartSelectedPosDescription = 0;
        EndSelectedPosDescription = 0;
        
        
        _stateEditor
            .StartSelectedPosName
            .Subscribe(x =>
            {
                StartSelectedPosName = x;
            });
        _stateEditor
            .EndSelectedPosName
            .Subscribe(x =>
            {
                EndSelectedPosName = x;
            });
        _stateEditor
            .StartSelectedPosDescription
            .Subscribe(x => StartSelectedPosDescription = x);
        _stateEditor
            .EndSelectedPosDescription
            .Subscribe(x => EndSelectedPosDescription = x);
        
        _stateEditor
            .State
            .Subscribe(x =>
            {
                try
                {
                    State = x;
                    Name = x.Name;
                    Description = x.Description;
                    TypeIndex = x.Type switch
                    {
                        StateType.Common => 0,
                        StateType.Start => 1,
                        StateType.End => 2,
                        _ => TypeIndex
                    };
                    Width = x.Width.ToString(CultureInfo.InvariantCulture);
                    Height = x.Height.ToString(CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

        projectManager
            .StateMachine
            .Subscribe(x =>
            {
                AllStates = x.States;
                Transitions = x.Transitions.Select(y =>
                        {
                            var startState = x.States.Find(s => s.Guid == y.Start)!;
                            var endState = x.States.Find(s => s.Guid == y.End)!;
                            return DrawUtils.GetTransition(startState, endState, y, false);
                        }
                    ).Where(y => y != null)
                    .OfType<DrawArrows>()
                    .ToList();
            });


        AddTransitionCommand = ReactiveCommand.Create<State>(AddTransition);
        RemoveTransitionCommand = ReactiveCommand.Create<Transition>(RemoveTransition);
        RemoveCommand = ReactiveCommand.Create(Remove);
    }

    private readonly IStateEditor _stateEditor;
    private readonly IProjectManager _projectManager;
    private readonly IEditorManager _editorManager;

    public ICommand SaveCommand { get; }
    public ICommand RemoveCommand { get; }

    public ReactiveCommand<State, Unit> AddTransitionCommand { get; }
    public ReactiveCommand<Transition, Unit> RemoveTransitionCommand { get; }

    [Reactive] private State? State { get; set; }

    [Reactive] public string Name { get; set; }
    [Reactive] public string Description { get; set; }
    [Reactive] public int TypeIndex { get; set; }

    [Reactive] public string Width { get; set; }
    [Reactive] public string Height { get; set; }

    [Reactive]
    public List<State> AllStates { get; set; }

    [Reactive]
    public List<DrawArrows> Transitions { get; set; }
    
    [Reactive]
    public int StartSelectedPosName { get; set; }
    
    [Reactive]
    public int EndSelectedPosName { get; set; }
    
    [Reactive]
    public int StartSelectedPosDescription { get; set; }
    
    [Reactive]
    public int EndSelectedPosDescription { get; set; }
    

    private void AddTransition(State state)
    {
        if (State == null) return;
        _projectManager.CreateTransition(State, state);
    }

    private void RemoveTransition(Transition transition)
    {
        _projectManager.RemoveTransition(transition.Guid);
    }

    private void Save()
    {
        if (State == null) return;
        var type = TypeIndex switch
        {
            2 => StateType.End,
            1 => StateType.Start,
            _ => StateType.Common
        };
        var tryWidthParse = double.TryParse(Width, out double width);
        var tryHeightParse = double.TryParse(Height, out double height);
        if (!tryWidthParse || !tryHeightParse) return;
        var state = _projectManager.GetState(State.Guid);
        if (state == null) return;
        var newState = state with
        {
            Name = Name, Description = Description, Type = type, Width = width, Height = height,
            CenterPoint = new Point(State.X, State.Y)
        };
        _stateEditor.Update(newState);
    }

    private void Remove()
    {
        if (State == null) return;
        _editorManager.DoSelectNull();
        _projectManager.RemoveState(State.Guid);
    }
}