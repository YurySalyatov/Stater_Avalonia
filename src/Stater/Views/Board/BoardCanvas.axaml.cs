using System;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Splat;
using Stater.Models;
using Stater.Models.Draw;
using Stater.Models.Editors;
using Stater.Utils;
using Stater.ViewModels.Board;

namespace Stater.Views.Board;

public partial class BoardCanvas : UserControl
{
    private const double ScaleFactor = 1.01;
    private double scale = 1;
    private const double MaxScale = 3;
    private const double MinScale = 0.5;

    private const double TranslateXFactor = 1;
    private double x;
    private double _x;
    private double startX;

    private const double TranslateYFactor = 1;
    private double y;
    private double _y;
    private double startY;

    private bool pressed;

    private const double StateTranslateXFactor = 1;
    private double stateX;
    private double _stateX;

    private const double StateTranslateYFactor = 1;
    private double stateY;
    private double _stateY;
    private bool statePressed;

    private bool pointPressed;

    private const int GridLineRangeX = 40;
    private const int GridLineRangeY = 40;
    private readonly Color gridLineColor = Color.Parse("Gray");
    private const int GridLineSize = 1;

    public BoardCanvas()
    {
        InitializeComponent();
        var projectManager = Locator.Current.GetService<IProjectManager>();
        var editorManager = Locator.Current.GetService<IEditorManager>();
        DataContext = new BoardCanvasViewModel(projectManager!, editorManager!);
        drawGrid();
    }

    private void drawGrid()
    {
        var canvas = TheCanvas;

        var height = canvas.Height;
        var width = canvas.Width;

        for (var i = GridLineRangeX; i < width; i += GridLineRangeX)
        {
            Line line = new()
            {
                StartPoint = new Point(i, 0),
                EndPoint = new Point(i, height),
                Stroke = new SolidColorBrush
                {
                    Color = gridLineColor
                },
                StrokeThickness = GridLineSize,
                ZIndex=-1
            };
            canvas.Children.Add(line);
        }

        for (var i = GridLineRangeY; i < height; i += GridLineRangeY)
        {
            Line line = new()
            {
                StartPoint = new Point(0, i),
                EndPoint = new Point(width, i),
                Stroke = new SolidColorBrush
                {
                    Color = gridLineColor
                },
                StrokeThickness = GridLineSize,
                ZIndex=-1
            };
            canvas.Children.Add(line);
        }
    }

    private void UpdateCanvasMatrix()
    {
        // TODO: Check scale and x, y ranges
        
        TheCanvas.RenderTransform =
            new MatrixTransform(Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(x, y));
    }

    private void InputElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Delta.Y >= 0 && scale < MaxScale)
        {
            scale *= ScaleFactor;
            UpdateCanvasMatrix();
        }
        else if (scale > MinScale)
        {
            scale /= ScaleFactor;
            UpdateCanvasMatrix();
        }

    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (statePressed || pointPressed) return;
        pressed = true;
        var point = e.GetCurrentPoint(TheCanvasPanel);
        _x = point.Position.X;
        _y = point.Position.Y;
        startX = x;
        startY = y;
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!pressed || sender == null) return;
        var point = e.GetCurrentPoint(TheCanvasPanel);
        var positionX = point.Position.X;
        var positionY = point.Position.Y;
        x = startX - (_x - positionX) * TranslateXFactor;
        y = startY - (_y - positionY) * TranslateYFactor;
        UpdateCanvasMatrix();
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        pressed = false;
    }

    private void State_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var rectangle = (Control)sender;
        if (rectangle?.DataContext is not State state) return;
        var context = (BoardCanvasViewModel)DataContext;
        context?.StateClickCommand.Execute(state).Subscribe();
        statePressed = true;

        var canvas = TheCanvasPanel;
        var point = e.GetCurrentPoint(canvas);
        stateX = 0;
        stateY = 0;
        _stateX = point.Position.X;
        _stateY = point.Position.Y;
    }

    private void State_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!statePressed || sender == null) return;
        var rectangle = (Control)sender!;
        var canvas = TheCanvasPanel;
        var point = e.GetCurrentPoint(canvas);
        var positionX = point.Position.X;
        var positionY = point.Position.Y;
        stateX = (positionX - _stateX) * StateTranslateXFactor / scale;
        stateY = (positionY - _stateY) * StateTranslateYFactor / scale;
        rectangle.RenderTransform =
            new MatrixTransform(Matrix.CreateTranslation(stateX, stateY));
    }

    private void State_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        statePressed = false;
        var context = (BoardCanvasViewModel)DataContext;
        context?.UpdateStateCoordsCommand.Execute(new Vector2((float)stateX, (float)stateY)).Subscribe();
    }
    
    private void Point_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var rectangle = (Control)sender;
        if (rectangle?.DataContext is not CircleFigure figure) return;
        var context = (BoardCanvasViewModel)DataContext;
        context?.PointClickCommand.Execute(figure).Subscribe();
        pointPressed = true;

        var canvas = TheCanvasPanel;
        var point = e.GetCurrentPoint(canvas);
        stateX = 0;
        stateY = 0;
        _stateX = point.Position.X;
        _stateY = point.Position.Y;
    }
    
    private void Point_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!pointPressed || sender == null) return;
        var rectangle = (Control)sender!;
        var canvas = TheCanvasPanel;
        var point = e.GetCurrentPoint(canvas);
        var positionX = point.Position.X;
        var positionY = point.Position.Y;
        stateX = (positionX - _stateX) * StateTranslateXFactor / scale;
        stateY = (positionY - _stateY) * StateTranslateYFactor / scale;
        rectangle.RenderTransform =
            new MatrixTransform(Matrix.CreateTranslation(stateX, stateY));
    }
    
    private void Point_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        pointPressed = false;
        var context = (BoardCanvasViewModel)DataContext;
        context?.UpdatePointCoordsCommand.Execute(new Vector2((float)stateX, (float)stateY)).Subscribe();
    }

    private void Transition_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var rectangle = (Control)sender;
        if (rectangle?.DataContext is not DrawArrows transition) return;
        var context = (BoardCanvasViewModel)DataContext;
        context?.TransitionClickCommand.Execute(transition.Transition).Subscribe();
    }
}