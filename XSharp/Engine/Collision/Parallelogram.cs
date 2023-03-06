using System;

using XSharp.Math;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Collision;

public enum ParallelogramVertex
{
    LEFT_TOP = 0,
    LEFT_BOTTOM = 1,
    RIGHT_BOTTOM = 2,
    RIGHT_TOP = 3
}

public enum ParallelogramSide
{
    LEFT = 0,
    TOP = 1,
    RIGHT = 2,
    BOTTOM = 3
}

public class Parallelogram : GeometrySet, IShape
{
    private static void HorizontalParallelogram(Vector origin, Vector direction, FixedSingle smallWidth, out Box box, out RightTriangle triangle1, out RightTriangle triangle2)
    {
        if (direction.X > 0)
        {
            if (direction.Y > 0)
            {
                box = new Box(origin, smallWidth + direction.X, direction.Y);
                triangle1 = new RightTriangle(origin + (0, direction.Y), direction.X, -direction.Y);
                triangle2 = new RightTriangle(origin + (direction.X + smallWidth, 0), -direction.X, direction.Y);
            }
            else
            {
                box = new Box(origin + (0, direction.Y), smallWidth + direction.X, -direction.Y);
                triangle1 = new RightTriangle(origin + (0, direction.Y), direction.X, -direction.Y);
                triangle2 = new RightTriangle(origin + (direction.X + smallWidth, 0), -direction.X, direction.Y);
            }
        }
        else if (direction.Y > 0)
        {
            box = new Box(origin + (direction.X, 0), smallWidth - direction.X, direction.Y);
            triangle1 = new RightTriangle(origin + (direction.X, 0), -direction.X, direction.Y);
            triangle2 = new RightTriangle(origin + (-smallWidth, direction.Y), direction.X, -direction.Y);
        }
        else
        {
            box = new Box(origin + direction, smallWidth - direction.X, -direction.Y);
            triangle1 = new RightTriangle(origin + (direction.X, 0), -direction.X, direction.Y);
            triangle2 = new RightTriangle(origin + (-smallWidth, direction.Y), direction.X, -direction.Y);
        }
    }

    private static void VerticalParallelogram(Vector origin, Vector direction, FixedSingle smallHeight, out Box box, out RightTriangle triangle1, out RightTriangle triangle2)
    {
        if (direction.X > 0)
        {
            if (direction.Y > 0)
            {
                box = new Box(origin, direction.X, smallHeight + direction.Y);
                triangle1 = new RightTriangle(origin + (direction.X, 0), -direction.X, direction.Y);
                triangle2 = new RightTriangle(origin + (0, direction.Y + smallHeight), direction.X, -direction.Y);
            }
            else
            {
                box = new Box(origin + (0, direction.Y), direction.X, smallHeight - direction.Y);
                triangle1 = new RightTriangle(origin + (0, direction.Y), direction.X, -direction.Y);
                triangle2 = new RightTriangle(origin + (direction.X, smallHeight), -direction.X, direction.Y);
            }
        }
        else if (direction.Y > 0)
        {
            box = new Box(origin + (direction.X, 0), -direction.X, smallHeight + direction.Y);
            triangle1 = new RightTriangle(origin + (direction.X, 0), -direction.X, direction.Y);
            triangle2 = new RightTriangle(origin + (0, direction.Y + smallHeight), direction.X, -direction.Y);
        }
        else
        {
            box = new Box(origin + direction, -direction.X, smallHeight - direction.Y);
            triangle1 = new RightTriangle(origin + (0, direction.Y), direction.X, -direction.Y);
            triangle2 = new RightTriangle(origin + (direction.X, smallHeight), -direction.X, direction.Y);
        }
    }

    private Vector[] vertices;
    private LineSegment[] sides;
    private Box wrappingBox;
    private RightTriangle triangle1;
    private RightTriangle triangle2;

    public Box WrappingBox => wrappingBox;

    public Vector LeftTop => wrappingBox.LeftTop;

    public FixedSingle Width => wrappingBox.Width;

    public FixedSingle Height => wrappingBox.Height;

    public Vector Origin
    {
        get;
        private set;
    }

    public Vector Direction
    {
        get;
        private set;
    }

    public FixedSingle SmallerHeight
    {
        get;
        private set;
    }

    public override FixedSingle Length => 2 * (Direction.Length + SmallerHeight);

    public FixedDouble Area => WrappingBox.Area - triangle1.Area - triangle2.Area;

    public Parallelogram() : base(SetOperation.INTERSECTION, (Box.EMPTY_BOX, false), (RightTriangle.EMPTY, true), (RightTriangle.EMPTY, true))
    {
        vertices = new Vector[4];
        sides = new LineSegment[4];
    }

