using System;
using System.Globalization;
using System.IO;
using System.Xml;
using SLXParser.Data;
using SLXParser.Utils;

namespace SLXParser;

public class Parser
{
    private readonly string inFile;
    private readonly string zipPath;

    public Parser(string path)
    {
        if (!File.Exists(path))
        {
            throw new InvalidOperationException("Файл не найден");
        }

        inFile = path;
        var parentDir = Directory.GetParent(inFile);

        if (parentDir == null)
        {
            throw new InvalidOperationException("Не удалось выбрать папку для распаковки");
        }

        zipPath = Path.Combine(parentDir.ToString(), "slx_parser.temp");
    }

    public Stateflow Parse()
    {
        System.IO.Compression.ZipFile.ExtractToDirectory(inFile, zipPath);

        var simulinkPath = Path.Combine(zipPath, "simulink", "stateflow.xml");

        if (!File.Exists(simulinkPath))
        {
            Directory.Delete(zipPath, true);
            throw new InvalidOperationException("Не удалось найти файл от stateflow");
        }

        try
        {
            return ParseFile(simulinkPath);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            Directory.Delete(zipPath, true);
        }
    }

    private Stateflow ParseFile(string file)
    {
        var xDoc = new XmlDocument();
        xDoc.Load(file);
        var xRoot = xDoc.DocumentElement;
        if (xRoot == null)
        {
            throw new InvalidOperationException("Проблема парсинга stateflow xml");
        }

        if (xRoot.Name != "Stateflow")
        {
            throw new InvalidOperationException("Главный элемент не является Stateflow");
        }

        return ParseStateflow(xRoot);
    }

    private Stateflow ParseStateflow(XmlNode xmlNode)
    {
        var stateflow = new Stateflow();

        foreach (XmlNode node in xmlNode.ChildNodes)
        {
            if (node.Name == "machine")
            {
                stateflow.Machine = ParseMachine(node);
            }

            if (node.Name == "instance")
            {
                stateflow.Instance = ParseInstance(node);
            }
        }

        return stateflow;
    }

    private Instance ParseInstance(XmlNode xmlNode)
    {
        var instance = new Instance
        {
            Id = ParseId(xmlNode)
        };

        foreach (XmlNode node in xmlNode.ChildNodes)
        {
            if (node.Name == "P")
            {
                {
                    var name = node.Attributes?["Name"]?.Value;
                    switch (name)
                    {
                        case null:
                            continue;
                        case "name":
                            instance.Name = node.InnerText;
                            break;
                    }

                    break;
                }
            }
        }

        return instance;
    }

    private Machine ParseMachine(XmlNode xmlNode)
    {
        var machine = new Machine
        {
            Id = ParseId(xmlNode)
        };

        foreach (XmlNode node in xmlNode.ChildNodes)
        {
            switch (node.Name)
            {
                case "P":
                {
                    var name = node.Attributes?["Name"]?.Value;
                    switch (name)
                    {
                        case null:
                            continue;
                        case "created":
                            machine.Created = node.InnerText;
                            break;
                    }

                    break;
                }
                case "Children":
                    foreach (XmlNode childrenNode in node.ChildNodes)
                    {
                        if (childrenNode.Name == "chart")
                        {
                            machine.Chart = ParseChart(childrenNode);
                        }
                    }

                    break;
            }
        }

        return machine;
    }

