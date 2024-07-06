using DX9Point = SharpDX.Point;
using Point = XSharp.Math.Geometry.Point;
using SDPoint = System.Drawing.Point;

namespace XSharp.Interop;

public static class PointExtensions
{
    public static SDPoint ToSDPoint(this Point point)
    {
        return new SDPoint(point.X, point.Y);
    }

    public static DX9Point ToDX9Point(this Point point)
    {
        return new DX9Point(point.X, point.Y);
    }

    public static Point ToPoint(this SDPoint point)
    {
        return new Point(point.X, point.Y);
    }

    public static Point ToPoint(this DX9Point point)
    {
        return new Point(point.X, point.Y);
    }
}