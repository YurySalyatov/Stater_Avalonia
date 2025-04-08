using System;
using SLXParser.Utils;

namespace SLXParser.Data
{
    public class Transition : BaseNode
    {
        public string LabelString { get; set; }

        public DoublePoint LabelPosition { get; set; }
        public int FontSize { get; set; }

        public Address Src { get; set; }
        public Address Dst { get; set; }
        public Point2D MidPoint { get; set; }
        public DoublePoint DataLimits { get; set; }
        public int Subviewer { get; set; }
        public string DrawStyle { get; set; }
        public int ExecutionOrder { get; set; }
    }

    public class Address
    {
        public Guid SSID { get; set; }
        public DoubleDoublePoint Intersection { get; set; }
    }
}