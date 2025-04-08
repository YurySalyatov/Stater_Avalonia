using Avalonia;

namespace Stater.Models.Draw;

public interface Figure
{
    public Point CenterPoint { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string Color { get; set; }

    public double Left => CenterPoint.X - Width / 2;
    public double Top => CenterPoint.Y - Height / 2;
}