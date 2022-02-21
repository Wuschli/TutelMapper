using Barbar.HexGrid;
using SkiaSharp;

namespace TutelMapper.Util
{
    public class SkPointPolicy : IPointPolicy<SKPoint>
    {
        public SKPoint Create(double x, double y)
        {
            return new SKPoint((float)x, (float)y);
        }

        public double GetX(SKPoint point)
        {
            return point.X;
        }

        public double GetY(SKPoint point)
        {
            return point.Y;
        }

        public SKPoint Add(SKPoint a, SKPoint b)
        {
            return a + b;
        }
    }
}