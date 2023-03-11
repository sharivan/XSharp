using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

using XSharp.Serialization;
using XSharp.Util;

namespace XSharp.Math.Geometry;

[Flags]
public enum BoxSide
{
    NONE = 0,
    LEFT = 1,
    TOP = 2,
    RIGHT = 4,
    BOTTOM = 8,
    INNER = 16,
    OUTER = 32,

    LEFT_TOP = LEFT | TOP,
    RIGHT_BOTTOM = RIGHT | BOTTOM,
    BORDERS = LEFT_TOP | RIGHT_BOTTOM
}

public enum OriginPosition
{
    LEFT_TOP = 0,
    LEFT_MIDDLE = 1,
    LEFT_BOTTOM = 2,
    MIDDLE_TOP = 3,
    CENTER = 4,
    MIDDLE_BOTTOM = 5,
    RIGHT_TOP = 6,
    RIGHT_MIDDLE = 7,
    RIGHT_BOTTOM = 8
}

public class BoxTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        var genericSourceType = sourceType.GetGenericTypeDefinition();
        return sourceType == typeof(Box)
            || (genericSourceType == typeof(ValueTuple<,>) || genericSourceType == typeof(Tuple<,>)
            ? Util.TupleExtensions.CanConvertTupleToArray<Vector>(sourceType)
            : genericSourceType == typeof(ValueTuple<,,>) || genericSourceType == typeof(Tuple<,,>)
            ? Util.TupleExtensions.CanConvertTupleToArray<Vector>(sourceType)
            : genericSourceType == typeof(ValueTuple<,,,>) || genericSourceType == typeof(Tuple<,,,>)
            ? Util.TupleExtensions.CanConvertTupleToArray<FixedSingle>(sourceType)
            : genericSourceType == typeof(ValueTuple<,,,,,>) || genericSourceType == typeof(Tuple<,,,,,>)
            ? Util.TupleExtensions.CanConvertTupleToArray<FixedSingle>(sourceType)
            : base.CanConvertFrom(context, sourceType));
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        Type sourceType = value.GetType();
        if (sourceType == typeof(Box))
            return value;

        var genericSourceType = sourceType.GetGenericTypeDefinition();

        if (genericSourceType == typeof(ValueTuple<,>) || genericSourceType == typeof(Tuple<,>))
        {
            var args = ((ITuple) value).ToArray<Vector>();
            return new Box(args[0], args[1]);
        }

        if (genericSourceType == typeof(ValueTuple<,,>) || genericSourceType == typeof(Tuple<,,>))
        {
            var args = ((ITuple) value).ToArray<Vector>();
            return new Box(args[0], args[1], args[2]);
        }

        if (genericSourceType == typeof(ValueTuple<,,,>) || genericSourceType == typeof(Tuple<,,,>))
        {
            var args = ((ITuple) value).ToArray<FixedSingle>();
            return new Box(args[0], args[1], args[2], args[3]);
        }

        if (genericSourceType == typeof(ValueTuple<,,,,,>) || genericSourceType == typeof(Tuple<,,,,,>))
        {
            var args = ((ITuple) value).ToArray<FixedSingle>();
            return new Box(args[0], args[1], args[2], args[3], args[4], args[5]);
        }

        return base.ConvertFrom(context, culture, value);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        var box = (Box) value;
        if (destinationType == typeof(Box))
            return box;

        var genericDestinationType = destinationType.GetGenericTypeDefinition();

        return genericDestinationType == typeof(ValueTuple<,>) || genericDestinationType == typeof(Tuple<,>)
            ? Util.TupleExtensions.ArrayToTuple(destinationType, box.LeftTop, box.RightBottom)
            : genericDestinationType == typeof(ValueTuple<,,>) || genericDestinationType == typeof(Tuple<,,>)
            ? Util.TupleExtensions.ArrayToTuple(destinationType, box.Origin, box.Mins, box.Maxs)
            : genericDestinationType == typeof(ValueTuple<,,,>) || genericDestinationType == typeof(Tuple<,,,>)
            ? Util.TupleExtensions.ArrayToTuple(destinationType, box.Left, box.Top, box.Width, box.Height)
            : genericDestinationType == typeof(ValueTuple<,,,,,>) || genericDestinationType == typeof(Tuple<,,,,,>)
            ? Util.TupleExtensions.ArrayToTuple(destinationType, box.Origin.X, box.Origin.Y, box.Left, box.Top, box.Width, box.Height)
            : base.ConvertTo(context, culture, value, destinationType);
    }
}

/// <summary>
/// Retângulo bidimensional com lados paralelos aos eixos coordenados
/// </summary>
[TypeConverter(typeof(BoxTypeConverter))]
public struct Box : IShape, ISerializable
{
    public const GeometryType type = GeometryType.BOX;

    /// <summary>
    /// Retângulo vazio
    /// </summary>
    public static readonly Box EMPTY_BOX = new(0, 0, 0, 0);
    /// <summary>
    /// Retângulo universo
    /// </summary>
    public static readonly Box UNIVERSE_BOX = new(Vector.NULL_VECTOR, new Vector(FixedSingle.MIN_VALUE, FixedSingle.MIN_VALUE), new Vector(FixedSingle.MAX_VALUE, FixedSingle.MAX_VALUE));

    public GeometryType Type => type;

