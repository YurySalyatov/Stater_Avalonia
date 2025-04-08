using ReactiveUI;

namespace Stater.Utils;

public static class PointToPoint
{
    public static Avalonia.Point ToAvaloniaPoint(Microsoft.Msagl.Core.Geometry.Point p) => new(p.X, p.Y);
}