    private Chart ParseChart(XmlNode xmlNode)
    {
        var chart = new Chart
        {
            Id = ParseId(xmlNode)
        };

        foreach (XmlNode node in xmlNode.ChildNodes)
        {
            switch (node.Name)
            {
                case "P":
                {
                    var name = node.Attributes?["Name"]?.Value;
                    switch (name)
                    {
                        case null:
                            continue;
                        case "name":
                            chart.Name = node.InnerText;
                            break;
                        case "windowPosition":
                            chart.WindowPosition = Parse4Point(node.InnerText);
                            break;
                        case "viewLimits":
                            chart.ViewLimits = Parse4Point(node.InnerText);
                            break;
                        case "zoomFactor":
                            chart.ZoomFactor = float.Parse(node.InnerText, CultureInfo.InvariantCulture);
                            break;
                        case "stateColor":
                            chart.StateColor = ParseColor(node.InnerText);
                            break;
                        case "stateLabelColor":
                            chart.StateLabelColor = ParseColor(node.InnerText);
                            break;
                        case "transitionColor":
                            chart.TransitionColor = ParseColor(node.InnerText);
                            break;
                        case "transitionLabelColor":
                            chart.TransitionLabelColor = ParseColor(node.InnerText);
                            break;
                        case "junctionColor":
                            chart.JunctionColor = ParseColor(node.InnerText);
                            break;
                        case "chartColor":
                            chart.ChartColor = ParseColor(node.InnerText);
                            break;
                        case "viewObj":
                            chart.ViewObj = int.Parse(node.InnerText);
                            break;
                        case "visible":
                            chart.Visible = ParseBool(node.InnerText);
                            break;
                    }

                    break;
                }
                case "Children":
                    foreach (XmlNode childrenNode in node.ChildNodes)
                    {
                        switch (childrenNode.Name)
                        {
                            case "state":
                                chart.ChildrenState.Add(ParseState(childrenNode));
                                break;
                            case "data":
                                chart.ChildrenData.Add(ParseData(childrenNode));
                                break;
                        }
                    }

                    break;
            }
        }

        return chart;
    }

    private State ParseState(XmlNode xmlNode)
    {
        var state = new State
        {
            Id = ParseId(xmlNode)
        };

        foreach (XmlNode node in xmlNode.ChildNodes)
        {
            switch (node.Name)
            {
                case "P":
                {
                    var name = node.Attributes?["Name"]?.Value;
                    switch (name)
                    {
                        case null:
                            continue;
                        case "labelString":
                            state.LabelString = node.InnerText;
                            break;
                        case "position":
                            state.Position = Parse4Point(node.InnerText);
                            break;
                        case "fontSize":
                            state.FontSize = int.Parse(node.InnerText);
                            break;
                        case "visible":
                            state.Visible = ParseBool(node.InnerText);
                            break;
                        case "subviewer":
                            state.Subviewer = int.Parse(node.InnerText);
                            break;
                        case "type":
                            state.Type = node.InnerText;
                            break;
                        case "decomposition":
                            state.Decomposition = node.InnerText;
                            break;
                        case "executionOrder":
                            state.ExecutionOrder = int.Parse(node.InnerText);
                            break;
                    }

                    break;
                }
                case "activeStateOutput":
                {
                    foreach (XmlNode childrenNode in node.ChildNodes)
                    {
                        if (childrenNode.Name == "P")
                        {
                            var name = childrenNode.Attributes?["Name"]?.Value;
                            switch (name)
                            {
                                case null:
                                    continue;
                                case "useCustomName":
                                    State.ActiveStateOutput.UseCustomName = ParseBool(node.InnerText);
                                    break;
                                case "customName":
                                    State.ActiveStateOutput.CustomName = node.InnerText;
                                    break;
                                case "useCustomEnumTypeName":
                                    State.ActiveStateOutput.UseCustomEnumTypeName = ParseBool(node.InnerText);
                                    break;
                                case "enumTypeName":
                                    State.ActiveStateOutput.EnumTypeName = node.InnerText;
                                    break;
                            }
                        }
                    }

                    break;
                }
                case "Children":
                    foreach (XmlNode childrenNode in node.ChildNodes)
                    {
                        switch (childrenNode.Name)
                        {
                            case "state":
                                state.ChildrenState.Add(ParseState(childrenNode));
                                break;
                            case "transition":
                                state.ChildrenTransition.Add(ParseTransition(childrenNode));
                                break;
                        }
                    }

                    break;
            }
        }

        return state;
    }


