using System;
using System.Collections.Generic;

using XSharp.Serialization;

namespace XSharp.Math.Geometry;

[Flags]
public enum RightTriangleSide
{
    NONE = 0,
    HCATHETUS = 1,
    VCATHETUS = 2,
    HYPOTENUSE = 4,
    INNER = 8,

    CATHETUS = HCATHETUS | VCATHETUS,
    BORDERS = CATHETUS | HYPOTENUSE,
    ALL = BORDERS | INNER
}

public struct RightTriangle : IShape, ISerializable
{
    public const GeometryType type = GeometryType.RIGHT_TRIANGLE;

    public static readonly RightTriangle EMPTY = new(Vector.NULL_VECTOR, 0, 0);

    private FixedSingle hCathetus;
    private FixedSingle vCathetus;

    public GeometryType Type => type;

    public Vector Origin
    {
        get;
        private set;
    }

    public Vector HypothenuseOpositeVertex => Origin;

    public Vector VCathetusOpositeVertex => Origin + HCathetusVector;

    public Vector HCathetusOpositeVertex => Origin + VCathetusVector;

    public FixedSingle HCathetus => hCathetus.Abs;

    public FixedSingle VCathetus => vCathetus.Abs;

    public FixedSingle Hypotenuse
    {
        get
        {
            FixedDouble h = hCathetus;
            FixedDouble v = vCathetus;
            return System.Math.Sqrt(h * h + v * v);
        }
    }

    public FixedSingle Length => HCathetus + VCathetus + Hypotenuse;

    public FixedDouble Area => FixedDouble.HALF * (FixedDouble) HCathetus * (FixedDouble) VCathetus;

    public Vector HCathetusVector => new(hCathetus, 0);

    public Vector VCathetusVector => new(0, vCathetus);

    public Vector HypotenuseVector => HCathetusVector - VCathetusVector;

    public LineSegment HypotenuseLine => new(VCathetusOpositeVertex, HCathetusOpositeVertex);

    public LineSegment HCathetusLine => new(Origin, VCathetusOpositeVertex);

    public LineSegment VCathetusLine => new(Origin, HCathetusOpositeVertex);

    public Box WrappingBox => new(FixedSingle.Min(Origin.X, Origin.X + hCathetus), FixedSingle.Min(Origin.Y, Origin.Y + vCathetus), HCathetus, VCathetus);

    public FixedSingle Left => FixedSingle.Min(Origin.X, Origin.X + hCathetus);

    public FixedSingle Top => FixedSingle.Min(Origin.Y, Origin.Y + vCathetus);

    public FixedSingle Right => FixedSingle.Max(Origin.X, Origin.X + hCathetus);

    public FixedSingle Bottom => FixedSingle.Max(Origin.Y, Origin.Y + vCathetus);

    public Vector LeftTop => new(Left, Top);

    public Vector RightBottom => new(Right, Bottom);

    public int HCathetusSign => hCathetus.Signal;

    public int VCathetusSign => vCathetus.Signal;

    public RightTriangle(Vector origin, FixedSingle hCathetus, FixedSingle vCathetus)
    {
        Origin = origin;
        this.hCathetus = hCathetus;
        this.vCathetus = vCathetus;
    }

    public RightTriangle(Vector leftTop, FixedSingle width, FixedSingle height, bool vCathetusOnTheLeft, bool hCathetusOnTheTop)
    {
        FixedSingle x;
        FixedSingle y;

        if (vCathetusOnTheLeft)
        {
            x = leftTop.X;
            hCathetus = width;
        }
        else
        {
            x = leftTop.X + width;
            hCathetus = -width;
        }

        if (hCathetusOnTheTop)
        {
            y = leftTop.Y;
            vCathetus = height;
        }
        else
        {
            y = leftTop.Y + height;
            vCathetus = -height;
        }

        Origin = (x, y);
    }

    public RightTriangle(Box wrappingBox, bool vCathetusOnTheLeft, bool hCathetusOnTheTop)
        : this(wrappingBox.LeftTop, wrappingBox.Width, wrappingBox.Height, vCathetusOnTheLeft, hCathetusOnTheTop)
    {
    }