    /// <summary>
    /// Origem do retângulo
    /// </summary>
    public Vector Origin
    {
        get;
        private set;
    }

    /// <summary>
    /// Mínimos relativos
    /// </summary>
    public Vector Mins
    {
        get;
        private set;
    }

    /// <summary>
    /// Máximos relativos
    /// </summary>
    public Vector Maxs
    {
        get;
        private set;
    }

    public FixedSingle X => Origin.X;

    public FixedSingle Y => Origin.Y;

    public FixedSingle Left => FixedSingle.Min(Origin.X + Mins.X, Origin.X + Maxs.X);

    public FixedSingle Top => FixedSingle.Min(Origin.Y + Mins.Y, Origin.Y + Maxs.Y);

    public FixedSingle Right => FixedSingle.Max(Origin.X + Mins.X, Origin.X + Maxs.X);

    public FixedSingle Bottom => FixedSingle.Max(Origin.Y + Mins.Y, Origin.Y + Maxs.Y);

    public LineSegment LeftSegment => new(LeftTop, LeftBottom);

    public LineSegment TopSegment => new(LeftTop, RightTop);

    public LineSegment RightSegment => new(RightTop, RightBottom);

    public LineSegment BottomSegment => new(LeftBottom, RightBottom);

    /// <summary>
    /// Extremo superior esquerdo do retângulo (ou mínimos absolutos)
    /// </summary>
    public Vector LeftTop => (Left, Top);

    public Vector LeftMiddle => (Left, (Top + Bottom) * FixedSingle.HALF);

    public Vector LeftBottom => (Left, Bottom);

    public Vector RightTop => (Right, Top);

    public Vector RightMiddle => (Right, (Top + Bottom) * FixedSingle.HALF);

    public Vector MiddleTop => ((Left + Right) * FixedSingle.HALF, Top);

    public Vector MiddleBottom => ((Left + Right) * FixedSingle.HALF, Bottom);

    /// <summary>
    /// Extremo inferior direito do retângulo (ou máximos absolutos)
    /// </summary>
    public Vector RightBottom => (Right, Bottom);

    public Vector Center => Origin + (Mins + Maxs) * FixedSingle.HALF;

    public Vector WidthVector => (Width, 0);

    public Vector HeightVector => (0, Height);

    /// <summary>
    /// Vetor correspondente ao tamanho do retângulo contendo sua largura (width) na coordenada x e sua altura (height) na coordenada y
    /// </summary>
    public Vector DiagonalVector => (Width, Height);

    /// <summary>
    /// Largura (base) do retângulo
    /// </summary>
    public FixedSingle Width => (Maxs.X - Mins.X).Abs;

    /// <summary>
    /// Altura do retângulo
    /// </summary>
    public FixedSingle Height => (Maxs.Y - Mins.Y).Abs;

    public FixedSingle Length => FixedSingle.TWO * (Width + Height);

    /// <summary>
    /// Área do retângulo
    /// </summary>
    /// <returns></returns>
    public FixedDouble Area => (FixedDouble) Width * (FixedDouble) Height;

    /// <summary>
    /// Cria um retângulo vazio com uma determinada origem
    /// </summary>
    /// <param name="origin">origem do retângulo</param>
    public Box(Vector origin)
    {
        Origin = origin;
        Mins = Vector.NULL_VECTOR;
        Maxs = Vector.NULL_VECTOR;
    }

    /// <summary>
    /// Cria um retângulo a partir da origem, mínimos e máximos
    /// </summary>
    /// <param name="origin">Origem</param>
    /// <param name="mins">Mínimos</param>
    /// <param name="maxs">Máximos</param>
    public Box(Vector origin, Vector mins, Vector maxs)
    {
        Origin = origin;
        Mins = mins;
        Maxs = maxs;
    }

    public Box((Vector, Vector, Vector) tuple) : this(tuple.Item1, tuple.Item2, tuple.Item3) { }

    public Box(FixedSingle x, FixedSingle y, FixedSingle width, FixedSingle height)
        : this(new Vector(x, y), width, height)
    {
    }

    public Box((FixedSingle, FixedSingle, FixedSingle, FixedSingle) tuple) : this(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4) { }

    public Box(FixedSingle left, FixedSingle top, FixedSingle width, FixedSingle height, OriginPosition originPosition)
    {
        Origin = originPosition switch
        {
            OriginPosition.LEFT_TOP => (left, top),
            OriginPosition.LEFT_MIDDLE => (left, top + height * FixedSingle.HALF),
            OriginPosition.LEFT_BOTTOM => (left, top + height),
            OriginPosition.MIDDLE_TOP => (left + width * FixedSingle.HALF, top),
            OriginPosition.CENTER => (left + width * FixedSingle.HALF, top + height * FixedSingle.HALF),
            OriginPosition.MIDDLE_BOTTOM => (left + width * FixedSingle.HALF, top + height),
            OriginPosition.RIGHT_TOP => (left + width, top),
            OriginPosition.RIGHT_MIDDLE => (left + width, top + height * FixedSingle.HALF),
            OriginPosition.RIGHT_BOTTOM => (left + width, top + height),
            _ => throw new ArgumentException("Unrecognized Origin Position."),
        };

        Mins = (left, top) - Origin;
        Maxs = (left + width, top + height) - Origin;
    }

