using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive;
using Avalonia;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Stater.Models;
using Stater.Models.Draw;
using Stater.Models.Editors;
using Stater.Utils;

namespace Stater.ViewModels.Board;

public class BoardCanvasViewModel : ReactiveObject
{
    public BoardCanvasViewModel(IProjectManager projectManager, IEditorManager editorManager)
    {
        _projectManager = projectManager;
        _editorManager = editorManager;

        Height = 380;
        Width = 800;

        projectManager
            .StateMachine
            .Subscribe(x =>
            {
                StateMachine = IsAnalyze ? AnalyzeGraph.Analyze(x)! : x;
                if (IsAnalyze)
                {
                    AllEndStates = StateMachine.States.Count(state => state.Type == StateType.End);
                    ReachableEndStates = StateMachine.States.Count(state => state is { IsReachable: true, Type: StateType.End });
                    UnReachableEndStates = AllEndStates - ReachableEndStates;
                }
                StartPointsFigure = StateMachine.States
                    .Where(y => y.Type == StateType.Start)
                    .Select(y => new CircleFigure(
                        new Point(y.Left + FigureRadius * 1.2, y.Bottom + FigureRadius * 1.2), FigureRadius, "black"))
                    .ToList();
                EndPointsFigure = StateMachine.States
                    .Where(y => y.Type == StateType.End)
                    .Select(y => new SquareFigure(
                        new Point(y.Left + FigureRadius * 1.2, y.Bottom + FigureRadius * 1.2), 2 * FigureRadius,
                        "black"))
                    .ToList();
                Transitions = StateMachine.Transitions.Select(y =>
                        {
                            var startState = StateMachine.States.Find(s => s.Guid == y.Start)!;
                            var endState = StateMachine.States.Find(s => s.Guid == y.End)!;
                            return DrawUtils.GetTransition(startState, endState, y, IsAnalyze);
                        }
                    ).Where(y => y != null)
                    .OfType<DrawArrows>()
                    .ToList();
                if (Transition != null)
                {
                    LinePointsFigure = StateMachine.Transitions.Find(t => t.Guid == Transition.Guid)
                        ?.LinePoints
                        .Select(p => new CircleFigure(p, PointCircleRadius, "green")).ToList() ?? [];
                }
            });
        _projectManager
            .IsAnalyze
            .Subscribe(x => IsAnalyze = x);

        StateClickCommand = ReactiveCommand.Create<State>(OnStateClicked);
        UpdateStateCoordsCommand = ReactiveCommand.Create<Vector2>(UpdateStateCoords);
        UpdatePointCoordsCommand = ReactiveCommand.Create<Vector2>(UpdatePointCoords);
        TransitionClickCommand = ReactiveCommand.Create<Transition>(OnTransitionClicked);
        PointClickCommand = ReactiveCommand.Create<CircleFigure>(OnPointClicked);
        ClearAllViewCommand = ReactiveCommand.Create(ClearAllView);
    }

    private readonly IProjectManager _projectManager;
    private readonly IEditorManager _editorManager;

    [Reactive] public List<DrawArrows> Transitions { get; set; }

    [Reactive] public bool IsAnalyze { get; set; }
    [Reactive] public int Width { get; set; }

    [Reactive] public int Height { get; set; }
    private double FigureRadius { get; set; } = 10;
    private double PointCircleRadius { get; set; } = 3.0;
    [Reactive] public List<CircleFigure> StartPointsFigure { get; set; }
    [Reactive] public List<CircleFigure> LinePointsFigure { get; set; }
    [Reactive] public List<SquareFigure> EndPointsFigure { get; set; }

    [Reactive] public int ReachableEndStates { get; set; } = 0;
    [Reactive] public int UnReachableEndStates { get; set; } = 0;
    [Reactive] public int AllEndStates { get; set; } = 0;

    [Reactive] public Transition? Transition { get; set; }
    [Reactive] public State? State { get; private set; }
    [Reactive] public CircleFigure? LinePoint { get; private set; }
    [Reactive] public StateMachine StateMachine { get; private set; }

    public ReactiveCommand<State, Unit> StateClickCommand { get; }
    public ReactiveCommand<CircleFigure, Unit> PointClickCommand { get; }
    public ReactiveCommand<Vector2, Unit> UpdateStateCoordsCommand { get; }

    public ReactiveCommand<Vector2, Unit> UpdatePointCoordsCommand { get; }

    public ReactiveCommand<Transition, Unit> TransitionClickCommand { get; }

    public ReactiveCommand<Unit, Unit> ClearAllViewCommand { get; }

