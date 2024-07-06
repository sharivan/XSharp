using DX9Rectangle = SharpDX.Rectangle;
using DX9RectangleF = SharpDX.RectangleF;
using Rectangle = XSharp.Math.Geometry.Rectangle;
using RectangleF = XSharp.Math.Geometry.RectangleF;

namespace XSharp.Interop;

public static class RectangleExtensions
{
    public static DX9Rectangle ToDX9Rectangle(this Rectangle rect)
    {
        return new DX9Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static Rectangle ToRectangle(this DX9Rectangle rect)
    {
        return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static DX9RectangleF ToDX9RectangleF(this Rectangle rect)
    {
        return new DX9RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static DX9RectangleF ToDX9RectangleF(this RectangleF rect)
    {
        return new DX9RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static RectangleF ToRectangleF(this DX9Rectangle rect)
    {
        return new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
    }

    public static RectangleF ToRectangleF(this DX9RectangleF rect)
    {
        return new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
    }
}