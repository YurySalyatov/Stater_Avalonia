using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using DynamicData;

namespace Stater.Models.Executor.impl;

public class Executor(IProjectManager projectManager) : IExecutor
{
    private readonly ReplaySubject<State?> _state = new();
    public IObservable<State?> State => _state;

    private StateMachine? _stateMachine;

    private readonly ReplaySubject<LogContainer> _logs = new();
    public IObservable<LogContainer> Logs => _logs;

    private readonly ReplaySubject<IDictionary<Guid, VariableValue>?> _variables = new();
    public IObservable<IDictionary<Guid, VariableValue>?> Variables => _variables;

    private Thread? ExecuteThread;
    private bool StopExecution;

    private void WriteLog(
        string message,
        int tab = 0,
        ExecuteLog.ExecuteLogStatusEnum executeLogStatus = ExecuteLog.ExecuteLogStatusEnum.Info)
    {
        LogContainer? logs = null;
        var s = _logs.Subscribe(x => logs = x);
        s.Dispose();
        logs ??= new LogContainer(new List<ExecuteLog>());
        logs.Logs.Add(new ExecuteLog(Text: message, Tab: tab, ExecuteLogStatus: executeLogStatus));
        _logs.OnNext(logs);
    }

    private State? GetCurrentState()
    {
        State? currentState = null;
        var subscribe = _state.Subscribe(s => currentState = s);
        subscribe.Dispose();
        Console.WriteLine(currentState);
        return currentState;
    }

    private IDictionary<Guid, VariableValue>? GetCurrentVariables()
    {
        IDictionary<Guid, VariableValue>? variables = null;
        var subscribe = _variables.Subscribe(s => variables = s);
        subscribe.Dispose();
        return variables;
    }

    private StateMachine? GetCurrentStateMachine()
    {
        StateMachine? currentStateMachine = null;
        var s = projectManager.StateMachine.Subscribe(stateMachine => currentStateMachine = stateMachine);
        s.Dispose();
        return currentStateMachine;
    }

    public void Start(int stepTime)
    {
        WriteLog("Starting executing.");
        if (_stateMachine == null)
        {
            Console.WriteLine("YES");
            var currentStateMachine = GetCurrentStateMachine();
            if (currentStateMachine == null)
            {
                WriteLog("Execution not started, State Machine not set", 0, ExecuteLog.ExecuteLogStatusEnum.Error);
                return;
            }

            _stateMachine = currentStateMachine;
            _state.OnNext(null);
            _variables.OnNext(null);
        }

        WriteLog("State Machine: " + _stateMachine.Name);

        var state = GetCurrentState();
        if (state == null)
        {
            state = _stateMachine.States.FirstOrDefault(s => s.Type == StateType.Start);
            if (state == null)
            {
                WriteLog("Execution not started, Start State not set", 0, ExecuteLog.ExecuteLogStatusEnum.Error);
                return;
            }

            _state.OnNext(state);
        }

        var variables = GetCurrentVariables();
        if (variables == null)
        {
            WriteLog("Initializing variables");

            variables = _stateMachine.Variables.ToDictionary(x => x.Guid, x =>
            {
                WriteLog($"{x.Name} init with {x.StartValue}");
                return x.StartValue;
            });
            _variables.OnNext(variables);
        }

        WriteLog("Start State: " + state.Name);

        ExecuteThread = new Thread(() =>
        {
            WriteLog("Execution started.");
            StopExecution = false;
            while (!StopExecution)
            {
                Step();
                Thread.Sleep(stepTime);
            }

            WriteLog("Execution stopped.");
        });
        ExecuteThread.Start();
    }

    public void Stop()
    {
        if (StopExecution) return;
        WriteLog("Stopping execution");
        StopExecution = true;
    }

    public void Reset()
    {
        StopExecution = true;
        WriteLog("Reset execution");
        _state.OnNext(null);
        _stateMachine = null;
    }

    public void Step()
    {
        var state = GetCurrentState();
        if (state == null)
        {
            WriteLog("Step is incorrect: State is not set", 0, ExecuteLog.ExecuteLogStatusEnum.Error);
            Stop();
            return;
        }

        if (state.Type == StateType.End)
        {
            WriteLog("State is end", 0, ExecuteLog.ExecuteLogStatusEnum.Info);
            Stop();
            return;
        }

        if (_stateMachine == null)
        {
            WriteLog("Step is incorrect: StateMachine is not set", 0, ExecuteLog.ExecuteLogStatusEnum.Error);
            Stop();
            return;
        }

        var variables = GetCurrentVariables();
        if (variables == null)
        {
            WriteLog("Step is incorrect: Variables is not set", 0, ExecuteLog.ExecuteLogStatusEnum.Error);
            Stop();
            return;
        }

        WriteLog("Processing state: " + state.Name + ". State Machine: " + _stateMachine.Name);

        var transitions = _stateMachine.Transitions.Where(t => t.Start == state.Guid).ToList();
        WriteLog($"Found {transitions.Count} transitions", 1);

        foreach (var transition in transitions)
        {
            if (transition.Condition == null)
            {
                WriteLog($"Transition {transition.Name} has not condition", 1);
                continue;
            }

            bool res;
            try
            {
                res = ConditionLogic.Evaluate(transition.Condition, variables);
            }
            catch (VariableOperatorException e)
            {
                Console.WriteLine(e);
                throw;
            }

            if (res)
            {
                WriteLog($"Transition {transition.Name} condition is true", 1);
                if (transition.Event != null)
                {
                    var newVariables = EventLogic.Evaluate(transition.Event, GetCurrentVariables());
                    foreach (var keyValuePair in newVariables)
                    {
                        Console.WriteLine(keyValuePair.Key + " " + keyValuePair.Value);
                        variables[keyValuePair.Key] = keyValuePair.Value;
                    }
                }

                var newState = _stateMachine.States.First(s => s.Guid == transition.End);
                _state.OnNext(newState);
                return;
            }

            WriteLog($"Transition {transition.Name} condition is false", 1);
        }

        WriteLog("No transition was suitable", 0, ExecuteLog.ExecuteLogStatusEnum.Warning);
        Stop();
    }

    public void ClearLog()
    {
        _logs.OnNext(new LogContainer(new List<ExecuteLog>()));
    }
}