    public Box(Vector origin, FixedSingle width, FixedSingle height)
    {
        Origin = origin;
        Mins = Vector.NULL_VECTOR;
        Maxs = (width, height);
    }

    public Box((Vector, FixedSingle, FixedSingle) tuple) : this(tuple.Item1, tuple.Item2, tuple.Item3) { }

    public Box(FixedSingle originX, FixedSingle originY, FixedSingle left, FixedSingle top, FixedSingle width, FixedSingle height)
    {
        Origin = (originX, originY);
        Mins = (left - originX, top - originY);
        Maxs = (left + width - originX, top + height - originY);
    }

    public Box((FixedSingle, FixedSingle, FixedSingle, FixedSingle, FixedSingle, FixedSingle) tuple) : this(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6) { }

    public Box(Vector v1, Vector v2)
    {
        Origin = v1;
        Mins = Vector.NULL_VECTOR;
        Maxs = v2 - v1;
    }

    public Box((Vector, Vector) tuple) : this(tuple.Item1, tuple.Item2) { }

    public Box(BinarySerializer reader)
    {
        Deserialize(reader);
    }

    public void Deserialize(BinarySerializer reader)
    {
        Origin = reader.ReadVector();
        Mins = reader.ReadVector();
        Maxs = reader.ReadVector();
    }

    public void Serialize(BinarySerializer writer)
    {
        Origin.Serialize(writer);
        Mins.Serialize(writer);
        Maxs.Serialize(writer);
    }

    public FixedSingle GetLength(Metric metric)
    {
        return LeftTop.DistanceTo(RightTop, metric) + RightTop.DistanceTo(RightBottom, metric)
            + RightBottom.DistanceTo(LeftBottom, metric) + LeftBottom.DistanceTo(LeftTop, metric);
    }

    /// <summary>
    /// Trunca as coordenadas do retângulo
    /// </summary>
    /// <returns>Retângulo truncado</returns>
    public Box Truncate()
    {
        Vector mins = Origin + Mins;
        Vector maxs = Origin + Maxs;
        return (mins.RoundToFloor(), Vector.NULL_VECTOR, (maxs - mins).RoundToFloor());
    }

    public Box TruncateOrigin()
    {
        return (Origin.Truncate(), Mins, Maxs);
    }

    public override int GetHashCode()
    {
        Vector m = Origin + Mins;
        Vector M = Origin + Maxs;

        return 65536 * m.GetHashCode() + M.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj != null && obj is Box other && this == other;
    }

    public override string ToString()
    {
        return "[" + Origin + " : " + Mins + " : " + Maxs + "]";
    }

    public Box LeftTopOrigin()
    {
        return (LeftTop, Vector.NULL_VECTOR, DiagonalVector);
    }

    public Box RightBottomOrigin()
    {
        return (RightBottom, -DiagonalVector, Vector.NULL_VECTOR);
    }

    public Box CenterOrigin()
    {
        Vector sv2 = DiagonalVector * FixedSingle.HALF;
        return new Box(Center, -sv2, sv2);
    }

    public Box RoundOriginToCeil()
    {
        return (Origin.RoundToCeil(), Mins, Maxs);
    }

    public Box RoundOriginXToCeil()
    {
        return (Origin.RoundXToCeil(), Mins, Maxs);
    }

    public Box RoundOriginYToCeil()
    {
        return (Origin.RoundYToCeil(), Mins, Maxs);
    }

    public Box RoundOriginToFloor()
    {
        return (Origin.RoundToFloor(), Mins, Maxs);
    }

    public Box RoundOriginXToFloor()
    {
        return (Origin.RoundXToFloor(), Mins, Maxs);
    }

    public Box RoundOriginYToFloor()
    {
        return (Origin.RoundYToFloor(), Mins, Maxs);
    }

    public Box RoundOrigin(RoundMode roundXMode = RoundMode.FLOOR, RoundMode roundYMode = RoundMode.FLOOR)
    {
        return (Origin.Round(roundXMode, roundYMode), Mins, Maxs);
    }

    public Box RoundOriginX()
    {
        return (Origin.RoundX(), Mins, Maxs);
    }

    public Box RoundOriginX(RoundMode mode)
    {
        return (Origin.RoundX(mode), Mins, Maxs);
    }

    public Box RoundOriginY()
    {
        return (Origin.RoundY(), Mins, Maxs);
    }

    public Box RoundOriginY(RoundMode mode)
    {
        return (Origin.RoundY(mode), Mins, Maxs);
    }

    /// <summary>
    /// Escala o retângulo para a esquerda
    /// </summary>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public Box ScaleLeft(FixedSingle alpha)
    {
        FixedSingle width = Width;
        return (LeftTop + alpha * width * Vector.LEFT_VECTOR, Vector.NULL_VECTOR, (alpha * width, Height));
    }

    public Box ExtendLeftFixed(FixedSingle fixedWidth)
    {
        return (LeftTop + fixedWidth * Vector.LEFT_VECTOR, Vector.NULL_VECTOR, (fixedWidth, Height));
    }

    public Box ClipLeft(FixedSingle clip)
    {
        return (Origin, (Mins.X + clip, Mins.Y), Maxs);
    }

    /// <summary>
    /// Escala o retângulo para a direita
    /// </summary>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public Box ScaleRight(FixedSingle alpha)
    {
        return (LeftTop, Vector.NULL_VECTOR, (alpha * Width, Height));
    }

