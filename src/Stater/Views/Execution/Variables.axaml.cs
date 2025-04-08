using Avalonia.Controls;
using Splat;
using Stater.Models;
using Stater.Models.Editors;
using Stater.ViewModels.Execution;

namespace Stater.Views.Execution;

public partial class Variables : UserControl
{
    public Variables()
    {
        InitializeComponent();
        var projectManager = Locator.Current.GetService<IProjectManager>();
        var editorManager = Locator.Current.GetService<IEditorManager>();
        DataContext = new VariablesViewModel(projectManager!, editorManager!);
    }
}