using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Stater.Plugin;
using Stater.ViewModels;

namespace Stater.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OpenFileButton_Clicked(object sender, RoutedEventArgs args)
    {
        var topLevel = GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Project",
            AllowMultiple = false
        });

        if (files.Count < 1) return;
        await using var stream = await files[0].OpenReadAsync();
        using var streamReader = new StreamReader(stream);
        var context = (MainWindowViewModel)DataContext;
        context?.OpenCommand.Execute(streamReader).Subscribe();
    }

    private async void SaveFileButton_Clicked(object sender, RoutedEventArgs args)
    {
        var topLevel = GetTopLevel(this);
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Project"
        });
        if (file is null) return;
        await using var stream = await file.OpenWriteAsync();
        await using var streamWriter = new StreamWriter(stream);
        var context = (MainWindowViewModel)DataContext;
        context?.SaveCommand.Execute(streamWriter).Subscribe();
    }

    private async void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var pluginMenu = (MenuItem)sender;
        var plugin = (ButtonFilePlugin)pluginMenu.DataContext;
        var topLevel = GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });

        if (files.Count < 1) return;

        var context = (MainWindowViewModel)DataContext;
        context?.PluginButtinCommand.Execute(
            new PathPluginDto(
                Plugin: plugin,
                Path: files[0].Path.ToString().Replace("file:/", "")
            )
        ).Subscribe();
    }
}