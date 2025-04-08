using Avalonia.Controls;
using Splat;
using Stater.Models;
using Stater.Models.Editors;
using Stater.ViewModels.Editors;

namespace Stater.Views.Editors;

public partial class TransitionEditor : UserControl
{
    public TransitionEditor()
    {
        InitializeComponent();
        var transitionEditor = Locator.Current.GetService<ITransitionEditor>();
        var projectManager = Locator.Current.GetService<IProjectManager>();
        DataContext = new TransitionEditorViewModel(
            transitionEditor!,
            projectManager!
        );
    }
}