using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Microsoft.Msagl.Core;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.Incremental;
using Microsoft.Msagl.Layout.LargeGraphLayout;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Layout.MDS;
using Microsoft.Msagl.Miscellaneous;
using Microsoft.Msagl.Prototype.Ranking;
using Microsoft.Msagl.Routing;
using Stater.Models;
using Stater.Views.Editors;
using Edge = Microsoft.Msagl.Core.Layout.Edge;
using Label = Microsoft.Msagl.Core.Layout.Label;
using LineSegment = Microsoft.Msagl.Core.Geometry.Curves.LineSegment;
using Node = Microsoft.Msagl.Core.Layout.Node;
using MPoint = Microsoft.Msagl.Core.Geometry.Point;
using APoint = Avalonia.Point;

namespace Stater.Utils;

public class GoodGraphView
{
    public StateMachine? ReBuildGraph(StateMachine? stateMachine)
    {
        if (stateMachine == null) return stateMachine;
        var graph = new GeometryGraph();
        foreach (var state in stateMachine.States)
        {
            var nd = new Node(
                CurveFactory.CreateRectangle(
                    state.Width,
                    state.Height,
                    new MPoint(0, 0)),
                state.Guid);
            graph.Nodes.Add(nd);
        }

        foreach (var transition in stateMachine.Transitions)
        {
            var nd = new Node(
                CurveFactory.CreateRectangle(
                    10 * transition.Name.Length,
                    20,
                    new MPoint(0, 0)),
                transition.Guid);
            graph.Nodes.Add(nd);
            var source = graph.FindNodeByUserData(transition.Start);
            var target = graph.FindNodeByUserData(transition.End);
            if(source == null || target == null) continue;
            graph.Edges.Add(
                new Edge(
                    graph.FindNodeByUserData(transition.Start),
                    nd
                )
                {
                    Weight = 1,
                    UserData = new KeyValuePair<Guid, bool>(transition.Guid, true)
                });
            graph.Edges.Add(
                new Edge(
                    nd,
                    graph.FindNodeByUserData(transition.End))
                {
                    Weight = 1,
                    UserData = new KeyValuePair<Guid, bool>(transition.Guid, false)
                });
        }

        var mode = EdgeRoutingMode.RectilinearToCenter;
        var bundSet = new BundlingSettings()
        {
            EdgeSeparation = 2000,
        };
        var routSet = new EdgeRoutingSettings
        {
            BundlingSettings = bundSet,
            EdgeRoutingMode = mode,
            Padding = 2000,
            EdgeSeparationRectilinear = 2000
        };
        var laySet = new MdsLayoutSettings() { EdgeRoutingSettings = routSet, NodeSeparation = 50.0 };
        try
        {
            LayoutHelpers.CalculateLayout(graph, laySet, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return stateMachine;
        }

        graph.UpdateBoundingBox();
        graph.Translate(new MPoint(-graph.Left, -graph.Bottom));

        List<State> newStates = [];
        List<Transition> newTransitions = [];
        var namePoints = new Dictionary<Guid, APoint>();
        var firstHalf = new Dictionary<Guid, List<APoint>>();
        var secondHalf = new Dictionary<Guid, List<APoint>>();
        foreach (var node in graph.Nodes)
        {
            if (node.UserData is not Guid guid) continue;
            var prevState = stateMachine.States.Find(el => el.Guid == guid);
            if (prevState != null)
            {
                var newState = prevState with
                {
                    CenterPoint = PointToPoint.ToAvaloniaPoint(node.BoundingBox.Center)
                };
                newStates.Add(newState);
            }
            else
            {
                namePoints[guid] = PointToPoint.ToAvaloniaPoint(node.BoundingBox.Center);
            }
        }

        foreach (var edge in graph.Edges)
        {
            var newLinePoints = new List<APoint>();
            DownloadPoints(newLinePoints, edge);
            if (edge.UserData is not KeyValuePair<Guid, bool> p) continue;
            if (p.Value)
            {
                firstHalf[p.Key] = newLinePoints;
            }
            else
            {
                secondHalf[p.Key] = newLinePoints;
            }
        }

        newTransitions.AddRange(from kv in namePoints
            let transition = stateMachine.Transitions.Find(el => el.Guid == kv.Key)!
            let linePoints = MergePoints(firstHalf[kv.Key], kv.Value, secondHalf[kv.Key])
            select transition with { LinePoints = linePoints, NamePoint = kv.Value });
        var newStateMachine = stateMachine with
        {
            States = newStates,
            Transitions = newTransitions,
        };
        return newStateMachine;
    }

    private List<APoint> MergePoints(List<APoint> firstHalf, APoint namePoint, List<APoint> secondHalf)
    {
        List<APoint> points =
        [
            firstHalf[0],
            namePoint,
            secondHalf[^1],
        ];

        return points;
    }

    private static void DownloadPoints(List<APoint> linePoints, Edge edge)
    {
        switch (edge.Curve)
        {
            case LineSegment lineSegment:
                linePoints.Add(PointToPoint.ToAvaloniaPoint(lineSegment.Start));
                linePoints.Add(PointToPoint.ToAvaloniaPoint(lineSegment.End));
                break;
            case Curve curve:
            {
                foreach (var segment in curve.Segments)
                {
                    switch (segment)
                    {
                        // When curve contains a line segment
                        case LineSegment line:
                        {
                            if (linePoints.Count == 0)
                                linePoints.Add(PointToPoint.ToAvaloniaPoint(line.Start));
                            linePoints.Add(PointToPoint.ToAvaloniaPoint(line.End));
                            break;
                        }
                        // When curve contains a cubic bezier segment
                        case CubicBezierSegment bezier:
                            linePoints.Add(
                                PointToPoint.ToAvaloniaPoint(new MPoint(bezier.B(0).X, bezier.B(0).Y)));
                            linePoints.Add(
                                PointToPoint.ToAvaloniaPoint(new MPoint(bezier.B(1).X, bezier.B(1).Y)));
                            linePoints.Add(
                                PointToPoint.ToAvaloniaPoint(new MPoint(bezier.B(2).X, bezier.B(2).Y)));
                            linePoints.Add(
                                PointToPoint.ToAvaloniaPoint(new MPoint(bezier.B(3).X, bezier.B(3).Y)));
                            break;
                        // When curve contains an arc
                        case Ellipse ellipse:
                        {
                            var interval = (ellipse.ParEnd - ellipse.ParStart) / 5.0;
                            for (var i = ellipse.ParStart;
                                 i < ellipse.ParEnd;
                                 i += interval)
                            {
                                var p = ellipse.Center
                                        + (Math.Cos(i) * ellipse.AxisA)
                                        + (Math.Sin(i) * ellipse.AxisB);
                                linePoints.Add(new APoint(p.X, p.Y));
                            }

                            break;
                        }
                        default:
                            break;
                    }
                }

                break;
            }
        }
    }
}