    public Box ClipRight(FixedSingle clip)
    {
        return (Origin, Mins, (Maxs.X - clip, Maxs.Y));
    }

    public Box ExtendRightFixed(FixedSingle fixedWidth)
    {
        return (LeftTop, Vector.NULL_VECTOR, (fixedWidth, Height));
    }

    /// <summary>
    /// Escala o retângulo para cima
    /// </summary>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public Box ScaleTop(FixedSingle alpha)
    {
        FixedSingle height = Height;
        return (LeftTop + alpha * (height - 1) * Vector.UP_VECTOR, Vector.NULL_VECTOR, (Width, alpha * height));
    }

    public Box ClipTop(FixedSingle clip)
    {
        return (Origin, (Mins.X, Mins.Y + clip), Maxs);
    }

    /// <summary>
    /// Escala o retângulo para baixo
    /// </summary>
    /// <param name="alpha"></param>
    /// <returns></returns>
    public Box ScaleBottom(FixedSingle alpha)
    {
        return new(LeftTop, Vector.NULL_VECTOR, new Vector(Width, alpha * Height));
    }

    public Box ClipBottom(FixedSingle clip)
    {
        return new(Origin, Mins, new Vector(Maxs.X, Maxs.Y - clip));
    }

    public Box Mirror()
    {
        return Mirror(Origin.X);
    }

    public Box Mirror(Vector origin)
    {
        return Mirror(origin.X);
    }

    public Box Mirror(FixedSingle x)
    {
        FixedSingle originX = Origin.X;
        FixedSingle minsX = originX + Mins.X;
        FixedSingle maxsX = originX + Maxs.X;

        x *= 2;
        FixedSingle newOriginX = x - originX;
        FixedSingle newMinsX = x - minsX;
        FixedSingle newMaxsX = x - maxsX;

        return new Box((newOriginX, Origin.Y), new Vector(newMaxsX - newOriginX, Mins.Y), new Vector(newMinsX - newOriginX, Maxs.Y));
    }

    public Box Flip()
    {
        return Flip(Origin.Y);
    }

    public Box Flip(Vector origin)
    {
        return Flip(origin.Y);
    }

    public Box Flip(FixedSingle y)
    {
        FixedSingle originY = Origin.Y;
        FixedSingle minsY = originY + Mins.Y;
        FixedSingle maxsY = originY + Maxs.Y;

        y *= 2;
        FixedSingle newOriginY = y - originY;
        FixedSingle newMinsY = y - minsY;
        FixedSingle newMaxsY = y - maxsY;

        return new Box((Origin.X, newOriginY), new Vector(Mins.X, newMaxsY - newOriginY), new Vector(Maxs.X, newMinsY - newOriginY));
    }

    public Vector GetNormal(BoxSide side)
    {
        return side switch
        {
            BoxSide.LEFT => Vector.RIGHT_VECTOR,
            BoxSide.TOP => Vector.DOWN_VECTOR,
            BoxSide.RIGHT => Vector.LEFT_VECTOR,
            BoxSide.BOTTOM => Vector.UP_VECTOR,
            _ => Vector.NULL_VECTOR,
        };
    }

    public LineSegment GetSideSegment(BoxSide side)
    {
        return side switch
        {
            BoxSide.LEFT => new LineSegment(LeftTop, LeftBottom),
            BoxSide.TOP => new LineSegment(LeftTop, RightTop),
            BoxSide.RIGHT => new LineSegment(RightTop, RightBottom),
            BoxSide.BOTTOM => new LineSegment(LeftBottom, RightBottom),
            _ => LineSegment.NULL_SEGMENT,
        };
    }

    public Box HalfLeft()
    {
        return new(Origin, Mins, new Vector((Mins.X + Maxs.X) * FixedSingle.HALF, Maxs.Y));
    }

    public Box HalfTop()
    {
        return new(Origin, Mins, new Vector(Maxs.X, (Mins.Y + Maxs.Y) * FixedSingle.HALF));
    }

    public Box HalfRight()
    {
        return new(Origin, new Vector((Mins.X + Maxs.X) * FixedSingle.HALF, Mins.Y), Maxs);
    }

    public Box HalfBottom()
    {
        return new(Origin, new Vector(Mins.X, (Mins.Y + Maxs.Y) * FixedSingle.HALF), Maxs);
    }

    public bool IsValid()
    {
        return Width > 0 && Height > 0;
    }

    public Box Scale(Vector origin, FixedSingle scaleX, FixedSingle scaleY)
    {
        return new((Origin - origin).Scale(scaleX, scaleY) + origin, Mins.Scale(scaleX, scaleY), Maxs.Scale(scaleX, scaleY));
    }

    public Box Scale(Vector origin, FixedSingle scale)
    {
        return Scale(origin, scale, scale);
    }

    public Box Scale(FixedSingle scaleX, FixedSingle scaleY)
    {
        return Scale(Vector.NULL_VECTOR, scaleX, scaleY);
    }

    public Box Scale(FixedSingle scale)
    {
        return Scale(Vector.NULL_VECTOR, scale, scale);
    }

    public Box ScaleInverse(Vector origin, FixedSingle divisorX, FixedSingle divisorY)
    {
        return new((Origin - origin).ScaleInverse(divisorX, divisorY) + origin, Mins.ScaleInverse(divisorX, divisorY), Maxs.ScaleInverse(divisorX, divisorY));
    }

