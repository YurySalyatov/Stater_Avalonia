using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using SLXParser.Data;
using Stater.Models;
using State = Stater.Models.State;
using SLXState = SLXParser.Data.State;
using Transition = Stater.Models.Transition;
using SLXTransition = SLXParser.Data.Transition;
using SLXData = SLXParser.Data.Data;

namespace SLXParser;

public class Translator
{
    public StateMachine Convert(Stateflow stateflow)
    {
        var stateMachine = new StateMachine
        {
            // Type = "SLX",
            Name = stateflow.Instance.Name,
            States = ConvertChart(stateflow.Machine.Chart),
            Variables = ConvertVariable(stateflow.Machine.Chart.ChildrenData),
        };

        stateMachine.Transitions.AddRange(ConvertChartTransitions(stateflow.Machine.Chart, stateMachine.States));

        stateMachine.States[0] = stateMachine.States[0] with { Type = StateType.Start };
        stateMachine.States[^1] = stateMachine.States[^1] with { Type = StateType.End };

        return stateMachine;
    }

    private static List<State> ConvertChart(Chart chart)
    {
        var states = new List<State>();

        foreach (var state in chart.ChildrenState)
        {
            states.AddRange(ConvertState(state));
        }

        return states;
    }

    private static IEnumerable<State> ConvertState(SLXState slxState)
    {
        var state = new State
        {
            Guid = slxState.Id,
            Name = slxState.LabelString,
            Type = StateType.Common,
            CenterPoint = new Point(slxState.Position.X.X, slxState.Position.X.Y)
        };

        var states = new List<State> { state };

        foreach (var slxStateChildren in slxState.ChildrenState)
        {
            states.AddRange(ConvertState(slxStateChildren));
        }

        return states;
    }

    private List<Transition> ConvertChartTransitions(Chart chart, List<State> slxStateOriginList)
    {
        var transitions = new List<Transition>();

        foreach (var state in chart.ChildrenState)
        {
            transitions.AddRange(ConvertStateTransition(state, slxStateOriginList));
        }

        return transitions;
    }

    private static IEnumerable<Transition> ConvertStateTransition(SLXState slxState, List<State> slxStateOriginList)
    {
        var transitions = new List<Transition>();

        foreach (var transition in slxState.ChildrenTransition)
        {
            var transition_ = ConvertTransition(transition, slxStateOriginList);
            if (transition_ != null)
            {
                transitions.Add(transition_);
            }
        }

        foreach (var state in slxState.ChildrenState)
        {
            transitions.AddRange(ConvertStateTransition(state, slxStateOriginList));
        }

        return transitions;
    }

    private static Transition ConvertTransition(SLXTransition slxTransition, List<State> slxStateOriginList)
    {
        var transition = new Transition
        {
            Guid = slxTransition.Id
        };

        var start = FindStateById(slxTransition.Src.SSID, slxStateOriginList);
        var end = FindStateById(slxTransition.Dst.SSID, slxStateOriginList);

        if (start == null || end == null)
        {
            return null;
        }

        transition = transition with
        {
            Start = start.Guid,
            End = end.Guid,
            Name = slxTransition.LabelString
        };

        // start.Outgoing.Add(transition);
        // end.Incoming.Add(transition);

        return transition;
    }

    private static State FindStateById(Guid id, List<State> slxStateOriginList)
    {
        return slxStateOriginList.FirstOrDefault(state => state.Guid == id);
    }

    private static List<Variable> ConvertVariable(List<SLXData> datas)
    {
        var result = new List<Variable>();
        foreach (var data in datas)
        {
            // var variableValue = VariableValueBuilder.fromString("");

            var variable = new Variable
            {
                Name = data.Name
            };

            // variable.ID = new UID(data.Id);
            result.Add(variable);
        }

        return result;
    }
}