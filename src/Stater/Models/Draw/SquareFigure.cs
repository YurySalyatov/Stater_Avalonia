using Avalonia;

namespace Stater.Models.Draw;

public class SquareFigure(Point centerPoint, double width, string color) : Figure
{
    public Point CenterPoint { get; set; } = centerPoint;
    public double Width { get; set; } = width;
    public double Height { get; set; } = width;
    public string Color { get; set; } = color;

    public double Left => CenterPoint.X - Width / 2;

    public double Top => CenterPoint.Y - Height / 2;
}