    public Box ScaleInverse(Vector origin, FixedSingle divisor)
    {
        return ScaleInverse(origin, divisor, divisor);
    }

    public Box ScaleInverse(FixedSingle divisorX, FixedSingle divisorY)
    {
        return ScaleInverse(Vector.NULL_VECTOR, divisorX, divisorY);
    }

    public Box ScaleInverse(FixedSingle divisor)
    {
        return ScaleInverse(Vector.NULL_VECTOR, divisor, divisor);
    }

    public Box RestrictIn(Box box)
    {
        FixedSingle x = box.Origin.X;
        FixedSingle y = box.Origin.Y;

        FixedSingle minX = box.Left;
        FixedSingle limitLeft = Left;
        if (minX < limitLeft)
        {
            minX = limitLeft;
            x = minX - box.Mins.X;
        }

        FixedSingle minY = box.Top;
        FixedSingle limitTop = Top;
        if (minY < limitTop)
        {
            minY = limitTop;
            y = minY - box.Mins.Y;
        }

        FixedSingle maxX = box.Right;
        FixedSingle limitRight = Right;
        if (maxX > limitRight)
        {
            maxX = limitRight;
            x = maxX - box.Maxs.X;
        }

        FixedSingle maxY = box.Bottom;
        FixedSingle limitBottom = Bottom;
        if (maxY > limitBottom)
        {
            maxY = limitBottom;
            y = maxY - box.Maxs.Y;
        }

        return ((x, y), box.Mins, box.Maxs);
    }

    public Box Union(Box other)
    {
        var lt1 = LeftTop;
        var rb1 = RightBottom;

        var lt2 = other.LeftTop;
        var rb2 = other.RightBottom;

        var left = FixedSingle.Min(lt1.X, lt2.X);
        var right = FixedSingle.Max(rb1.X, rb2.X);

        var top = FixedSingle.Min(lt1.Y, lt2.Y);
        var bottom = FixedSingle.Max(rb1.Y, rb2.Y);

        return new Box((left, top), (right - left).Abs, (bottom - top).Abs);
    }
    public Box Intersection(Box other)
    {
        var lt1 = LeftTop;
        var rb1 = RightBottom;

        var lt2 = other.LeftTop;
        var rb2 = other.RightBottom;

        var left = FixedSingle.Max(lt1.X, lt2.X);
        var right = FixedSingle.Min(rb1.X, rb2.X);

        if (right < left)
            return EMPTY_BOX;

        var top = FixedSingle.Max(lt1.Y, lt2.Y);
        var bottom = FixedSingle.Min(rb1.Y, rb2.Y);

        return bottom < top ? EMPTY_BOX : new Box(new Vector(left, top), Vector.NULL_VECTOR, new Vector(right - left, bottom - top));
    }
    public GeometryType Intersection(LineSegment line, out LineSegment result)
    {
        bool startInside = Contains(line.Start, BoxSide.BORDERS | BoxSide.INNER);
        bool endInside = Contains(line.End, BoxSide.BORDERS | BoxSide.INNER);
        if (startInside)
        {
            if (endInside)
            {
                result = line;
                return GeometryType.LINE_SEGMENT;
            }

            for (int i = 0; i < 4; i++)
            {
                var kind = (BoxSide) (1 << i);
                var sideLine = GetSideSegment(kind);
                var type = sideLine.Intersection(line, out LineSegment intersection);

                if (type == GeometryType.VECTOR && intersection.Start != line.Start)
                {
                    result = new LineSegment(line.Start, intersection.Start);
                    return GeometryType.LINE_SEGMENT;
                }
            }

            result = line;
            return GeometryType.VECTOR;
        }

        if (endInside)
        {
            for (int i = 0; i < 4; i++)
            {
                var kind = (BoxSide) (1 << i);
                var sideLine = GetSideSegment(kind);
                var type = sideLine.Intersection(line, out LineSegment intersection);

                if (type == GeometryType.VECTOR && intersection.Start != line.End)
                {
                    result = new LineSegment(intersection.Start, line.End);
                    return GeometryType.LINE_SEGMENT;
                }
            }

            result = line;
            return GeometryType.VECTOR;
        }

        int founds = 0;
        Vector start = Vector.NULL_VECTOR;
        Vector end = Vector.NULL_VECTOR;

        for (int i = 0; i < 4; i++)
        {
            var kind = (BoxSide) (1 << i);
            var sideLine = GetSideSegment(kind);
            var type = sideLine.Intersection(line, out LineSegment intersection);

            if (type == GeometryType.VECTOR)
            {
                if (founds == 0)
                    start = intersection.Start;
                else if (founds == 1)
                    end = intersection.Start;

                founds++;
            }

            if (founds == 2)
            {
                result = new LineSegment(start, end);
                return GeometryType.LINE_SEGMENT;
            }
        }

        if (founds == 1)
        {
            result = new LineSegment(start, start);
            return GeometryType.VECTOR;
        }

        result = LineSegment.NULL_SEGMENT;
        return GeometryType.EMPTY;
    }

