using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Point = XSharp.Math.Geometry.Point;
using SDPoint = System.Drawing.Point;
using DX11Point = SharpDX.Point;

namespace XSharp.Interop;

public static class PointExtensions
{
    public static SDPoint ToSDPoint(this Point point)
    {
        return new SDPoint(point.X, point.Y);
    }

    public static DX11Point ToDX9Point(this Point point)
    {
        return new DX11Point(point.X, point.Y);
    }

    public static Point ToPoint(this SDPoint point)
    {
        return new Point(point.X, point.Y);
    }

    public static Point ToPoint(this DX11Point point)
    {
        return new Point(point.X, point.Y);
    }
}