    public RightTriangle(BinarySerializer reader)
    {
        Deserialize(reader);
    }

    public void Deserialize(BinarySerializer reader)
    {
        Origin = reader.ReadVector();
        hCathetus = reader.ReadFixedSingle();
        vCathetus = reader.ReadFixedSingle();
    }

    public void Serialize(BinarySerializer writer)
    {
        Origin.Serialize(writer);
        hCathetus.Serialize(writer);
        vCathetus.Serialize(writer);
    }

    public FixedSingle GetLength(Metric metric)
    {
        return HypothenuseOpositeVertex.DistanceTo(HCathetusOpositeVertex, metric)
            + HCathetusOpositeVertex.DistanceTo(VCathetusOpositeVertex, metric)
            + VCathetusOpositeVertex.DistanceTo(HypothenuseOpositeVertex, metric);
    }

    public RightTriangle Translate(Vector shift)
    {
        return new(Origin + shift, hCathetus, vCathetus);
    }

    public RightTriangle Negate()
    {
        return new(-Origin, -hCathetus, -vCathetus);
    }

    public Vector GetNormal(RightTriangleSide side, Metric metric = Metric.EUCLIDIAN)
    {
        switch (side)
        {
            case RightTriangleSide.HCATHETUS:
                return vCathetus >= 0 ? Vector.UP_VECTOR : Vector.DOWN_VECTOR;

            case RightTriangleSide.VCATHETUS:
                return hCathetus >= 0 ? Vector.RIGHT_VECTOR : Vector.LEFT_VECTOR;

            case RightTriangleSide.HYPOTENUSE:
            {
                int sign = vCathetus.Signal == hCathetus.Signal ? 1 : -1;
                var v = new Vector(sign * vCathetus, -sign * hCathetus);
                return v.Versor(metric);
            }
        }

        return Vector.NULL_VECTOR;
    }

    public bool Contains(Vector v, RightTriangleSide include = RightTriangleSide.ALL)
    {
        if (!include.HasFlag(RightTriangleSide.INNER))
        {
            return include.HasFlag(RightTriangleSide.HCATHETUS) && HCathetusLine.Contains(v)
                    || include.HasFlag(RightTriangleSide.VCATHETUS) && VCathetusLine.Contains(v)
                    || include.HasFlag(RightTriangleSide.HYPOTENUSE) && HypotenuseLine.Contains(v);
        }

        var dx = v.X - Origin.X;
        var dy = v.Y - Origin.Y;
        var xl = vCathetus != 0 ? (FixedSingle) (hCathetus * (1 - (FixedDouble) dy / vCathetus)) : hCathetus;
        var yl = hCathetus != 0 ? (FixedSingle) (vCathetus * (1 - (FixedDouble) dx / hCathetus)) : vCathetus;

        var interval = Interval.MakeInterval((0, include.HasFlag(RightTriangleSide.VCATHETUS)), (xl, include.HasFlag(RightTriangleSide.HYPOTENUSE)));

        if (!interval.Contains(dx))
            return false;

        interval = Interval.MakeInterval((0, include.HasFlag(RightTriangleSide.HCATHETUS)), (yl, include.HasFlag(RightTriangleSide.HYPOTENUSE)));

        return interval.Contains(dy);
    }

    public bool Contains(Vector point)
    {
        return Contains(point, RightTriangleSide.ALL);
    }

    public bool HasIntersectionWith(LineSegment line, RightTriangleSide include = RightTriangleSide.ALL)
    {
        bool hasIntersectionWithHypothenuse = HypotenuseLine.HasIntersectionWith(line);
        bool hasIntersectionWithHCathetus = HCathetusLine.HasIntersectionWith(line);
        bool hasIntersectionWithVCathetus = VCathetusLine.HasIntersectionWith(line);

        return include.HasFlag(RightTriangleSide.HYPOTENUSE) && hasIntersectionWithHypothenuse
            || include.HasFlag(RightTriangleSide.HCATHETUS) && hasIntersectionWithHCathetus
            || include.HasFlag(RightTriangleSide.VCATHETUS) && hasIntersectionWithVCathetus
            || include.HasFlag(RightTriangleSide.INNER) && (
                hasIntersectionWithHypothenuse
                || hasIntersectionWithHCathetus
                || hasIntersectionWithVCathetus
                || Contains(line.Start, RightTriangleSide.INNER)
                || Contains(line.End, RightTriangleSide.INNER)
            );
    }