    public BoxSide GetSidePosition(Vector point)
    {
        var code = BoxSide.NONE;

        var x = point.X;
        var y = point.Y;

        if (x < Left)
            code |= BoxSide.OUTER | BoxSide.LEFT;
        else if (x == Left)
            code |= BoxSide.LEFT;
        else if (x == Right)
            code |= BoxSide.RIGHT;
        else if (x > Right)
            code |= BoxSide.OUTER | BoxSide.RIGHT;

        if (y < Top)
            code |= BoxSide.OUTER | BoxSide.TOP;
        else if (y == Top)
            code |= BoxSide.TOP;
        else if (y == Bottom)
            code |= BoxSide.BOTTOM;
        else if (y > Bottom)
            code |= BoxSide.OUTER | BoxSide.BOTTOM;

        if (code == BoxSide.NONE)
            code = BoxSide.INNER;

        return code;
    }

    public bool Contains(Vector point, BoxSide include)
    {
        var m = Origin + Mins;
        var M = Origin + Maxs;

        if (!include.HasFlag(BoxSide.INNER))
        {
            return include.HasFlag(BoxSide.LEFT) && LeftSegment.Contains(point)
                || include.HasFlag(BoxSide.TOP) && TopSegment.Contains(point)
                || include.HasFlag(BoxSide.RIGHT) && RightSegment.Contains(point)
                || include.HasFlag(BoxSide.BOTTOM) && BottomSegment.Contains(point);
        }

        var interval = Interval.MakeInterval((m.X, include.HasFlag(BoxSide.LEFT)), (M.X, include.HasFlag(BoxSide.RIGHT)));

        if (!interval.Contains(point.X))
            return false;

        interval = Interval.MakeInterval((m.Y, include.HasFlag(BoxSide.TOP)), (M.Y, include.HasFlag(BoxSide.BOTTOM)));

        return interval.Contains(point.Y);
    }

    public bool Contains(Vector point)
    {
        return Contains(point, BoxSide.LEFT_TOP | BoxSide.INNER);
    }

    public bool HasIntersectionWith(LineSegment line)
    {
        var type = Intersection(line, out line);
        return type != GeometryType.EMPTY;
    }

    public bool IsOverlaping(Box other, BoxSide includeSides = BoxSide.LEFT_TOP | BoxSide.INNER, BoxSide includeOtherBoxSides = BoxSide.LEFT_TOP | BoxSide.INNER)
    {
        var m1 = Origin + Mins;
        var M1 = Origin + Maxs;

        var m2 = other.Origin + other.Mins;
        var M2 = other.Origin + other.Maxs;

        var interval1 = Interval.MakeInterval((m1.X, includeSides.HasFlag(BoxSide.LEFT)), (M1.X, includeSides.HasFlag(BoxSide.RIGHT)));
        var interval2 = Interval.MakeInterval((m2.X, includeOtherBoxSides.HasFlag(BoxSide.LEFT)), (M2.X, includeOtherBoxSides.HasFlag(BoxSide.RIGHT)));

        if (!interval1.IsOverlaping(interval2))
            return false;

        interval1 = Interval.MakeInterval((m1.Y, includeSides.HasFlag(BoxSide.TOP)), (M1.Y, includeSides.HasFlag(BoxSide.BOTTOM)));
        interval2 = Interval.MakeInterval((m2.Y, includeOtherBoxSides.HasFlag(BoxSide.TOP)), (M2.Y, includeOtherBoxSides.HasFlag(BoxSide.BOTTOM)));

        return interval1.IsOverlaping(interval2);
    }

    public bool HasIntersectionWith(IGeometry geometry)
    {
        return (IGeometry) this == geometry
            || geometry switch
            {
                Vector v => Contains(v),
                Box box => IsOverlaping(box),
                LineSegment line => HasIntersectionWith(line),
                RightTriangle triangle => triangle.HasIntersectionWith(this),
                _ => throw new NotImplementedException()
            };
    }

    /// <summary>
    /// Translata um retângulo na direção de um vetor
    /// </summary>
    /// <param name="box">Retângulo</param>
    /// <param name="vec">Vetor</param>
    /// <returns>Retângulo box translatado na direção de vec</returns>
    public static Box operator +(Box box, Vector vec)
    {
        return new(box.Origin + vec, box.Mins, box.Maxs);
    }

    /// <summary>
    /// Translata um retângulo na direção de um vetor
    /// </summary>
    /// <param name="vec">Vetor</param>
    /// /// <param name="box">Retângulo</param>
    /// <returns>Retângulo box translatado na direção de vec</returns>

    public static Box operator +(Vector vec, Box box)
    {
        return new(box.Origin + vec, box.Mins, box.Maxs);
    }

    /// <summary>
    /// Translata um retângulo na direção oposta de um vetor
    /// </summary>
    /// <param name="box">Retângulo</param>
    /// <param name="vec">Vetor</param>
    /// <returns>Retângulo box translatado na direção oposta de vec</returns>
    public static Box operator -(Box box, Vector vec)
    {
        return new(box.Origin - vec, box.Mins, box.Maxs);
    }

    /// <summary>
    /// Escala um retângulo
    /// </summary>
    /// <param name="factor">Fator de escala</param>
    /// <param name="box">Retângulo</param>
    /// <returns>Retângulo com suas coordenadas e dimensões escaladas por factor</returns>
    public static Box operator *(FixedSingle factor, Box box)
    {
        Vector m = box.Origin + box.Mins;
        return new Box(m * factor, Vector.NULL_VECTOR, box.DiagonalVector * factor);
    }