    private Transition ParseTransition(XmlNode xmlNode)
    {
        var transition = new Transition
        {
            Id = ParseId(xmlNode)
        };

        foreach (XmlNode node in xmlNode.ChildNodes)
        {
            switch (node.Name)
            {
                case "P":
                {
                    var name = node.Attributes?["Name"]?.Value;
                    switch (name)
                    {
                        case null:
                            continue;
                        case "labelString":
                            transition.LabelString = node.InnerText;
                            break;
                        case "labelPosition":
                            transition.LabelPosition = Parse4Point(node.InnerText);
                            break;
                        case "fontSize":
                            transition.FontSize = int.Parse(node.InnerText);
                            break;
                        case "midPoint":
                            transition.MidPoint = ParsePoint(node.InnerText);
                            break;
                        case "dataLimits":
                            transition.DataLimits = Parse4Point(node.InnerText);
                            break;
                        case "subviewer":
                            transition.Subviewer = int.Parse(node.InnerText);
                            break;
                        case "drawStyle":
                            transition.DrawStyle = node.InnerText;
                            break;
                        case "executionOrder":
                            transition.ExecutionOrder = int.Parse(node.InnerText);
                            break;
                    }

                    break;
                }
                case "src":
                    transition.Src = ParseAddress(node);
                    break;
                case "dst":
                    transition.Dst = ParseAddress(node);
                    break;
            }
        }

        return transition;
    }

    private static Address ParseAddress(XmlNode xmlNode)
    {
        var address = new Address();
        foreach (XmlNode node in xmlNode.ChildNodes)
        {
            switch (node.Name)
            {
                case "P":
                {
                    var name = node.Attributes?["Name"]?.Value;
                    switch (name)
                    {
                        case null:
                            continue;
                        case "SSID":
                            address.SSID = Int2Guid(int.Parse(node.InnerText));
                            break;
                        case "intersection":
                            address.Intersection = Parse8Point(node.InnerText);
                            break;
                    }

                    break;
                }
            }
        }

        return address;
    }


    private static Data.Data ParseData(XmlNode xmlNode)
    {
        var data = new Data.Data
        {
            Id = ParseId(xmlNode),
            Name = ParseName(xmlNode)
        };

        foreach (XmlNode node in xmlNode.ChildNodes)
        {
            switch (node.Name)
            {
                case "P":
                {
                    var name = node.Attributes?["Name"]?.Value;
                    switch (name)
                    {
                        case null:
                            continue;
                        case "scope":
                            data.Scope = node.InnerText;
                            break;
                        case "dataType":
                            data.DataType = node.InnerText;
                            break;
                    }

                    break;
                }
                case "props":
                    foreach (XmlNode childrenNode in node.ChildNodes)
                    {
                        switch (childrenNode.Name)
                        {
                            case "P":
                            {
                                var name = childrenNode.Attributes?["Name"]?.Value;
                                switch (name)
                                {
                                    case null:
                                        continue;
                                    case "frame":
                                        data.Props.Frame = childrenNode.InnerText;
                                        break;
                                }

                                break;
                            }
                            case "type":
                            {
                                foreach (XmlNode childrenNode2 in childrenNode.ChildNodes)
                                {
                                    if (childrenNode2.Name == "P")
                                    {
                                        var name = childrenNode2.Attributes?["Name"]?.Value;
                                        switch (name)
                                        {
                                            case null:
                                                continue;
                                            case "method":
                                                data.Props.TypeMethod = childrenNode2.InnerText;
                                                break;
                                            case "primitive":
                                                data.Props.TypePrimitive = childrenNode2.InnerText;
                                                break;
                                            case "wordLength":
                                                data.Props.TypeWordLength = int.Parse(childrenNode2.InnerText);
                                                break;
                                        }

                                        break;
                                    }

                                    switch (childrenNode2.Name)
                                    {
                                        case "fixpt":
                                        {
                                            foreach (XmlNode childrenNode3 in childrenNode2.ChildNodes)
                                            {
                                                if (childrenNode3.Name == "P")
                                                {
                                                    var name = childrenNode3.Attributes?["Name"]?.Value;
                                                    switch (name)
                                                    {
                                                        case null:
                                                            continue;
                                                        case "scalingMode":
                                                            data.Props.TypeFixptScalingMode =
                                                                childrenNode3.InnerText;
                                                            break;
                                                        case "fractionLength":
                                                            data.Props.TypeFixptFractionLength =
                                                                int.Parse(childrenNode3.InnerText);
                                                            break;
                                                        case "slope":
                                                            data.Props.TypeFixptSlope = childrenNode3.InnerText;
                                                            break;
                                                        case "bias":
                                                            data.Props.TypeFixptBias =
                                                                int.Parse(childrenNode3.InnerText);
                                                            break;
                                                    }
                                                }
                                            }

                                            break;
                                        }
                                    }
                                }

                                break;
                            }
                            case "unit":
                            {
                                foreach (XmlNode childrenNode2 in childrenNode.ChildNodes)
                                {
                                    if (childrenNode2.Name == "P")
                                    {
                                        var name = childrenNode2.Attributes?["Name"]?.Value;
                                        switch (name)
                                        {
                                            case null:
                                                continue;
                                            case "frame":
                                                data.Props.UnitName = childrenNode2.InnerText;
                                                break;
                                        }
                                    }
                                }

                                break;
                            }
                        }
                    }

                    break;
            }
        }

        return data;
    }
        
