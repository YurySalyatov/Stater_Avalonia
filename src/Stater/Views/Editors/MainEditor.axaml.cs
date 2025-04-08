using Avalonia.Controls;
using Splat;
using Stater.Models.Editors;
using Stater.ViewModels.Editors;

namespace Stater.Views.Editors;

public partial class MainEditor : UserControl
{
    public MainEditor()
    {
        InitializeComponent();
        var editorManager = Locator.Current.GetService<IEditorManager>();
        DataContext = new MainEditorViewModel(
            editorManager!
        );
    }
}