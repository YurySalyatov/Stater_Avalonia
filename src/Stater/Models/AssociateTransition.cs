using System.Collections.Generic;
using Avalonia;
using Stater.Utils;
namespace Stater.Models;

public record AssociateTransition(
    State Start,
    State End,
    Transition Transition,
    List<Point> ArrowPoints,
    List<Point> DrawLinePoints,
    bool IsAnalyze
) : DrawArrows(Start, End, Transition)
{
    public Point StartPoint => Transition.LinePoints[0];
    public Point EndPoint => Transition.LinePoints[^1];
    public Point LeftArrowPoint => ArrowPoints[0];
    public Point RightArrowPoint => ArrowPoints[1];
    public Point EndArrowPoint => EndPoint;
    
    public Point NamePoint => Transition.NamePoint;
    
    public double Left => NamePoint.X - Name.Length * 5;
    public double Top => NamePoint.Y + 10;
    public string Name => Transition.Name;
    public string Color
    {
        get
        {
            if (!IsAnalyze) return "black";
            return End.IsReachable ? "green" : "red";
        }
    }

    public List<Point>? LinePoints => Transition.LinePoints;
}