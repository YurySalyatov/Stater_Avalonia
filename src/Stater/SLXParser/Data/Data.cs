namespace SLXParser.Data
{
    public class Data : BaseNode
    {
        public string Name { get; set; }
        public string Scope { get; set; }
        public Props Props = new Props();
        public string DataType { get; set; }
    }

    public class Props
    {
        public string Frame { get; set; }

        // Type
        public string TypeMethod { get; set; }
        public string TypePrimitive { get; set; }
        public int TypeWordLength { get; set; }

        // Type Fixpt
        public string TypeFixptScalingMode { get; set; }
        public int TypeFixptFractionLength { get; set; }
        public string TypeFixptSlope { get; set; }
        public int TypeFixptBias { get; set; }

        // Unit
        public string UnitName { get; set; }
    }
}