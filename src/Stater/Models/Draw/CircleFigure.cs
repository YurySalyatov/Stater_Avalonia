using Avalonia;

namespace Stater.Models.Draw;

public class CircleFigure(Point centerPoint, double radius, string color): Figure
{
    public Point CenterPoint { get; set; } = centerPoint;
    public double Width { get; set; } = 2 * radius;
    public double Height { get; set; } = 2 * radius;
    public string Color { get; set; } = color;
    
    public double Left => CenterPoint.X - Width / 2;
    
    public double Top => CenterPoint.Y - Height / 2;
}