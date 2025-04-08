namespace SLXParser.Utils
{
    public class Point2D
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Point2D(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public class DoublePoint
    {
        public Point2D X { get; set; }
        public Point2D Y { get; set; }

        public DoublePoint(Point2D x, Point2D y)
        {
            X = x;
            Y = y;
        }
    }
    
    
    public class DoubleDoublePoint
    {
        public DoublePoint X { get; set; }
        public DoublePoint Y { get; set; }

        public DoubleDoublePoint(DoublePoint x, DoublePoint y)
        {
            X = x;
            Y = y;
        }
    }
}