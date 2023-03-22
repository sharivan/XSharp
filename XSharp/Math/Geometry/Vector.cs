using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

using XSharp.Serialization;
using XSharp.Util;

using TupleExtensions = XSharp.Util.TupleExtensions;

namespace XSharp.Math.Geometry;

public class VectorTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        Type genericSourceType;
        return sourceType == typeof(Vector)
            || sourceType.IsGenericType && (
                ((genericSourceType = sourceType.GetGenericTypeDefinition()) == typeof(ValueTuple<,>)
                || genericSourceType == typeof(Tuple<,>)
            )
            ? Util.TupleExtensions.CanConvertTupleToArray<FixedSingle>(sourceType)
            : base.CanConvertFrom(context, sourceType));
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        Type sourceType = value.GetType();
        if (sourceType == typeof(Vector))
            return value;

        if (sourceType.IsGenericType)
        {
            var genericSourceType = sourceType.GetGenericTypeDefinition();
            if (genericSourceType == typeof(ValueTuple<,>) || genericSourceType == typeof(Tuple<,>))
            {
                var args = ((ITuple) value).ToArray<FixedSingle>();
                return new Vector(args[0], args[1]);
            }
        }

        return base.ConvertFrom(context, culture, value);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        var vec = (Vector) value;
        if (destinationType == typeof(Vector))
            return vec;

        if (destinationType.IsGenericType)
        {
            var genericDestinationType = destinationType.GetGenericTypeDefinition();
            if (genericDestinationType == typeof(ValueTuple<,>) || genericDestinationType == typeof(Tuple<,>))
                return TupleExtensions.ArrayToTuple(destinationType, vec.X, vec.Y);
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}

/// <summary>
/// Vetor bidimensional
/// </summary>
[TypeConverter(typeof(VectorTypeConverter))]
public struct Vector : IGeometry, ISerializable
{
    public const GeometryType type = GeometryType.VECTOR;

    /// <summary>
    /// Vetor nulo
    /// </summary>
    public static readonly Vector NULL_VECTOR = new(0, 0); // Vetor nulo
    /// <summary>
    /// Vetor leste
    /// </summary>
    public static readonly Vector LEFT_VECTOR = new(-1, 0);
    /// <summary>
    /// Vetor norte
    /// </summary>
    public static readonly Vector UP_VECTOR = new(0, -1);
    /// <summary>
    /// Vetor oeste
    /// </summary>
    public static readonly Vector RIGHT_VECTOR = new(1, 0);
    /// <summary>
    /// Vetor sul
    /// </summary>
    public static readonly Vector DOWN_VECTOR = new(0, 1);

    public GeometryType Type => type;

    /// <summary>
    /// Coordenada x do vetor
    /// </summary>
    public FixedSingle X
    {
        get;
        private set;
    }

    /// <summary>
    /// Coordenada y do vetor
    /// </summary>
    public FixedSingle Y
    {
        get;
        private set;
    }

    /// <summary>
    /// Módulo/Norma/Comprimento do vetor
    /// </summary>
    public FixedSingle Length => GetLength();

    /// <summary>
    /// Retorna true se este vetor for nulo, false caso contrário
    /// </summary>
    public bool IsNull => X == 0 && Y == 0;

    public Vector XVector => new(X, 0);

    public Vector YVector => new(0, Y);

    /// <summary>
    /// Cria um vetor a partir de duas coordenadas
    /// </summary>
    /// <param name="x">Coordenada x</param>
    /// <param name="y">Coordenada y</param>
    public Vector(FixedSingle x, FixedSingle y)
    {
        X = x;
        Y = y;
    }

    public Vector((FixedSingle, FixedSingle) tuple) : this(tuple.Item1, tuple.Item2) { }

    public Vector(BinarySerializer reader)
    {
        Deserialize(reader);
    }

    public void Deserialize(BinarySerializer reader)
    {
        X = reader.ReadFixedSingle();
        Y = reader.ReadFixedSingle();
    }

    public void Serialize(BinarySerializer writer)
    {
        X.Serialize(writer);
        Y.Serialize(writer);
    }

    public override int GetHashCode()
    {
        return (X.GetHashCode() << 16) + Y.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is not Vector)
            return false;

