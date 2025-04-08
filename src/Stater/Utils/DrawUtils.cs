using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Stater.Models;
using MathNet.Numerics.Interpolation;

namespace Stater.Utils;

public static class DrawUtils
{
    private static double PifagoreDistatnce(Point p1, Point p2)
    {
        return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
    }

    private static List<Point> GeneratePoints(State s)
    {
        var left = new Point(s.Left, s.Y);
        var right = new Point(s.Right, s.Y);
        var top = new Point(s.X, s.Top);
        var bottom = new Point(s.X, s.Bottom);
        return [left, right, top, bottom];
    }

    private static List<Point> GeneratePifagorPath(State start, State end)
    {
        if (start.Guid == end.Guid)
        {
            return
            [
                new Point(start.CenterPoint.X - 30, start.Top), new Point(start.CenterPoint.X, start.Top + 30),
                new Point(start.CenterPoint.X + 30, start.Top)
            ];
        }

        var startPoints = GeneratePoints(start);
        var endPoints = GeneratePoints(end);
        var startPoint = startPoints[0];
        var endPoint = endPoints[0];
        var dist = PifagoreDistatnce(startPoint, endPoint);
        foreach (var p1 in startPoints)
        {
            foreach (var p2 in endPoints)
            {
                if (dist <= PifagoreDistatnce(p1, p2)) continue;
                dist = PifagoreDistatnce(p1, p2);
                startPoint = p1;
                endPoint = p2;
            }
        }

        var centerPoint = (endPoint + startPoint) / 2;
        return [startPoint, centerPoint, endPoint];
    }

    private static List<Point> GenerateManhattanPath(State start, State end)
    {
        var startPoints = GeneratePoints(start);
        var endPoints = GeneratePoints(end);
        var startPoint = startPoints[0];
        var endPoint = endPoints[0];
        var dist = PifagoreDistatnce(startPoint, endPoint);
        foreach (var p1 in startPoints)
        {
            foreach (var p2 in endPoints)
            {
                if (dist <= PifagoreDistatnce(p1, p2)) continue;
                dist = PifagoreDistatnce(p1, p2);
                startPoint = p1;
                endPoint = p2;
            }
        }

        var startP = new Point(startPoint.X, startPoint.Y);

        return [startPoint, endPoint];
    }

    public static Point GetTransitionNamePoint(List<Point> p)
    {
        if (p.Count % 2 != 0) return p[p.Count / 2];
        var right = p[p.Count / 2];
        var left = p[p.Count / 2 - 1];
        return new Point((right.X + left.X) / 2, (right.Y + left.Y) / 2);
    }

    public static Point GetNearestPoint(double x, double y, Point a1, Point a2)
    {
        var k = (a2.Y - a1.Y) / (a2.X - a1.X);
        var b = (a1.Y * a2.X - a1.X * a2.Y) /
                (a2.X - a1.X);
        var xPoint = new Point(x, k * x + b);
        var yPoint = new Point((y - b) / k, y);
        return PifagoreDistatnce(xPoint, a1) > PifagoreDistatnce(yPoint, a1) ? yPoint : xPoint;
    }

    public static DrawArrows? GetTransition(State? s, State? e, Transition t, bool isAnalyze)
    {
        if (s == null || e == null) return null;
        return GetAssociateTransition(s, e, t, isAnalyze);
    }

    public static List<Point> GeneratePath(State s, State e, TypeArrow t)
    {
        return t switch
        {
            TypeArrow.Pifagor => GeneratePifagorPath(s, e),
            TypeArrow.Manhattan => GenerateManhattanPath(s, e),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static AssociateTransition GetAssociateTransition(State s, State e, Transition t, bool isAnalyze)
    {
        var drawLinePoints = GenerateCubicPoints(t.LinePoints);
        return new AssociateTransition(
            Start: s,
            End: e,
            Transition: t,
            ArrowPoints: GetArrowPoints(drawLinePoints[^2], drawLinePoints[^1]),
            DrawLinePoints: drawLinePoints,
            IsAnalyze: isAnalyze
        );
    }

    private static List<Point> GenerateCubicPoints(List<Point> points)
    {
        var x = points.Select(p => p.X).ToArray();
        var y = points.Select(p => p.Y).ToArray();
        var t = Enumerable.Range(0, points.Count).Select(i => (double)i).ToArray();

        var xSpline = MathNet.Numerics.Interpolate.Polynomial(t, x);
        var ySpline = MathNet.Numerics.Interpolate.Polynomial(t, y);

        int n = 100;
        var newT = Enumerable.Range(0, n).Select(i => t.Min() + (t.Max() - t.Min()) * i / (n - 1)).ToList();
        var newX = newT.Select(ti => xSpline.Interpolate(ti)).ToList();
        var newY = newT.Select(ti => ySpline.Interpolate(ti)).ToList();
        return newX.Zip(newY, (xp, yp) => new Point(xp, yp)).ToList();
    }

    private const double Eps = 1e-9;

    private static double NotZeroDivisor(double b)
    {
        var bAbs = Math.Abs(b);
        var bSign = Math.Sign(b);
        return bSign * (bAbs + Eps);
    }

    private const double LengthArrow = 20;
    private const double AngleArrow = Math.PI / 6;

    public static List<Point> GetArrowPoints(Point p1, Point p2)
    {
        var angle = Math.Atan2(p2.Y - p1.Y, NotZeroDivisor(p2.X - p1.X));
        var leftAngle = angle + AngleArrow;
        var rightAngle = angle - AngleArrow;

        var leftP = new Point(p2.X - LengthArrow * Math.Cos(leftAngle), p2.Y - LengthArrow * Math.Sin(leftAngle));
        var rightP = new Point(p2.X - LengthArrow * Math.Cos(rightAngle), p2.Y - LengthArrow * Math.Sin(rightAngle));
        return [leftP, rightP];
    }
}