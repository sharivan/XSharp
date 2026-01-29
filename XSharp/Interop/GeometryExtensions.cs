using XSharp.Math.Fixed.Geometry;
using XSharp.Math.Geometry;

namespace XSharp.Interop;

public static class GeometryExtensions
{
    public static Point ToPoint(this Vector v)
    {
        return new((int) v.X, (int) v.Y);
    }

    public static Vector2 ToVector2(this Vector v)
    {
        return new((float) v.X, (float) v.Y);
    }

    public static Vector3 ToVector3(this Vector v)
    {
        return new((float) v.X, (float) v.Y, 0);
    }

    public static Vector4 ToVector4(this Vector v)
    {
        return new((float) v.X, (float) v.Y, 0, 0);
    }

    public static Size2 ToSize2(this Vector v)
    {
        return new((int) v.X, (int) v.Y);
    }

    public static Size2F ToSize2F(this Vector v)
    {
        return new((float) v.X, (float) v.Y);
    }

    public static Rectangle ToRectangle(this Box box)
    {
        return new((int) box.Left, (int) box.Top, (int) box.Width, (int) box.Height);
    }

    public static RectangleF ToRectangleF(this Box box)
    {
        return new((float) box.Left, (float) box.Top, (float) box.Width, (float) box.Height);
    }

    public static Vector ToVector(this Point p)
    {
        return new(p.X, p.Y);
    }

    public static Vector ToVector(this Vector2 v)
    {
        return new(v.X, v.Y);
    }

    public static Box ToBox(this Rectangle rect)
    {
        return new(rect.Left, rect.Top, rect.Width, rect.Height);
    }

    public static Box ToBox(this RectangleF rect)
    {
        return new(rect.Left, rect.Top, rect.Width, rect.Height);
    }
}