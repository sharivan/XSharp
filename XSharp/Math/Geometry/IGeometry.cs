using System;

namespace XSharp.Math.Geometry
{
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

        bool HasIntersectionWith(IGeometry geometry);
    }

    public struct EmptyGeometry : IGeometry
    {
        public const GeometryType type = GeometryType.EMPTY;

        public FixedSingle Length => FixedSingle.ZERO;

        public GeometryType Type => type;

        public bool HasIntersectionWith(IGeometry geometry)
        {
            return false;
        }
    }

    public struct UniverseGeometry : IGeometry
    {
        public const GeometryType type = GeometryType.EMPTY;

        public FixedSingle Length => FixedSingle.ZERO;

        public GeometryType Type => type;

        public bool HasIntersectionWith(IGeometry geometry)
        {
            return true;
        }
    }

    public interface IShape : IGeometry
    {
        FixedDouble Area
        {
            get;
        }
    }
}