        var other = (Vector) obj;
        return this == other;
    }

    public override string ToString()
    {
        return "(" + X + ", " + Y + ")";
    }

    public Vector Versor()
    {
        if (IsNull)
            return NULL_VECTOR;

        FixedSingle abs = Length;
        return new Vector(X / abs, Y / abs);
    }

    public Vector Versor(Metric metric)
    {
        if (IsNull)
            return NULL_VECTOR;

        FixedSingle abs = GetLength(metric);
        return new Vector(X / abs, Y / abs);
    }

    public Vector VersorScale(FixedSingle scale)
    {
        if (IsNull)
            return NULL_VECTOR;

        FixedSingle abs = Length;
        return new Vector((FixedSingle) ((FixedDouble) scale * X / abs), (FixedSingle) ((FixedDouble) scale * Y / abs));
    }

    public Vector VersorScale(FixedSingle scale, Metric metric)
    {
        if (IsNull)
            return NULL_VECTOR;

        FixedSingle abs = GetLength(metric);
        return new Vector((FixedSingle) ((FixedDouble) scale * X / abs), (FixedSingle) ((FixedDouble) scale * Y / abs));
    }

    /// <summary>
    /// Rotaciona o vetor ao redor da origem
    /// </summary>
    /// <param name="angle">Angulo de rotação em radianos</param>
    /// <returns>O vetor rotacionado</returns>
    public Vector Rotate(FixedSingle angle)
    {
        FixedDouble x = X;
        FixedDouble y = Y;
        var a = (FixedDouble) angle;

        FixedDouble cos = a.Cos();
        FixedDouble sin = a.Sin();

        return new Vector((FixedSingle) (x * cos - y * sin), (FixedSingle) (x * sin + y * cos));
    }

    /// <summary>
    /// Rotaciona o vetor ao redor de um outro vetor
    /// </summary>
    /// <param name="center">Centro de rotação</param>
    /// <param name="angle">Angulo de rotação em radianos</param>
    /// <returns>O vetor rotacionado</returns>
    public Vector Rotate(Vector center, FixedSingle angle)
    {
        return (this - center).Rotate(angle) + center;
    }

    /// <summary>
    /// Rotaciona um vetor em 90 graus ao redor da origem
    /// </summary>
    /// <returns>O vetor rotacionado</returns>
    public Vector Rotate90()
    {
        return new(-Y, X);
    }

    /// <summary>
    /// Rotaciona um vetor em 90 graus ao redor de outro vetor
    /// </summary>
    /// <param name="center">Centro de rotação</param>
    /// <returns>O vetor rotacionado</returns>
    public Vector Rotate90(Vector center)
    {
        return (this - center).Rotate90() + center;
    }

    /// <summary>
    /// Rotaciona um vetor em 180 graus ao redor da origem
    /// </summary>
    /// <returns>O vetor rotacionado</returns>
    public Vector Rotate180()
    {
        return new(-X, -Y);
    }

    /// <summary>
    /// Rotaciona um vetor em 180 graus ao redor de outro vetor
    /// </summary>
    /// <param name="center">Centro de rotação</param>
    /// <returns>O vetor rotacionado</returns>
    public Vector Rotate180(Vector center)
    {
        return (this - center).Rotate180() + center;
    }

    /// <summary>
    /// Rotaciona um vetor em 270 graus ao redor da origem
    /// </summary>
    /// <returns>O vetor rotacionado</returns>
    public Vector Rotate270()
    {
        return new(Y, -X);
    }

    /// <summary>
    /// Rotaciona um vetor em 270 graus ao redor de outro vetor
    /// </summary>
    /// <param name="center">Centro de rotação</param>
    /// <returns>O vetor rotacionado</returns>
    public Vector Rotate270(Vector center)
    {
        return (this - center).Rotate270() + center;
    }

    public Vector Truncate()
    {
        return new(X.IntValue, Y.IntValue);
    }

    public Vector TruncateX()
    {
        return new(X.IntValue, Y);
    }

    public Vector TruncateY()
    {
        return new(X, Y.IntValue);
    }

    public Vector RoundToFloor()
    {
        return new(X.Floor(), Y.Floor());
    }

    public Vector RoundXToFloor()
    {
        return new(X.Floor(), Y);
    }

    public Vector RoundYToFloor()
    {
        return new(X, Y.Floor());
    }

    public Vector RoundToCeil()
    {
        return new(X.Ceil(), Y.Ceil());
    }

    public Vector RoundXToCeil()
    {
        return new(X.Ceil(), Y);
    }

    public Vector RoundYToCeil()
    {
        return new(X, Y.Ceil());
    }

    public Vector RoundToNearest()
    {
        return new(X.Round(), Y.Round());
    }

    public Vector RoundXToNearest()
    {
        return new(X.Round(), Y);
    }

    public Vector RoundYToNearest()
    {
        return new(X, Y.Round());
    }

    public Vector Round(RoundMode roundXMode = RoundMode.FLOOR, RoundMode roundYMode = RoundMode.FLOOR)
    {
        return (X.Round(roundXMode), Y.Round(roundYMode));
    }

    public Vector RoundX()
    {
        return new(X.Round(), Y);
    }

    public Vector RoundX(RoundMode mode)
    {
        return (X.Round(mode), Y);
    }

    public Vector RoundY()
    {
        return new(X, Y.Round());
    }

    public Vector RoundY(RoundMode mode)
    {
        return (X, Y.Round(mode));
    }

    /// <summary>
    /// Distâcia entre vetores
    /// </summary>
    /// <param name="vec">Vetor no qual será medido a sua distância até este vetor</param>
    /// <returns>A distância entre este vetor e o vetor dado</returns>
    public FixedSingle DistanceTo(Vector vec)
    {
        FixedDouble dx = X - vec.X;
        FixedDouble dy = Y - vec.Y;
        return System.Math.Sqrt(dx * dx + dy * dy);
    }

    public FixedSingle DistanceTo(Vector vec, Metric metric)
    {
        FixedSingle dx = (X - vec.X).Abs;
        FixedSingle dy = (Y - vec.Y).Abs;

        return metric switch
        {
            Metric.EUCLIDIAN => (FixedSingle) System.Math.Sqrt((FixedDouble) dx * dx + (FixedDouble) dy * dy),
            Metric.SUM => dx + dy,
            Metric.MAX => FixedSingle.Max(dx, dy),
            Metric.DISCRETE => (FixedSingle) (dx == dy ? 1 : 0),
            _ => FixedSingle.ZERO,
        };
    }

    public FixedSingle GetLength()
    {
        if (X == 0)
            return Y.Abs;

        if (Y == 0)
            return X.Abs;

        FixedDouble x = X;
        FixedDouble y = Y;
        return System.Math.Sqrt(x * x + y * y);
    }

    public FixedDouble GetLengthSquare()
    {
        if (X == 0)
            return (FixedDouble) Y * (FixedDouble) Y;

        if (Y == 0)
            return (FixedDouble) X * (FixedDouble) X;

        FixedDouble x = X;
        FixedDouble y = Y;
        return x * x + y * y;
    }

    public FixedSingle GetLength(Metric metric)
    {
        return metric == Metric.EUCLIDIAN ? GetLength() : DistanceTo(NULL_VECTOR, metric);
    }

    public Vector Scale(FixedSingle scaleX, FixedSingle scaleY)
    {
        return new(scaleX * X, scaleY * Y);
    }

    public Vector Scale(FixedSingle scale)
    {
        return Scale(scale, scale);
    }

    public Vector ScaleInverse(FixedSingle scaleX, FixedSingle scaleY)
    {
        return new(X / scaleX, Y / scaleY);
    }

    public Vector ScaleInverse(FixedSingle divisor)
    {
        return ScaleInverse(divisor, divisor);
    }

    public Vector TruncFracPart(int bits = 8)
    {
        return (X.TruncFracPart(bits), Y.TruncFracPart(bits));
    }

    public bool Contains(Vector v)
    {
        return this == v;
    }

    public bool HasIntersectionWith(IGeometry geometry)
    {
        return (IGeometry) this == geometry
            || geometry switch
            {
                Vector v => Contains(v),
                Box box => box.Contains(this),
                LineSegment line => line.Contains(this),
                RightTriangle triangle => triangle.Contains(this),
                _ => throw new NotImplementedException()
            };
    }

    /// <summary>
    /// Adição de vetores
    /// </summary>
    /// <param name="vec1">Primeiro vetor</param>
    /// <param name="vec2">Segundo vetor</param>
    /// <returns>Soma entre os dois vetores</returns>
    public static Vector operator +(Vector vec1, Vector vec2)
    {
        return new(vec1.X + vec2.X, vec1.Y + vec2.Y);
    }

    /// <summary>
    /// Subtração de vetores
    /// </summary>
    /// <param name="vec1">Primeiro vetor</param>
    /// <param name="vec2">Segundo vetor</param>
    /// <returns>Diferença entre os dois vetores</returns>
    public static Vector operator -(Vector vec1, Vector vec2)
    {
        return new(vec1.X - vec2.X, vec1.Y - vec2.Y);
    }

    /// <summary>
    /// Inverte o sentido do vetor
    /// </summary>
    /// <param name="vec">Vetor</param>
    /// <returns>O oposto do vetor</returns>
    public static Vector operator -(Vector vec)
    {
        return new(-vec.X, -vec.Y);
    }

    /// <summary>
    /// Produto de um vetor por um escalar
    /// </summary>
    /// <param name="alpha">Escalar</param>
    /// <param name="vec">Vetor</param>
    /// <returns>O vetor escalado por alpha</returns>
    public static Vector operator *(FixedSingle alpha, Vector vec)
    {
        return new(alpha * vec.X, alpha * vec.Y);
    }

    /// <summary>
    /// Produto de um vetor por um escalar
    /// </summary>
    /// <param name="vec">Vetor</param>
    /// <param name="alpha">Escalar</param>
    /// <returns>O vetor escalado por alpha</returns>
    public static Vector operator *(Vector vec, FixedSingle alpha)
    {
        return new(alpha * vec.X, alpha * vec.Y);
    }

    /// <summary>
    /// Divisão de vetor por um escalar, o mesmo que multiplicar o vetor pelo inverso do escalar
    /// </summary>
    /// <param name="vec">Vetor</param>
    /// <param name="alpha">Escalar</param>
    /// <returns>O vetor dividido pelo escalar alpha</returns>
    public static Vector operator /(Vector vec, FixedSingle alpha)
    {
        return new(vec.X / alpha, vec.Y / alpha);
    }

    /// <summary>
    /// Produto escalar/interno/ponto entre dois vetores
    /// </summary>
    /// <param name="vec1">Primeiro vetor</param>
    /// <param name="vec2">Segundo vetor</param>
    /// <returns>Produto escalar entre os dois vetores</returns>
    public static FixedDouble operator *(Vector vec1, Vector vec2)
    {
        return (FixedDouble) vec1.X * (FixedDouble) vec2.X + (FixedDouble) vec1.Y * (FixedDouble) vec2.Y;
    }

    /// <summary>
    /// Igualdade entre vetores
    /// </summary>
    /// <param name="vec1">Primeiro vetor</param>
    /// <param name="vec2">Segundo vetor</param>
    /// <returns>true se os vetores forem iguais, false caso contrário</returns>
    public static bool operator ==(Vector vec1, Vector vec2)
    {
        return vec1.X == vec2.X && vec1.Y == vec2.Y;
    }

    /// <summary>
    /// Inequalidade entre vetores
    /// </summary>
    /// <param name="vec1">Primeiro vetor</param>
    /// <param name="vec2">Segundo vetor</param>
    /// <returns>true se os vetores forem diferentes, false caso contrário</returns>
    public static bool operator !=(Vector vec1, Vector vec2)
    {
        return vec1.X != vec2.X || vec1.Y != vec2.Y;
    }

    public static implicit operator (FixedSingle, FixedSingle)(Vector vec)
    {
        return (vec.X, vec.Y);
    }

    public static implicit operator Vector((FixedSingle, FixedSingle) tuple)
    {
        return new(tuple.Item1, tuple.Item2);
    }

    public void Deconstruct(out FixedSingle x, out FixedSingle y)
    {
        x = X;
        y = Y;
    }
}