    private void OnStateClicked(State state)
    {
        ClearLinePoints();
        Transition = null;
        var selectedState = _projectManager.GetState(state.Guid);
        State = selectedState ?? null;
        if (State != null) _editorManager.DoSelectState(State);
    }

    private void OnPointClicked(CircleFigure figure)
    {
        LinePoint = figure;
    }

    private void OnTransitionClicked(Transition transition)
    {
        State = null;
        var selectedTransition = _projectManager.GetTransition(transition.Guid);
        Transition = selectedTransition ?? null;
        if (selectedTransition == null) return;
        _editorManager.DoSelectTransition(selectedTransition);
        LinePointsFigure = selectedTransition.LinePoints.Select(x => new CircleFigure(x, PointCircleRadius, "green"))
            .ToList();
    }

    private void UpdateStateCoords(Vector2 coords)
    {
        if (State == null) return;
        var newState = State with
        {
            CenterPoint = State.CenterPoint + coords,
        };
        var selfTransitions = StateMachine.Transitions
            .FindAll(t => t.Start == newState.Guid && t.End == newState.Guid);
        var newSelft = UpdateTransition(coords, selfTransitions, true, true);
        var outgoingTransitions =
            StateMachine.Transitions.FindAll(t => t.Start == newState.Guid && t.End != newState.Guid);
        var newOutgoing = UpdateTransition(coords, outgoingTransitions, first: true);
        var incomingTransitions =
            StateMachine.Transitions.FindAll(t => t.End == newState.Guid && t.Start != newState.Guid);
        var newIncoming = UpdateTransition(coords, incomingTransitions, last: true);
        _editorManager.DoSelectState(newState);
        var unchangedStates = StateMachine.States.FindAll(s => s.Guid != newState.Guid);
        var unchangedTransitions =
            StateMachine.Transitions.FindAll(t => t.Start != newState.Guid && t.End != newState.Guid);
        unchangedTransitions.AddRange(newSelft);
        unchangedTransitions.AddRange(newOutgoing);
        unchangedTransitions.AddRange(newIncoming);
        var newStateMachine = StateMachine with
        {
            States = [..unchangedStates, newState],
            Transitions = unchangedTransitions
        };
        _projectManager.UpdateStateMachine(newStateMachine);
    }

    private void UpdatePointCoords(Vector2 coords)
    {
        if (LinePoint == null || Transition == null) return;
        State? st = null;
        var changeNamePoint = Transition.NamePoint == LinePoint.CenterPoint ? true : false;
        if (LinePoint.CenterPoint == LinePointsFigure[0].CenterPoint)
        {
            st = StateMachine.States.Find(s => s.Guid == Transition.Start);
            if (st == null) return;
        }

        if (LinePoint.CenterPoint == LinePointsFigure[^1].CenterPoint)
        {
            st = StateMachine.States.Find(s => s.Guid == Transition.End);
            if (st == null) return;
        }

        LinePoint.CenterPoint += coords;
        if (st != null)
        {
            var x = LinePoint.CenterPoint.X > st.CenterPoint.X ? st.Right : st.Left;
            var y = LinePoint.CenterPoint.Y > st.CenterPoint.Y ? st.Top : st.Bottom;
            LinePoint.CenterPoint = DrawUtils.GetNearestPoint(x, y, st.CenterPoint, LinePoint.CenterPoint);
        }

        var newTransition = Transition with
        {
            LinePoints = LinePointsFigure.Select(x => x.CenterPoint).ToList(),
            NamePoint = changeNamePoint ? LinePoint.CenterPoint : Transition.NamePoint,
        };
        Transition = newTransition;
        _projectManager.UpdateTransition(newTransition);
    }

    private List<Transition> UpdateTransition(Vector2 coords, List<Transition> transitions, bool first = false,
        bool last = false)
    {
        var newTransitions = new List<Transition>();
        foreach (var transition in transitions)
        {
            var newLinePoints = transition.LinePoints.ToList();
            if (first)
            {
                newLinePoints[0] += coords;
            }

            if (last)
            {
                newLinePoints[^1] += coords;
            }

            var newTransition = transition with
            {
                LinePoints = newLinePoints
            };
            newTransitions.Add(newTransition);
        }

        return newTransitions;
    }

    private void UpdateTransitionEnd(Vector2 coords, List<Transition> transitions)
    {
        foreach (var transition in transitions)
        {
            transition.LinePoints[^1] += coords;
        }
    }

    private void ClearLinePoints()
    {
        LinePointsFigure = [];
    }

    private void ClearAllView()
    {
        ClearLinePoints();
        State = null;
        Transition = null;
        LinePoint = null;
        _editorManager.DoSelectNull();
    }
}