    public static Guid Int2Guid(int value)
    {
        byte[] bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
        
    private static Guid ParseId(XmlNode xmlNode)
    {
        var id = xmlNode.Attributes?["id"];
        if (id != null) return Int2Guid(int.Parse(id.Value));
        var name = xmlNode.Attributes?["Name"];
        if (name != null && name.Value == "SSID")
        {
            return Int2Guid(int.Parse(xmlNode.InnerText));
        }
        var ssid = xmlNode.Attributes?["SSID"];
        return ssid != null ? Int2Guid(int.Parse(ssid.Value)) : new Guid();
    }


    private static string ParseName(XmlNode xmlNode)
    {
        var name = xmlNode.Attributes?["name"]?.Value;
        return name ?? "";
    }

    private static Point2D ParsePoint(string line)
    {
        line = line.Substring(1, line.Length - 2);
        var numbers = line.Split(' ');

        var x1 = float.Parse(numbers[0], CultureInfo.InvariantCulture);
        var y1 = float.Parse(numbers[1], CultureInfo.InvariantCulture);

        return new Point2D(x1, y1);
    }

    private static DoublePoint Parse4Point(string line)
    {
        line = line.Substring(1, line.Length - 2);

        var numbers = line.Split(' ');

        var x1 = float.Parse(numbers[0], CultureInfo.InvariantCulture);
        var y1 = float.Parse(numbers[1], CultureInfo.InvariantCulture);
        var x2 = float.Parse(numbers[2], CultureInfo.InvariantCulture);
        var y2 = float.Parse(numbers[3], CultureInfo.InvariantCulture);

        return new DoublePoint(new Point2D(x1, y1), new Point2D(x2, y2));
    }

    private static DoubleDoublePoint Parse8Point(string line)
    {
        line = line.Substring(1, line.Length - 2);
        var numbers = line.Split(' ');

        var x1 = float.Parse(numbers[0], CultureInfo.InvariantCulture);
        var y1 = float.Parse(numbers[1], CultureInfo.InvariantCulture);
        var x2 = float.Parse(numbers[2], CultureInfo.InvariantCulture);
        var y2 = float.Parse(numbers[3], CultureInfo.InvariantCulture);
        var x3 = float.Parse(numbers[4], CultureInfo.InvariantCulture);
        var y3 = float.Parse(numbers[5], CultureInfo.InvariantCulture);
        var x4 = float.Parse(numbers[6], CultureInfo.InvariantCulture);
        var y4 = float.Parse(numbers[7], CultureInfo.InvariantCulture);

        return new DoubleDoublePoint(new DoublePoint(new Point2D(x1, y1), new Point2D(x2, y2)),
            new DoublePoint(new Point2D(x3, y3), new Point2D(x4, y4)));
    }

    private static Color ParseColor(string line)
    {
        line = line.Substring(1, line.Length - 2);
        var numbers = line.Split(' ');

        var r = (int)(double.Parse(numbers[0], CultureInfo.InvariantCulture) * 256);
        var g = (int)(double.Parse(numbers[1], CultureInfo.InvariantCulture) * 256);
        var b = (int)(double.Parse(numbers[2], CultureInfo.InvariantCulture) * 256);

        return new Color(r, g, b);
    }

    private static bool ParseBool(string line)
    {
        return line == "1";
    }
}