using System;
using System.Collections.Generic;
using System.Linq;
using Stater.Models;

namespace Stater.Utils;

public static class AnalyzeGraph
{
    public static StateMachine? Analyze(StateMachine? stateMachine)
    {
        if (stateMachine == null) return null;
        List<State> states = stateMachine.States
            .Select(y => y with { IsReachableList = [false], IsAnalyze = true })
            .ToList();
        var start = states.Find(x => x.Type == StateType.Start);
        if (start == null) return stateMachine with { States = states };
        Dfs(start, stateMachine, states);
        return stateMachine with { States = states };
    }

    private static void Dfs(State curState, StateMachine stateMachine, List<State> states)
    {
        curState.IsReachableList[0] = true;
        var outputState = stateMachine.Transitions
            .FindAll(t => t.Start == curState.Guid)
            .Select(t => t.End)
            .Where(t => t != null)
            .OfType<Guid>()
            .Select(g => states.Find(s => s.Guid == g))
            .Where(s => s != null)
            .OfType<State>()
            .Where(s => !s.IsReachable);
        foreach (var state in outputState)
        {
            Dfs(state, stateMachine, states);
        }
    }
}