using Avalonia.Controls;
using Splat;
using Stater.Models.Executor;
using Stater.ViewModels.Execution;

namespace Stater.Views.Execution;

public partial class ExecutionControl : UserControl
{
    public ExecutionControl()
    {
        InitializeComponent();
        var executor = Locator.Current.GetService<IExecutor>();
        DataContext = new ExecutionControlViewModel(executor!);
    }
}