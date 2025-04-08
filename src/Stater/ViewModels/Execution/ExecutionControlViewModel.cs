using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Stater.Models.Executor;

namespace Stater.ViewModels.Execution;

public class ExecutionControlViewModel : ReactiveObject
{
    public ExecutionControlViewModel(IExecutor executor)
    {
        _executor = executor;

        StartCommand = ReactiveCommand.Create(Start);
        StopCommand = ReactiveCommand.Create(Stop);
        StepCommand = ReactiveCommand.Create(Step);
        ResetCommand = ReactiveCommand.Create(Reset);
        ClearCommand = ReactiveCommand.Create(Clear);

        _executor.Logs.Subscribe(x => { Logs = x.Logs.ToList(); });
    }

    private readonly IExecutor _executor;

    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand StepCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand ClearCommand { get; }

    [Reactive] public string StepTime { get; set; }
    [Reactive] public List<ExecuteLog> Logs { get; set; }

    private void Start() => _executor.Start(int.Parse(StepTime));
    private void Stop() => _executor.Stop();
    private void Step() => _executor.Step();
    private void Reset() => _executor.Reset();
    private void Clear() => _executor.ClearLog();
}