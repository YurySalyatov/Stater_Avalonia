using Avalonia.Controls;
using Splat;
using Stater.Models;
using Stater.Models.Editors;
using Stater.ViewModels.FindLine;

namespace Stater.Views.FindLine;

public partial class FindLine : UserControl
{
    public FindLine()
    {
        InitializeComponent();
        var projectManager = Locator.Current.GetService<IProjectManager>();
        var editorManager = Locator.Current.GetService<IEditorManager>();
        DataContext = new FindLineViewModel(
            projectManager!,
            editorManager!
        );
}
}