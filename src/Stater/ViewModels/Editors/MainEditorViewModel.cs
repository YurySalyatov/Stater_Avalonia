using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Stater.Models.Editors;

namespace Stater.ViewModels.Editors;

public class MainEditorViewModel : ReactiveObject
{
    public MainEditorViewModel(IEditorManager editorManager)
    {
        editorManager
            .EditorType
            .Subscribe(x => EditorType = x);
    }

    [Reactive] public EditorTypeEnum EditorType { get; private set; }
}