    public LineSegment GetSegment(ParallelogramSide side)
    {
        return sides[(int) side];
    }

    public Vector GetVertex(ParallelogramVertex vertex)
    {
        return vertices[(int) vertex];
    }

    public void SetupHorizontal(Vector origin, Vector direction, FixedSingle smallerHeight)
    {
        HorizontalParallelogram(origin, direction, smallerHeight, out wrappingBox, out triangle1, out triangle2);

        parts[0] = (wrappingBox, false);
        parts[1] = (triangle1, true);
        parts[2] = (triangle2, true);

        if (direction.X.Signal * direction.Y.Signal > 0)
        {
            vertices[0] = triangle1.HCathetusOpositeVertex;
            vertices[1] = triangle1.VCathetusOpositeVertex;
            vertices[2] = triangle2.HCathetusOpositeVertex;
            vertices[3] = triangle2.VCathetusOpositeVertex;
        }
        else
        {
            vertices[0] = triangle1.VCathetusOpositeVertex;
            vertices[1] = triangle1.HCathetusOpositeVertex;
            vertices[2] = triangle2.VCathetusOpositeVertex;
            vertices[3] = triangle2.HCathetusOpositeVertex;
        }

        sides[0] = new LineSegment(vertices[0], vertices[1]);
        sides[1] = new LineSegment(vertices[1], vertices[2]);
        sides[2] = new LineSegment(vertices[2], vertices[3]);
        sides[3] = new LineSegment(vertices[3], vertices[0]);

        Origin = origin;
        Direction = direction;
        SmallerHeight = smallerHeight;
    }

    public void SetupVertical(Vector origin, Vector direction, FixedSingle smallerHeight)
    {
        VerticalParallelogram(origin, direction, smallerHeight, out wrappingBox, out triangle1, out triangle2);

        parts[0] = (wrappingBox, false);
        parts[1] = (triangle1, true);
        parts[2] = (triangle2, true);

        if (direction.X.Signal * direction.Y.Signal > 0)
        {
            vertices[0] = triangle1.VCathetusOpositeVertex;
            vertices[1] = triangle2.HCathetusOpositeVertex;
            vertices[2] = triangle2.VCathetusOpositeVertex;
            vertices[3] = triangle1.HCathetusOpositeVertex;
        }
        else
        {
            vertices[0] = triangle1.HCathetusOpositeVertex;
            vertices[1] = triangle2.VCathetusOpositeVertex;
            vertices[2] = triangle2.HCathetusOpositeVertex;
            vertices[3] = triangle1.VCathetusOpositeVertex;
        }

        sides[0] = new LineSegment(vertices[0], vertices[1]);
        sides[1] = new LineSegment(vertices[1], vertices[2]);
        sides[2] = new LineSegment(vertices[2], vertices[3]);
        sides[3] = new LineSegment(vertices[3], vertices[0]);

        Origin = origin;
        Direction = direction;
        SmallerHeight = smallerHeight;
    }

    public bool HasIntersection(LineSegment line)
    {
        foreach (var side in sides)
        {
            if (side.HasIntersectionWith(line))
                return true;
        }

        return Contains(line.Start) || Contains(line.End);
    }

    public bool HasIntersection(Box box)
    {
        Box intersection = box & wrappingBox;
        if (!intersection.IsValid())
            return false;

        if (intersection == wrappingBox
            || Contains(intersection.LeftTop)
            || Contains(intersection.RightTop)
            || Contains(intersection.LeftBottom)
            || Contains(intersection.RightBottom))
        {
            return true;
        }

        foreach (var side in sides)
        {
            if (intersection.HasIntersectionWith(side))
                return true;
        }

        return false;
    }

    public bool HasIntersection(RightTriangle triangle)
    {
        Box intersection = triangle.WrappingBox & wrappingBox;
        if (!intersection.IsValid())
            return false;

        if (Contains(triangle.HypothenuseOpositeVertex)
            || Contains(triangle.HCathetusOpositeVertex)
            || Contains(triangle.VCathetusOpositeVertex))
        {
            return true;
        }

        foreach (var vertex in vertices)
        {
            if (triangle.Contains(vertex))
                return true;
        }

        foreach (var side in sides)
        {
            if (triangle.HasIntersectionWith(side))
                return true;
        }

        return false;
    }

    public override bool HasIntersectionWith(IGeometry geometry)
    {
        return (IGeometry) this == geometry
            || geometry switch
            {
                Vector v => Contains(v),
                Box box => HasIntersection(box),
                LineSegment line => HasIntersection(line),
                RightTriangle triangle => HasIntersection(triangle),
                _ => throw new NotImplementedException()
            };
    }
}