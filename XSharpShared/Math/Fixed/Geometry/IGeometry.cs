using System;
using XSharp.Math.Fixed;

namespace XSharp.Math.Fixed.Geometry;

[Flags]
public enum GeometryType
{
    EMPTY = 0,
    VECTOR = 1,
    LINE_SEGMENT = 2,
    RIGHT_TRIANGLE = 4,
    BOX = 8,
    POLIGON = 16,

    SET = 1 << 31,

    VECTOR_SET = VECTOR | SET,
    LINE_SET = LINE_SEGMENT | SET,
    BOX_SET = BOX | SET,
    RIGHT_TRIANGLE_SET = RIGHT_TRIANGLE | SET
}

public enum Metric
{
    EUCLIDIAN,
    SUM,
    MAX,
    DISCRETE
}

public interface IGeometry
{
    GeometryType Type
    {
        get;
    }

    FixedSingle Length
    {
        get;
    }

    bool Contains(Vector v);

    bool HasIntersectionWith(IGeometry geometry);

    FixedSingle GetLength(Metric metric);
}

public struct EmptyGeometry : IGeometry
{
    public const GeometryType type = GeometryType.EMPTY;

    public FixedSingle Length => FixedSingle.ZERO;

    public GeometryType Type => type;

    public bool Contains(Vector v)
    {
        return false;
    }

    public bool HasIntersectionWith(IGeometry geometry)
    {
        return false;
    }

    public FixedSingle GetLength(Metric metric)
    {
        return FixedSingle.ZERO;
    }
}

public struct UniverseGeometry : IGeometry
{
    public const GeometryType type = GeometryType.EMPTY;

    public FixedSingle Length => FixedSingle.MAX_VALUE;

    public GeometryType Type => type;

    public bool Contains(Vector v)
    {
        return true;
    }

    public bool HasIntersectionWith(IGeometry geometry)
    {
        return true;
    }

    public FixedSingle GetLength(Metric metric)
    {
        return FixedSingle.MAX_VALUE;
    }
}

public interface IShape : IGeometry
{
    FixedDouble Area
    {
        get;
    }
}