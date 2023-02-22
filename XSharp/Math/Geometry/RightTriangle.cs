using System;
using System.Collections.Generic;

namespace XSharp.Math.Geometry
{
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

    public struct RightTriangle : IShape
    {
        public const GeometryType type = GeometryType.RIGHT_TRIANGLE;

        public static readonly RightTriangle EMPTY = new(Vector.NULL_VECTOR, 0, 0);
        private readonly FixedSingle hCathetus;
        private readonly FixedSingle vCathetus;

        public Vector Origin
        {
            get;
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

        public RightTriangle Translate(Vector shift)
        {
            return new(Origin + shift, hCathetus, vCathetus);
        }

        public RightTriangle Negate()
        {
            return new(-Origin, -hCathetus, -vCathetus);
        }

        public FixedDouble Area => FixedDouble.HALF * (FixedDouble) HCathetus * (FixedDouble) VCathetus;

        public Vector GetNormal(RightTriangleSide side)
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
                    return v.Versor();
                }
            }

            return Vector.NULL_VECTOR;
        }

        public bool Contains(Vector v, FixedSingle epslon, RightTriangleSide include = RightTriangleSide.ALL)
        {
            if (!include.HasFlag(RightTriangleSide.INNER))
                return include.HasFlag(RightTriangleSide.HCATHETUS) && HCathetusLine.Contains(v, epslon)
                        || include.HasFlag(RightTriangleSide.VCATHETUS) && VCathetusLine.Contains(v, epslon)
                        || include.HasFlag(RightTriangleSide.HYPOTENUSE) && HypotenuseLine.Contains(v, epslon);

            var dx = v.X - Origin.X;
            var dy = v.Y - Origin.Y;
            var xl = vCathetus != 0 ? (FixedSingle) (hCathetus * (1 - (FixedDouble) dy / vCathetus)) : hCathetus;
            var yl = hCathetus != 0 ? (FixedSingle) (vCathetus * (1 - (FixedDouble) dx / hCathetus)) : vCathetus;

            var interval = Interval.MakeInterval((0, include.HasFlag(RightTriangleSide.VCATHETUS)), (xl, include.HasFlag(RightTriangleSide.HYPOTENUSE)));

            if (!interval.Contains(dx, epslon))
                return false;

            interval = Interval.MakeInterval((0, include.HasFlag(RightTriangleSide.HCATHETUS)), (yl, include.HasFlag(RightTriangleSide.HYPOTENUSE)));

            return interval.Contains(dy, epslon);
        }

        public bool Contains(Vector v, RightTriangleSide include)
        {
            return Contains(v, 0, include);
        }

        public bool Contains(Vector point)
        {
            return Contains(point, RightTriangleSide.ALL);
        }

        public bool HasIntersectionWith(LineSegment line, FixedSingle epslon, RightTriangleSide include = RightTriangleSide.ALL)
        {
            bool hasIntersectionWithHypothenuse = HypotenuseLine.HasIntersectionWith(line, epslon);
            bool hasIntersectionWithHCathetus = HCathetusLine.HasIntersectionWith(line, epslon);
            bool hasIntersectionWithVCathetus = VCathetusLine.HasIntersectionWith(line, epslon);

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

        public bool HasIntersectionWith(LineSegment line, RightTriangleSide include = RightTriangleSide.ALL)
        {
            return HasIntersectionWith(line, 0, include);
        }

        public bool HasIntersectionWith(Box box, FixedSingle epslon, RightTriangleSide include = RightTriangleSide.ALL)
        {
            Box wrappingBox = WrappingBox;
            Box intersection = box & wrappingBox;

            return intersection.IsValid(epslon) && (
                    intersection == wrappingBox
                    || include.HasFlag(RightTriangleSide.INNER) && (
                        Contains(intersection.LeftTop)
                        || Contains(intersection.RightTop)
                        || Contains(intersection.LeftBottom)
                        || Contains(intersection.RightBottom)
                    )
                    || include.HasFlag(RightTriangleSide.HYPOTENUSE) && intersection.HasIntersectionWith(HypotenuseLine, epslon)
                    || include.HasFlag(RightTriangleSide.HCATHETUS) && intersection.HasIntersectionWith(HCathetusLine, epslon)
                    || include.HasFlag(RightTriangleSide.VCATHETUS) && intersection.HasIntersectionWith(VCathetusLine, epslon)
                );
            ;
        }

        public bool HasIntersectionWith(Box box, RightTriangleSide include = RightTriangleSide.ALL)
        {
            return HasIntersectionWith(box, 0, include);
        }

        public bool HasIntersectionWith(RightTriangle triangle, FixedSingle epslon, RightTriangleSide include = RightTriangleSide.ALL)
        {
            // TODO : Implement!
            throw new NotImplementedException();
        }

        public bool HasIntersectionWith(RightTriangle triangle, RightTriangleSide include = RightTriangleSide.ALL)
        {
            return HasIntersectionWith(triangle, 0, include);
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
                    GeometrySet set => set.HasIntersectionWith(this),
                    _ => false,
                };
        }

        public override string ToString()
        {
            return "[" + Origin + " : " + hCathetus + " : " + vCathetus + "]";
        }

        public override int GetHashCode()
        {
            int hashCode = 2005360325;
            hashCode = hashCode * -1521134295 + hCathetus.GetHashCode();
            hashCode = hashCode * -1521134295 + vCathetus.GetHashCode();
            hashCode = hashCode * -1521134295 + Origin.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is RightTriangle triangle &&
                   EqualityComparer<FixedSingle>.Default.Equals(hCathetus, triangle.hCathetus) &&
                   EqualityComparer<FixedSingle>.Default.Equals(vCathetus, triangle.vCathetus) &&
                   EqualityComparer<Vector>.Default.Equals(Origin, triangle.Origin);
        }

        public GeometryType Type => type;

        public FixedSingle Length => HCathetus + VCathetus + Hypotenuse;

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

    public static class GeometryOperations
    {
        public static void HorizontalParallelogram(Vector origin, Vector direction, FixedSingle smallWidth, out Box box, out RightTriangle triangle1, out RightTriangle triangle2)
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

        public static void VerticalParallelogram(Vector origin, Vector direction, FixedSingle smallHeight, out Box box, out RightTriangle triangle1, out RightTriangle triangle2)
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
    }
}