    /// <summary>
    /// Escala um retângulo
    /// </summary>
    /// <param name="box">Retângulo</param>
    /// /// <param name="factor">Fator de escala</param>
    /// <returns>Retângulo com suas coordenadas e dimensões escaladas por factor</returns>
    public static Box operator *(Box box, FixedSingle factor)
    {
        Vector m = box.Origin + box.Mins;
        return new Box(m * factor, Vector.NULL_VECTOR, box.DiagonalVector * factor);
    }

    /// <summary>
    /// Escala um retângulo inversamente (escala pelo inverso do divisor)
    /// </summary>
    /// <param name="box">Retângulo</param>
    /// <param name="divisor">Divisor</param>
    /// <returns>Retângulo com suas coordenadas e dimensões divididas por divisor</returns>
    public static Box operator /(Box box, FixedSingle divisor)
    {
        Vector m = box.Origin + box.Mins;
        return new Box(m / divisor, Vector.NULL_VECTOR, box.DiagonalVector / divisor);
    }

    /// <summary>
    /// Faz a união entre dois retângulos que resultará no menor retângulo que contém os retângulos dados
    /// </summary>
    /// <param name="box1">Primeiro retângulo</param>
    /// <param name="box2">Segundo retângulo</param>
    /// <returns>Menor retângulo que contém os dois retângulos dados</returns>
    public static Box operator |(Box box1, Box box2)
    {
        return box1.Union(box2);
    }

    /// <summary>
    /// Faz a intersecção de dois retângulos que resultará em um novo retângulo que esteja contido nos dois retângulos dados. Se os dois retângulos dados forem disjuntos então o resultado será um vetor nulo.
    /// </summary>
    /// <param name="box1">Primeiro retângulo</param>
    /// <param name="box2">Segundo retângulo</param>
    /// <returns>Interesecção entre os dois retângulos dados ou um vetor nulo caso a intersecção seja um conjunto vazio</returns>
    public static Box operator &(Box box1, Box box2)
    {
        return box1.Intersection(box2);
    }

    public static LineSegment operator &(Box box, LineSegment line)
    {
        box.Intersection(line, out line);
        return line;
    }

    public static LineSegment operator &(LineSegment line, Box box)
    {
        return box & line;
    }

    /// <summary>
    /// Verifica se um vetor está contido no interior de um retângulo
    /// </summary>
    /// <param name="vec">Vetor</param>
    /// <param name="box">Retângulo</param>
    /// <returns>true se vec estiver contido no interior box, false caso contrário</returns>
    public static bool operator <(Vector vec, Box box)
    {
        Vector m = box.Origin + box.Mins;
        Vector M = box.Origin + box.Maxs;

        var interval = Interval.MakeOpenInterval(m.X, M.X);
        if (!interval.Contains(vec.X))
            return false;

        interval = Interval.MakeOpenInterval(m.Y, M.Y);
        return interval.Contains(vec.Y);
    }

    /// <summary>
    /// Verifica se um vetor está contido no de um retângulo
    /// </summary>
    /// <param name="vec">Vetor</param>
    /// <param name="box">Retângulo</param>
    /// <returns>true se vec estiver contido no exterior de box, false caso contrário</returns>
    public static bool operator >(Vector vec, Box box)
    {
        return !(vec <= box);
    }

    /// <summary>
    /// Verifica se um retâgulo contém um vetor em seu exterior
    /// </summary>
    /// <param name="box">Retângulo</param>
    /// <param name="vec">Vetor</param>
    /// <returns>true se box contém vec em seu exterior, false caso contrário</returns>
    public static bool operator <(Box box, Vector vec)
    {
        return !(box >= vec);
    }

    /// <summary>
    /// Verifica um retângulo contém um vetor em seu interior
    /// </summary>
    /// <param name="box">Retângulo</param>
    /// /// <param name="vec">Vetor</param>
    /// <returns>true box contém vec em seu interior, false caso contrário</returns>
    public static bool operator >(Box box, Vector vec)
    {
        return vec < box;
    }

    /// <summary>
    /// Verifica se um vetor está contido no interior ou na borda de um retângulo
    /// </summary>
    /// <param name="vec">Vetor</param>
    /// <param name="box">Retângulo</param>
    /// <returns>true se vec estiver contido no interior ou na borda de box, false caso contrário</returns>
    public static bool operator <=(Vector vec, Box box)
    {
        return box.Contains(vec);
    }

    /// <summary>
    /// Verifica se um vetor está contido no exterior ou na borda de um retângulo
    /// </summary>
    /// <param name="vec">Vetor</param>
    /// <param name="box">Retângulo</param>
    /// <returns>true se vec estiver contido no exterior ou na borda de box, false caso contrário</returns>
    public static bool operator >=(Vector vec, Box box)
    {
        return !(vec < box);
    }

    /// <summary>
    /// Verifica se um retângulo contém um vetor em seu exterior ou em sua borda
    /// </summary>
    /// <param name="box">Retângulo</param>
    /// <param name="vec">Vetor</param>
    /// <returns>true se box contém vec em seu exterior ou em sua borda, false caso contrário</returns>
    public static bool operator <=(Box box, Vector vec)
    {
        return !(box > vec);
    }

