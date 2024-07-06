using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using XSharp.Math.Geometry;

using Rectangle = XSharp.Math.Geometry.Rectangle;
using RectangleF = XSharp.Math.Geometry.RectangleF;
using DX11Rectangle = SharpDX.Rectangle;
using DX11RectangleF = SharpDX.RectangleF;

namespace XSharp.Interop;

public static class RectangleExtensions
{
    public static DX11Rectangle ToDX9Rectangle(this Rectangle rect)
    {
        return new DX11Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static Rectangle ToRectangle(this DX11Rectangle rect)
    {
        return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static DX11RectangleF ToDX9RectangleF(this Rectangle rect)
    {
        return new DX11RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static DX11RectangleF ToDX9RectangleF(this RectangleF rect)
    {
        return new DX11RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static RectangleF ToRectangleF(this DX11Rectangle rect)
    {
        return new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static RectangleF ToRectangleF(this DX11RectangleF rect)
    {
        return new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
    }
}