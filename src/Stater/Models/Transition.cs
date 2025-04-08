using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation;

namespace Stater.Models;

public enum TypeArrow { Manhattan, Pifagor}

public record Transition(
    Guid Guid,
    string Name,
    Guid? Start,
    Guid? End, 
    TypeArrow Type,
    List<Point> LinePoints,
    Condition? Condition,
    Event? Event,
    Point NamePoint
)
{
    public Transition() : this(
        Guid.NewGuid(),
        "Transition",
        null,
        null,
        TypeArrow.Pifagor,
        [],
        null,
        null,
        new Point(0, 0)
    )
    {
    }

    public Point? StartPoint => LinePoints?[0];
    
    public Point? EndPoint => LinePoints?[^1];

    public Point? EndArrowPoint => EndPoint;
    public Point? StartArrowPoint => LinePoints?[^2];
}