    /// <summary>
    /// Verifica um retângulo contém um vetor em seu interior ou emsua borda
    /// </summary>
    /// <param name="box">Retângulo</param>
    /// /// <param name="vec">Vetor</param>
    /// <returns>true box contém vec em seu interior ou em sua borda, false caso contrário</returns>
    public static bool operator >=(Box box, Vector vec)
    {
        return box.Contains(vec);
    }

    /// <summary>
    /// Veririca se um retângulo está contido em outro retângulo
    /// </summary>
    /// <param name="box1">Primeiro retângulo</param>
    /// <param name="box2">Segundo retângulo</param>
    /// <returns>true se box1 está contido em box2, falso caso contrário</returns>
    public static bool operator <=(Box box1, Box box2)
    {
        return (box1 & box2) == box1;
    }

    /// <summary>
    /// Verifica se um retângulo contém outro retângulo
    /// </summary>
    /// <param name="box1">Primeiro retângulo</param>
    /// <param name="box2">Segundo retângulo</param>
    /// <returns>true se box1 contém box2, false caso contrário</returns>
    public static bool operator >=(Box box1, Box box2)
    {
        return (box2 & box1) == box2;
    }

    /// <summary>
    /// Veririca se um retângulo está inteiramente contido em outro retângulo
    /// </summary>
    /// <param name="box1">Primeiro retângulo</param>
    /// <param name="box2">Segundo retângulo</param>
    /// <returns>true se box1 está inteiramente contido em box2 (ou seja box1 está em box2 mas box1 não é igual a box2), falso caso contrário</returns>
    public static bool operator <(Box box1, Box box2)
    {
        return box1 <= box2 && box1 != box2;
    }

    /// <summary>
    /// Verifica se um retângulo contém inteiramente outro retângulo
    /// </summary>
    /// <param name="box1">Primeiro retângulo</param>
    /// <param name="box2">Segundo retângulo</param>
    /// <returns>true se box1 contém inteiramente box2 (ou seja, box1 contém box2 mas box1 não é igual a box2), false caso contrário</returns>
    public static bool operator >(Box box1, Box box2)
    {
        return box2 <= box1 && box1 != box2;
    }

    /// <summary>
    /// Verifica se dois retângulos são iguais
    /// </summary>
    /// <param name="box1">Primeiro retângulo</param>
    /// <param name="box2">Segundo retângulo</param>
    /// <returns>true se forem iguais, false caso contrário</returns>
    public static bool operator ==(Box box1, Box box2)
    {
        Vector m1 = box1.Origin + box1.Mins;
        Vector m2 = box2.Origin + box2.Mins;

        if (m1 != m2)
            return false;

        Vector M1 = box1.Origin + box1.Maxs;
        Vector M2 = box2.Origin + box2.Maxs;

        return M1 == M2;
    }

    /// <summary>
    /// Verifica se dois retângulos são diferentes
    /// </summary>
    /// <param name="box1">Primeiro retângulo</param>
    /// <param name="box2">Segundo retângulo</param>
    /// <returns>true se forem diferentes, false caso contrário</returns>
    public static bool operator !=(Box box1, Box box2)
    {
        return !(box1 == box2);
    }

    public static implicit operator Box((Vector, Vector, Vector) tuple)
    {
        return new(tuple.Item1, tuple.Item2, tuple.Item3);
    }

    public static implicit operator Box((FixedSingle, FixedSingle, FixedSingle, FixedSingle) tuple)
    {
        return new(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
    }

    public static implicit operator Box((FixedSingle, FixedSingle, FixedSingle, FixedSingle, FixedSingle, FixedSingle) tuple)
    {
        return new(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6);
    }

    public static implicit operator Box((Vector, Vector) tuple)
    {
        return new(tuple.Item1, tuple.Item2);
    }

    public static implicit operator (Vector, Vector, Vector)(Box box)
    {
        return (box.Origin, box.Mins, box.Maxs);
    }

    public static implicit operator (FixedSingle, FixedSingle, FixedSingle, FixedSingle)(Box box)
    {
        return (box.Left, box.Top, box.Width, box.Height);
    }

    public static implicit operator (FixedSingle, FixedSingle, FixedSingle, FixedSingle, FixedSingle, FixedSingle)(Box box)
    {
        return (box.Origin.X, box.Origin.Y, box.Left, box.Top, box.Width, box.Height);
    }

    public static implicit operator (Vector, Vector)(Box box)
    {
        return (box.Origin + box.Mins, box.Origin + box.Maxs);
    }

    public void Deconstruct(out Vector leftTop, out Vector rightBottom)
    {
        leftTop = LeftTop;
        rightBottom = RightBottom;
    }

    public void Deconstruct(out Vector origin, out Vector mins, out Vector maxs)
    {
        origin = Origin;
        mins = Mins;
        maxs = Maxs;
    }

    public void Deconstruct(out FixedSingle left, out FixedSingle top, out FixedSingle right, out FixedSingle bottom)
    {
        left = Left;
        top = Top;
        right = Right;
        bottom = Bottom;
    }

    public void Deconstruct(out FixedSingle x, out FixedSingle y, out FixedSingle left, out FixedSingle top, out FixedSingle right, out FixedSingle bottom)
    {
        x = Origin.X;
        y = Origin.Y;
        left = Left;
        top = Top;
        right = Right;
        bottom = Bottom;
    }
}