    public bool HasIntersectionWith(Box box, RightTriangleSide include = RightTriangleSide.ALL)
    {
        Box wrappingBox = WrappingBox;
        Box intersection = box & wrappingBox;

        return intersection.IsValid() && (
                intersection == wrappingBox
                || include.HasFlag(RightTriangleSide.INNER) && (
                    Contains(intersection.LeftTop)
                    || Contains(intersection.RightTop)
                    || Contains(intersection.LeftBottom)
                    || Contains(intersection.RightBottom)
                )
                || include.HasFlag(RightTriangleSide.HYPOTENUSE) && intersection.HasIntersectionWith(HypotenuseLine)
                || include.HasFlag(RightTriangleSide.HCATHETUS) && intersection.HasIntersectionWith(HCathetusLine)
                || include.HasFlag(RightTriangleSide.VCATHETUS) && intersection.HasIntersectionWith(VCathetusLine)
            );
    }

    public bool HasIntersectionWith(RightTriangle triangle)
    {
        Box intersection = triangle.WrappingBox & WrappingBox;
        return intersection.IsValid() && (
            Contains(triangle.HypothenuseOpositeVertex)
            || Contains(triangle.HCathetusOpositeVertex)
            || Contains(triangle.VCathetusOpositeVertex)
            || triangle.Contains(HypothenuseOpositeVertex)
            || triangle.Contains(HCathetusOpositeVertex)
            || triangle.Contains(VCathetusOpositeVertex)
            || triangle.HasIntersectionWith(HypotenuseLine)
            || triangle.HasIntersectionWith(HCathetusLine)
            || triangle.HasIntersectionWith(VCathetusLine));
    }

    public bool HasIntersectionWith(IGeometry geometry)
    {
        return (IGeometry) this == geometry
            || geometry switch
            {
                Vector v => Contains(v),
                Box box => HasIntersectionWith(box),
                LineSegment line => HasIntersectionWith(line),
                RightTriangle triangle => HasIntersectionWith(triangle),
                _ => throw new NotImplementedException()
            };
    }

    public RightTriangle FlipAroundHypothenuse()
    {
        return new RightTriangle(Origin + (hCathetus, vCathetus), -hCathetus, -vCathetus);
    }

    public override string ToString()
    {
        return "[" + Origin + " : " + hCathetus + " : " + vCathetus + "]";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(hCathetus, vCathetus, Origin);
    }

    public override bool Equals(object obj)
    {
        return obj is RightTriangle triangle &&
               EqualityComparer<FixedSingle>.Default.Equals(hCathetus, triangle.hCathetus) &&
               EqualityComparer<FixedSingle>.Default.Equals(vCathetus, triangle.vCathetus) &&
               EqualityComparer<Vector>.Default.Equals(Origin, triangle.Origin);
    }

    public static bool operator ==(RightTriangle left, RightTriangle right)
    {
        return left.Origin == right.Origin && left.hCathetus == right.hCathetus && left.vCathetus == right.vCathetus;
    }

    public static bool operator !=(RightTriangle left, RightTriangle right)
    {
        return left.Origin != right.Origin || left.hCathetus != right.hCathetus || left.vCathetus != right.vCathetus;
    }

    public static RightTriangle operator +(RightTriangle triangle, Vector shift)
    {
        return triangle.Translate(shift);
    }

    public static RightTriangle operator +(Vector shift, RightTriangle triangle)
    {
        return triangle.Translate(shift);
    }

    public static RightTriangle operator -(RightTriangle triangle)
    {
        return triangle.Negate();
    }

    public static RightTriangle operator -(RightTriangle triangle, Vector shift)
    {
        return triangle.Translate(-shift);
    }

    public static RightTriangle operator -(Vector shift, RightTriangle triangle)
    {
        return (-triangle).Translate(shift);
    }
}