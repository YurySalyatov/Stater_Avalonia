using System;
using System.Collections.Generic;
using Avalonia;

namespace Stater.Models;

public enum StateType
{
    Common,
    Start,
    End
}

public record State(
    Guid Guid,
    string Name,
    string Description,
    StateType Type,
    Point CenterPoint,
    double Width,
    double Height,
    bool IsAnalyze,
    List<Event> EntryEvents,
    List<Event> ExitEvents,
    List<bool> IsReachableList
)
{
    public double X => CenterPoint.X;
    public double Y => CenterPoint.Y;
    public double Left => X - Width / 2;
    public double Right => X + Width / 2;
    public double Top => Y + Height / 2;
    public double Bottom => Y - Height / 2;

    public bool IsReachable => IsReachableList[0];
    public string Color
    {
        get
        {
            if(!IsAnalyze) return "black";
            return IsReachable ? "green" : "red";
        }
    }

    public State() : this(
        Guid.NewGuid(),
        "State",
        "",
        StateType.Common,
        new Point(50, 25),
        100,
        50,
        false,
        new List<Event>(),
        new List<Event>(),
        [false]
    )
    {
    }
}