using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using XSharp.Engine;
using XSharp.Math;

namespace XSharp.Geometry
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

    public enum SetOperation
    {
        UNION,
        INTERSECTION
    }

    public class GeometrySet : IGeometry
    {
        public const GeometryType type = GeometryType.SET;

        public static readonly GeometrySet EMPTY = new(SetOperation.UNION);

        public static readonly GeometrySet UNIVERSE = Complementary(EMPTY);

        public static GeometrySet Union(IGeometry a, IGeometry b)
        {
            return new GeometrySet(SetOperation.UNION, (a, false), (b, false));
        }

        public static GeometrySet Intersection(IGeometry a, IGeometry b)
        {
            return new GeometrySet(SetOperation.INTERSECTION, (a, false), (b, false));
        }

        public static GeometrySet Complementary(IGeometry a)
        {
            return new GeometrySet(SetOperation.UNION, (a, true));
        }

        public static GeometrySet Diference(IGeometry a, IGeometry b)
        {
            return new GeometrySet(SetOperation.INTERSECTION, (a, false), (b, true));
        }

        public static GeometrySet Diference(IGeometry a, IGeometry b, IGeometry c)
        {
            return new GeometrySet(SetOperation.INTERSECTION, (a, false), (b, true), (c, true));
        }

        public static void Split(IGeometry a, IGeometry b, out GeometrySet part1, out GeometrySet part2, out GeometrySet part3)
        {
            part1 = Diference(a, b);
            part2 = Intersection(a, b);
            part3 = Diference(b, a);
        }

        protected readonly List<(IGeometry part, bool negate)> parts;

        public GeometryType Type => type;

        public (IGeometry part, bool negate) this[int index]
        {
            get => parts[index];
            set => parts[index] = value;
        }

        public SetOperation Operation
        {
            get;
            set;
        }

        public FixedSingle Length => throw new NotImplementedException();

        public int Count => parts.Count;

        public IEnumerable<(IGeometry part, bool negate)> Parts => parts;

        public GeometrySet(SetOperation operation, params (IGeometry part, bool negate)[] parts)
        {
            Operation = operation;

            this.parts = new List<(IGeometry part, bool negate)>(parts);
        }

        public bool HasIntersectionWith(IGeometry geometry)
        {
            switch (Operation)
            {
                case SetOperation.UNION:
                    foreach (var (part, negate) in parts)
                        if (negate ? !part.HasIntersectionWith(geometry) : part.HasIntersectionWith(geometry))
                            return true;

                    return false;

                case SetOperation.INTERSECTION:
                    foreach (var (part, negate) in parts)
                        if (negate ? part.HasIntersectionWith(geometry) : !part.HasIntersectionWith(geometry))
                            return false;

                    return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is not GeometrySet)
            {
                return false;
            }

            var set = (GeometrySet) obj;
            return this == set;
        }

        public override int GetHashCode()
        {
            return -223833363 + EqualityComparer<List<(IGeometry part, bool negate)>>.Default.GetHashCode(parts);
        }

        public static bool operator ==(GeometrySet set1, GeometrySet set2)
        {
            var list = set2.parts.ToList();

            for (int i = 0; i < set1.parts.Count; i++)
            {
                var (part1, exclude1) = set1.parts[i];
                bool found = false;

                for (int j = 0; j < list.Count; j++)
                {
                    var (part2, exclude2) = list[j];

                    if (Equals(part1, part2) && exclude1 == exclude2)
                    {
                        found = true;
                        list.RemoveAt(j);
                        break;
                    }
                }

                if (!found)
                    return false;
            }

            return true;
        }

        public static bool operator !=(GeometrySet set1, GeometrySet set2)
        {
            return !(set1 == set2);
        }
    }

    public class VectorTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            var genericSourceType = sourceType.GetGenericTypeDefinition();
            return sourceType == typeof(Vector)
                || (genericSourceType == typeof(ValueTuple<,>) || genericSourceType == typeof(Tuple<,>)
                ? XSharpTupleExtensions.CanConvertTupleToArray<FixedSingle>(sourceType)
                : base.CanConvertFrom(context, sourceType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            Type sourceType = value.GetType();
            if (sourceType == typeof(Vector))
                return value;

            var genericSourceType = sourceType.GetGenericTypeDefinition();

            if (genericSourceType == typeof(ValueTuple<,>) || genericSourceType == typeof(Tuple<,>))
            {
                var args = ((ITuple) value).ToArray<FixedSingle>();
                return new Vector(args[0], args[1]);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var vec = (Vector) value;
            if (destinationType == typeof(Vector))
                return vec;

            var genericDestinationType = destinationType.GetGenericTypeDefinition();

            return genericDestinationType == typeof(ValueTuple<,>) || genericDestinationType == typeof(Tuple<,>)
                ? XSharpTupleExtensions.ArrayToTuple(destinationType, vec.X, vec.Y)
                : base.ConvertTo(context, culture, value, destinationType);
        }
    }

    /// <summary>
    /// Vetor bidimensional
    /// </summary>
    [TypeConverter(typeof(VectorTypeConverter))]
    public struct Vector : IGeometry
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
        }

        /// <summary>
        /// Coordenada y do vetor
        /// </summary>
        public FixedSingle Y
        {
            get;
        }

        /// <summary>
        /// Módulo/Norma/Comprimento do vetor
        /// </summary>
        public FixedSingle Length
        {
            get
            {
                if (X == 0)
                    return Y.Abs;

                if (Y == 0)
                    return X.Abs;

                FixedDouble x = X;
                FixedDouble y = Y;
                return System.Math.Sqrt(x * x + y * y);
            }
        }

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

        public Vector(BinaryReader reader)
        {
            X = new FixedSingle(reader);
            Y = new FixedSingle(reader);
        }

        public void Write(BinaryWriter writer)
        {
            X.Write(writer);
            Y.Write(writer);
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

        /// <summary>
        /// Normaliza o vetor
        /// </summary>
        /// <returns>O vetor normalizado</returns>
        public Vector Versor(FixedSingle epslon)
        {
            if (X.Abs <= epslon)
                return (0, Y.Abs <= epslon ? 0 : Y);

            if (Y.Abs <= epslon)
                return (X, 0);

            FixedDouble abs = Length;
            return new Vector((FixedSingle) ((FixedDouble) X / abs), (FixedSingle) ((FixedDouble) Y / abs));
        }

        public Vector Versor()
        {
            if (IsNull)
                return NULL_VECTOR;

            FixedSingle abs = Length;
            return new Vector(X / abs, Y / abs);
        }

        public Vector VersorScale(FixedSingle scale, FixedSingle epslon)
        {
            if (X.Abs <= epslon)
                return (0, Y.Abs <= epslon ? 0 : Y);

            if (Y.Abs <= epslon)
                return (X, 0);

            FixedSingle abs = Length;
            return new Vector((FixedSingle) ((FixedDouble) scale * X / abs), (FixedSingle) ((FixedDouble) scale * Y / abs));
        }

        public Vector VersorScale(FixedSingle scale)
        {
            if (IsNull)
                return NULL_VECTOR;

            FixedSingle abs = Length;
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

        public Vector RoundToCeil()
        {
            return new(X.Ceil(), Y.Ceil());
        }

        public Vector RoundToFloor()
        {
            return new(X.Floor(), Y.Floor());
        }

        public Vector RoundXToCeil()
        {
            return new(X.Ceil(), Y);
        }

        public Vector RoundXToFloor()
        {
            return new(X.Floor(), Y);
        }

        public Vector RoundYToCeil()
        {
            return new(X, Y.Ceil());
        }

        public Vector RoundYToFloor()
        {
            return new(X, Y.Floor());
        }

        public Vector Round()
        {
            return new(X.Round(), Y.Round());
        }

        public Vector RoundX()
        {
            return new(X.Round(), Y);
        }

        public Vector RoundY()
        {
            return new(X, Y.Round());
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

        public bool HasIntersectionWith(IGeometry geometry)
        {
            return (IGeometry) this == geometry
                || geometry switch
                {
                    Vector v => this == v,
                    Box box => box.Contains(this),
                    LineSegment line => line.Contains(this),
                    RightTriangle triangle => triangle.Contains(this),
                    GeometrySet set => set.HasIntersectionWith(this),
                    _ => false,
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

    /// <summary>
    /// Segmento de reta
    /// </summary>
    public struct LineSegment : IGeometry
    {
        public const GeometryType type = GeometryType.LINE_SEGMENT;

        public static readonly LineSegment NULL_SEGMENT = new(Vector.NULL_VECTOR, Vector.NULL_VECTOR);

        /// <summary>
        /// Cria um segmento de reta a partir de dois pontos
        /// </summary>
        /// <param name="start">Ponto inicial do segmento</param>
        /// <param name="end">Ponto final do segmento</param>
        public LineSegment(Vector start, Vector end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Ponto inicial do segmento
        /// </summary>
        public Vector Start
        {
            get;
        }

        /// <summary>
        /// Ponto final do segmento
        /// </summary>
        public Vector End
        {
            get;
        }

        /// <summary>
        /// Comprimento do segmento
        /// </summary>
        public FixedSingle Length => (End - Start).Length;

        /// <summary>
        /// Inverte o sentido do segmento trocando seu ponto inicial com seu ponto final
        /// </summary>
        /// <returns>O segmento de reta invertido</returns>
        public LineSegment Negate()
        {
            return new(End, Start);
        }

        /// <summary>
        /// Rotaciona um segmento de reta ao redor de um ponto
        /// </summary>
        /// <param name="origin">Centro de rotação</param>
        /// <param name="angle">Algumo de rotação em radianos</param>
        /// <returns>Segmento de reta rotacionado</returns>
        public LineSegment Rotate(Vector origin, FixedSingle angle)
        {
            Vector u = Start.Rotate(origin, angle);
            Vector v = End.Rotate(origin, angle);
            return new LineSegment(u, v);
        }

        /// <summary>
        /// Rotaciona um segmento de reta em 90 graus ao redor de um ponto
        /// </summary>
        /// <param name="origin">Centro de rotação</param>
        /// <returns>Segmento de reta rotacionado</returns>
        public LineSegment Rotate90(Vector origin)
        {
            Vector u = Start.Rotate90(origin);
            Vector v = End.Rotate90(origin);
            return new LineSegment(u, v);
        }

        /// <summary>
        /// Rotaciona um segmento de reta em 180 graus ao redor de um ponto
        /// </summary>
        /// <param name="origin">Centro de rotação</param>
        /// <returns>Segmento de reta rotacionado</returns>
        public LineSegment Rotate180(Vector origin)
        {
            Vector u = Start.Rotate180(origin);
            Vector v = End.Rotate180(origin);
            return new LineSegment(u, v);
        }

        /// <summary>
        /// Rotaciona um segmento de reta em 270 graus ao redor de um ponto
        /// </summary>
        /// <param name="origin">Centro de rotação</param>
        /// <returns>Segmento de reta rotacionado</returns>
        public LineSegment Rotate270(Vector origin)
        {
            Vector u = Start.Rotate270(origin);
            Vector v = End.Rotate270(origin);
            return new LineSegment(u, v);
        }

        /// <summary>
        /// Compara a posição de um vetor em relação ao segmento de reta
        /// </summary>
        /// <param name="v">Vetor a ser testado</param>
        /// <returns>1 se o vetor está a esquerda do segmento, -1 se estiver a direta, 0 se for colinear ao segmento</returns>
        public int Compare(Vector v)
        {
            FixedDouble f = (FixedDouble) (v.Y - Start.Y) * (End.X - Start.X) - (FixedDouble) (v.X - Start.X) * (End.Y - Start.Y);
            return f.Signal;
        }

        /// <summary>
        /// Verifica se o segmento contém o vetor dado
        /// </summary>
        /// <param name="v">Vetor a ser testado</param>
        /// <returns>true se o segmento contém o vetor, false caso contrário</returns>
        public bool Contains(Vector v, FixedSingle epslon)
        {
            if (Compare(v) != 0)
                return false;

            var mX = FixedSingle.Min(Start.X, End.X);
            var MX = FixedSingle.Max(Start.X, End.X);
            var mY = FixedSingle.Min(Start.Y, End.Y);
            var MY = FixedSingle.Max(Start.Y, End.Y);

            return mX - epslon <= v.X && v.X <= MX + epslon && mY - epslon <= v.Y && v.Y <= MY + epslon;
        }

        public bool Contains(Vector v)
        {
            return Contains(v, 0);
        }

        /// <summary>
        /// Verifica se dois segmentos de reta são paralelos
        /// </summary>
        /// <param name="s">Segmento a ser testado</param>
        /// <returns>true se forem paralelos, false caso contrário</returns>
        public bool IsParallel(LineSegment s)
        {
            FixedSingle A1 = End.Y - Start.Y;
            FixedSingle B1 = End.X - Start.X;
            FixedSingle A2 = s.End.Y - s.Start.Y;
            FixedSingle B2 = s.End.X - s.Start.X;

            return A1 * B2 == A2 * B1;
        }

        /// <summary>
        /// Obtém a intersecção entre dois segmentos de reta
        /// </summary>
        /// <param name="s">Segmento de reta a ser testado</param>
        /// <returns>A intersecção entre os dois segmentos caso ela exista, ou retorna conjunto vazio caso contrário</returns>
        public GeometryType Intersection(LineSegment s, FixedSingle epslon, out LineSegment result)
        {
            Vector v;

            result = NULL_SEGMENT;

            if (s == this)
            {
                result = this;
                return GeometryType.LINE_SEGMENT;
            }

            FixedDouble A1 = End.Y - Start.Y;
            FixedDouble B1 = End.X - Start.X;
            FixedDouble A2 = s.End.Y - s.Start.Y;
            FixedDouble B2 = s.End.X - s.Start.X;

            FixedDouble D = A1 * B2 - A2 * B1;

            FixedDouble C1 = Start.X * End.Y - End.X * Start.Y;
            FixedDouble C2 = s.Start.X * s.End.Y - s.End.X * s.Start.Y;

            if (D == 0)
            {
                if (C1 != 0 || C2 != 0)
                    return GeometryType.EMPTY;

                var xmin = FixedSingle.Max(Start.X, s.Start.X);
                var ymin = FixedSingle.Max(Start.Y, s.Start.Y);
                var xmax = FixedSingle.Min(End.X, s.End.X);
                var ymax = FixedSingle.Min(End.Y, s.End.Y);

                if (xmin < xmax)
                {
                    result = new LineSegment(new Vector(xmin, ymin), new Vector(xmax, ymax));
                    return GeometryType.LINE_SEGMENT;
                }

                if (xmin == xmax)
                {
                    v = (xmin, ymin);
                    result = new LineSegment(v, v);
                    return GeometryType.VECTOR;
                }

                return GeometryType.EMPTY;
            }

            var x = (FixedSingle) ((B2 * C1 - B1 * C2) / D);
            var y = (FixedSingle) ((A2 * C1 - A1 * C2) / D);
            v = (x, y);

            if (!Contains(v))
                return GeometryType.EMPTY;

            result = new LineSegment(v, v);
            return GeometryType.VECTOR;
        }

        public GeometryType Intersection(LineSegment s, out LineSegment result)
        {
            return Intersection(s, 0, out result);
        }

        public bool HasIntersectionWith(LineSegment other, FixedSingle epslon)
        {
            return Intersection(other, epslon, out _) != GeometryType.EMPTY;
        }

        public bool HasIntersectionWith(LineSegment other)
        {
            return HasIntersectionWith(other, 0);
        }

        public bool HasIntersectionWith(IGeometry geometry)
        {
            return (IGeometry) this == geometry
                || geometry switch
                {
                    Vector v => Contains(v),
                    Box box => box.HasIntersectionWith(this),
                    LineSegment line => HasIntersectionWith(line),
                    RightTriangle triangle => triangle.HasIntersectionWith(this),
                    GeometrySet set => set.HasIntersectionWith(this),
                    _ => false,
                };
        }

        public Box WrappingBox()
        {
            return (Start, Vector.NULL_VECTOR, End - Start);
        }

        public override bool Equals(object obj)
        {
            if (obj is not LineSegment)
            {
                return false;
            }

            var segment = (LineSegment) obj;
            return StrictEquals(segment);
        }

        public bool StrictEquals(LineSegment other)
        {
            return Start == other.Start && End == other.End;
        }

        public bool UnstrictEquals(LineSegment other)
        {
            return Start == other.Start && End == other.End || Start == other.End && End == other.Start;
        }

        public override int GetHashCode()
        {
            var hashCode = 1075529825;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector>.Default.GetHashCode(Start);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector>.Default.GetHashCode(End);
            return hashCode;
        }

        public override string ToString()
        {
            return "[" + Start + " : " + End + "]";
        }

        public GeometryType Type => type;

        /// <summary>
        /// Verifica se o vetor v está a direita do seguimento de reta s
        /// </summary>
        /// <param name="v">Vetor</param>
        /// <param name="s">Seguimento de reta</param>
        /// <returns>Resultado da comparação</returns>
        public static bool operator <(Vector v, LineSegment s)
        {
            return s.Compare(v) == -1;
        }

        /// <summary>
        /// Verifica se o vetor v está a direita ou é colinear ao seguimento de reta s
        /// </summary>
        /// <param name="v">Vetor</param>
        /// <param name="s">Seguimento de reta</param>
        /// <returns>Resultado da comparação</returns>
        public static bool operator <=(Vector v, LineSegment s)
        {
            return s.Compare(v) <= 0;
        }

        /// <summary>
        /// Verifica se o vetor v está a esquerda do seguimento de reta s
        /// </summary>
        /// <param name="v">Vetor</param>
        /// <param name="s">Seguimento de reta</param>
        /// <returns>Resultado da comparação</returns>
        public static bool operator >(Vector v, LineSegment s)
        {
            return s.Compare(v) == 1;
        }

        /// <summary>
        /// Verifica se o vetor v está a esquerda ou é colinear ao seguimento de reta s
        /// </summary>
        /// <param name="v">Vetor</param>
        /// <param name="s">Seguimento de reta</param>
        /// <returns>Resultado da comparação</returns>
        public static bool operator >=(Vector v, LineSegment s)
        {
            return s.Compare(v) >= 0;
        }

        /// <summary>
        /// O mesmo que v > s
        /// </summary>
        /// <param name="s">Seguimento de reta</param>
        /// <param name="v">Vetor</param>
        /// <returns>v > s</returns>
        public static bool operator <(LineSegment s, Vector v)
        {
            return v > s;
        }

        /// <summary>
        /// O mesmo que v >= s
        /// </summary>
        /// <param name="s">Seguimento de reta</param>
        /// <param name="v">Vetor</param>
        /// <returns>v >= s</returns>
        public static bool operator <=(LineSegment s, Vector v)
        {
            return v >= s;
        }

        /// <summary>
        /// O mesmo que v < s
        /// </summary>
        /// <param name="s">Seguimento de reta</param>
        /// <param name="v">Vetor</param>
        /// <returns>v < s</returns>
        public static bool operator >(LineSegment s, Vector v)
        {
            return v < s;
        }

        /// <summary>
        /// O mesmo que v <= s
        /// </summary>
        /// <param name="s">Seguimento de reta</param>
        /// <param name="v">Vetor</param>
        /// <returns>v <= s</returns>
        public static bool operator >=(LineSegment s, Vector v)
        {
            return v <= s;
        }

        /// <summary>
        /// Compara se dois seguimentos de reta são iguais
        /// </summary>
        /// <param name="s1">Primeiro seguimento de reta</param>
        /// <param name="s2">Seguindo seguimento de reta</param>
        /// <returns>true se forem iguais, false caso contrário</returns>
        public static bool operator ==(LineSegment s1, LineSegment s2)
        {
            return s1.StrictEquals(s2);
        }

        /// <summary>
        /// Compara se dois seguimentos de reta são diferentes
        /// </summary>
        /// <param name="s1">Primeiro seguimento de reta</param>
        /// <param name="s2">Seguindo seguimento de reta</param>
        /// <returns>true se forem diferentes, false caso contrário</returns>
        public static bool operator !=(LineSegment s1, LineSegment s2)
        {
            return !s1.StrictEquals(s2);
        }
    }

    /// <summary>
    /// Uma matriz quadrada de ordem 2
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Matrix2x2
    {
        /// <summary>
        /// Matriz nula
        /// </summary>
        public static readonly Matrix2x2 NULL_MATRIX = new(0, 0, 0, 0);
        /// <summary>
        /// Matriz identidade
        /// </summary>
        public static readonly Matrix2x2 IDENTITY = new(1, 0, 0, 1);

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4 * 4)]
        private readonly int[] elements;

        /// <summary>
        /// Cria uma matriz a partir de um array de valores numéricos
        /// </summary>
        /// <param name="values"></param>
        public Matrix2x2(params FixedSingle[] values)
        {
            elements = new int[4];
            elements[0] = values[0].RawValue;
            elements[1] = values[1].RawValue;
            elements[2] = values[2].RawValue;
            elements[3] = values[3].RawValue;
        }

        /// <summary>
        /// Cria uma matriz a partir de dois vetores
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public Matrix2x2(Vector v1, Vector v2)
        {
            elements = new int[4];
            elements[0] = v1.X.RawValue;
            elements[1] = v1.Y.RawValue;
            elements[2] = v2.X.RawValue;
            elements[3] = v2.Y.RawValue;
        }

        public FixedSingle Element00 => FixedSingle.FromRawValue(elements[0]);

        public FixedSingle Element01 => FixedSingle.FromRawValue(elements[1]);

        public FixedSingle Element10 => FixedSingle.FromRawValue(elements[2]);

        public FixedSingle Element11 => FixedSingle.FromRawValue(elements[3]);

        public FixedSingle GetElement(int i, int j)
        {
            return FixedSingle.FromRawValue(elements[2 * i + j]);
        }

        /// <summary>
        /// Calcula o determinante da uma matriz
        /// </summary>
        /// <returns>Determinante</returns>
        public FixedDouble Determinant()
        {
            return (FixedDouble) Element00 * (FixedDouble) Element11 - (FixedDouble) Element10 * (FixedDouble) Element01;
        }

        /// <summary>
        /// Transpõe a matriz
        /// </summary>
        /// <returns>Transposta da matriz</returns>
        public Matrix2x2 Transpose()
        {
            return new(Element00, Element10, Element01, Element11);
        }

        /// <summary>
        /// Inverte a matriz
        /// </summary>
        /// <returns>Inversa da matriz</returns>
        public Matrix2x2 Inverse()
        {
            FixedDouble determinant = Determinant();
            return new Matrix2x2(
                    (FixedSingle) (Element01 / determinant),
                    (FixedSingle) (-Element01 / determinant),
                    (FixedSingle) (-Element10 / determinant),
                    (FixedSingle) (Element00 / determinant)
                );
        }

        /// <summary>
        /// Soma entre duas matrizes
        /// </summary>
        /// <param name="m1">Primeira matriz</param>
        /// <param name="m2">Segunda matriz</param>
        /// <returns>Soma</returns>
        public static Matrix2x2 operator +(Matrix2x2 m1, Matrix2x2 m2)
        {
            return new(m1.Element00 + m2.Element00, m1.Element01 + m2.Element01, m1.Element10 + m2.Element10, m1.Element11 + m2.Element11);
        }

        /// <summary>
        /// Diferença/Subtração entre duas matrizes
        /// </summary>
        /// <param name="m1">Primeira matriz</param>
        /// <param name="m2">Segunda matriz</param>
        /// <returns>Diferença</returns>
        public static Matrix2x2 operator -(Matrix2x2 m1, Matrix2x2 m2)
        {
            return new(m1.Element00 - m2.Element00, m1.Element01 - m2.Element01, m1.Element10 - m2.Element10, m1.Element11 - m2.Element11);
        }

        /// <summary>
        /// Oposto aditivo de uma matriz
        /// </summary>
        /// <param name="m">Matriz</param>
        /// <returns>Oposto</returns>
        public static Matrix2x2 operator -(Matrix2x2 m)
        {
            return new(-m.Element00, -m.Element01, -m.Element10, -m.Element11);
        }

        /// <summary>
        /// Produto de uma matriz por um escalar
        /// </summary>
        /// <param name="factor">Escalar</param>
        /// <param name="m">Matriz</param>
        /// <returns>Produto</returns>
        public static Matrix2x2 operator *(FixedSingle factor, Matrix2x2 m)
        {
            return new(factor * m.Element00, factor * m.Element01, factor * m.Element10, factor * m.Element11);
        }

        /// <summary>
        /// Produto de uma matriz por um escalar
        /// </summary>
        /// <param name="m">Matriz</param>
        /// <param name="factor">Escalar</param>
        /// <returns>Produto</returns>
        public static Matrix2x2 operator *(Matrix2x2 m, FixedSingle factor)
        {
            return new(m.Element00 * factor, m.Element01 * factor, m.Element10 * factor, m.Element11 * factor);
        }

        /// <summary>
        /// Divisão de uma matriz por um escalar
        /// </summary>
        /// <param name="m">Matriz</param>
        /// <param name="divisor">Escalar</param>
        /// <returns>Divisão</returns>
        public static Matrix2x2 operator /(Matrix2x2 m, FixedSingle divisor)
        {
            return new(m.Element00 / divisor, m.Element01 / divisor, m.Element10 / divisor, m.Element11 / divisor);
        }

        /// <summary>
        /// Produto entre duas matrizes
        /// </summary>
        /// <param name="m1">Primeira matriz</param>
        /// <param name="m2">Segunda matriz</param>
        /// <returns>Produto matricial</returns>
        public static Matrix2x2 operator *(Matrix2x2 m1, Matrix2x2 m2)
        {
            return new(m1.Element00 * m2.Element00 + m1.Element01 * m2.Element10, m1.Element00 * m2.Element01 + m1.Element01 * m2.Element11,
                                 m1.Element10 * m2.Element00 + m1.Element11 * m2.Element10, m1.Element10 * m2.Element01 + m1.Element11 * m2.Element11);
        }

        /// <summary>
        /// Calcula a matriz de rotação a partir de um angulo dado
        /// </summary>
        /// <param name="angle">Angulo em radianos</param>
        /// <returns>Matriz de rotação</returns>
        public static Matrix2x2 RotationMatrix(FixedSingle angle)
        {
            FixedSingle cos = System.Math.Cos(angle);
            FixedSingle sin = System.Math.Sin(angle);
            return new Matrix2x2(cos, -sin, sin, cos);
        }
    }

    public interface IShape : IGeometry
    {
        FixedDouble Area
        {
            get;
        }
    }

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
                ? XSharpTupleExtensions.CanConvertTupleToArray<Vector>(sourceType)
                : genericSourceType == typeof(ValueTuple<,,>) || genericSourceType == typeof(Tuple<,,>)
                ? XSharpTupleExtensions.CanConvertTupleToArray<Vector>(sourceType)
                : genericSourceType == typeof(ValueTuple<,,,>) || genericSourceType == typeof(Tuple<,,,>)
                ? XSharpTupleExtensions.CanConvertTupleToArray<FixedSingle>(sourceType)
                : genericSourceType == typeof(ValueTuple<,,,,,>) || genericSourceType == typeof(Tuple<,,,,,>)
                ? XSharpTupleExtensions.CanConvertTupleToArray<FixedSingle>(sourceType)
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
                ? XSharpTupleExtensions.ArrayToTuple(destinationType, box.LeftTop, box.RightBottom)
                : genericDestinationType == typeof(ValueTuple<,,>) || genericDestinationType == typeof(Tuple<,,>)
                ? XSharpTupleExtensions.ArrayToTuple(destinationType, box.Origin, box.Mins, box.Maxs)
                : genericDestinationType == typeof(ValueTuple<,,,>) || genericDestinationType == typeof(Tuple<,,,>)
                ? XSharpTupleExtensions.ArrayToTuple(destinationType, box.Left, box.Top, box.Width, box.Height)
                : genericDestinationType == typeof(ValueTuple<,,,,,>) || genericDestinationType == typeof(Tuple<,,,,,>)
                ? XSharpTupleExtensions.ArrayToTuple(destinationType, box.Origin.X, box.Origin.Y, box.Left, box.Top, box.Width, box.Height)
                : base.ConvertTo(context, culture, value, destinationType);
        }
    }

    /// <summary>
    /// Retângulo bidimensional com lados paralelos aos eixos coordenados
    /// </summary>
    [TypeConverter(typeof(BoxTypeConverter))]
    public struct Box : IShape
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

        public Box(BinaryReader reader)
        {
            Origin = new Vector(reader);
            Mins = new Vector(reader);
            Maxs = new Vector(reader);
        }

        public void Write(BinaryWriter writer)
        {
            Origin.Write(writer);
            Mins.Write(writer);
            Maxs.Write(writer);
        }

        /// <summary>
        /// Trunca as coordenadas do retângulo
        /// </summary>
        /// <returns>Retângulo truncado</returns>
        public Box Truncate()
        {
            Vector mins = Origin + Mins;
            mins = (mins.X.Floor(), mins.Y.Floor());
            Vector maxs = Origin + Maxs;
            maxs = (maxs.X.Floor(), maxs.Y.Floor());
            return (mins, Vector.NULL_VECTOR, maxs - mins);
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

        public GeometryType Type => type;

        /// <summary>
        /// Origem do retângulo
        /// </summary>
        public Vector Origin
        {
            get;
        }

        /// <summary>
        /// Mínimos relativos
        /// </summary>
        public Vector Mins
        {
            get;
        }

        /// <summary>
        /// Máximos relativos
        /// </summary>
        public Vector Maxs
        {
            get;
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

        public Box RoundOrigin()
        {
            return (Origin.Round(), Mins, Maxs);
        }

        public Box RoundOriginX()
        {
            return (Origin.RoundX(), Mins, Maxs);
        }

        public Box RoundOriginY()
        {
            return (Origin.RoundY(), Mins, Maxs);
        }

        /// <summary>
        /// Área do retângulo
        /// </summary>
        /// <returns></returns>
        public FixedDouble Area => (FixedDouble) Width * (FixedDouble) Height;

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

        public bool IsValid(FixedSingle epslon)
        {
            return Width > epslon && Height > epslon;
        }

        public bool IsValid()
        {
            return IsValid(0);
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

        public bool Contains(Vector point, FixedSingle epslon, BoxSide include = BoxSide.LEFT | BoxSide.TOP | BoxSide.INNER)
        {
            var m = Origin + Mins;
            var M = Origin + Maxs;

            if (!include.HasFlag(BoxSide.INNER))
                return include.HasFlag(BoxSide.LEFT) && LeftSegment.Contains(point, epslon)
                    || include.HasFlag(BoxSide.TOP) && TopSegment.Contains(point, epslon)
                    || include.HasFlag(BoxSide.RIGHT) && RightSegment.Contains(point, epslon)
                    || include.HasFlag(BoxSide.BOTTOM) && BottomSegment.Contains(point, epslon);

            var interval = Interval.MakeInterval((m.X, include.HasFlag(BoxSide.LEFT)), (M.X, include.HasFlag(BoxSide.RIGHT)));

            if (!interval.Contains(point.X, epslon))
                return false;

            interval = Interval.MakeInterval((m.Y, include.HasFlag(BoxSide.TOP)), (M.Y, include.HasFlag(BoxSide.BOTTOM)));

            return interval.Contains(point.Y, epslon);
        }

        public bool Contains(Vector point, BoxSide include)
        {
            return Contains(point, 0, include);
        }

        public bool Contains(Vector point)
        {
            return Contains(point, 0, BoxSide.LEFT | BoxSide.TOP | BoxSide.INNER);
        }

        public bool HasIntersectionWith(LineSegment line, FixedSingle epslon)
        {
            var type = Intersection(line, out line);
            return type != GeometryType.EMPTY;
        }

        public bool HasIntersectionWith(LineSegment line)
        {
            return HasIntersectionWith(line, 0);
        }

        public bool IsOverlaping(Box other, BoxSide includeSides = BoxSide.LEFT | BoxSide.TOP, BoxSide includeOtherBoxSides = BoxSide.LEFT | BoxSide.TOP)
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
                    GeometrySet set => set.HasIntersectionWith(this),
                    _ => false,
                };
        }

        public FixedSingle Length => FixedSingle.TWO * (Width + Height);

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

        public override bool Equals(object obj)
        {
            if (obj is not RightTriangle)
            {
                return false;
            }

            var triangle = (RightTriangle) obj;
            return EqualityComparer<Vector>.Default.Equals(Origin, triangle.Origin) &&
                   EqualityComparer<FixedSingle>.Default.Equals(hCathetus, triangle.hCathetus) &&
                   EqualityComparer<FixedSingle>.Default.Equals(vCathetus, triangle.vCathetus);
        }

        public override int GetHashCode()
        {
            var hashCode = -1211292891;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector>.Default.GetHashCode(Origin);
            hashCode = hashCode * -1521134295 + EqualityComparer<FixedSingle>.Default.GetHashCode(hCathetus);
            hashCode = hashCode * -1521134295 + EqualityComparer<FixedSingle>.Default.GetHashCode(vCathetus);
            return hashCode;
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
        public static void HorizontalParallelogram(Vector origin, Vector direction, FixedSingle height, out Box box, out RightTriangle triangle1, out RightTriangle triangle2)
        {
            if (direction.X > 0)
            {
                if (direction.Y > 0)
                {
                    box = new Box(origin, direction.X, direction.Y + height);
                    triangle1 = new RightTriangle(origin + (direction.X, 0), -direction.X, direction.Y);
                    triangle2 = new RightTriangle(origin + (0, direction.Y + height), direction.X, -direction.Y);
                }
                else
                {
                    box = new Box(origin + (0, direction.Y), direction.X, height - direction.Y);
                    triangle1 = new RightTriangle(origin + (0, direction.Y), direction.X, -direction.Y);
                    triangle2 = new RightTriangle(origin + (direction.X, height), -direction.X, direction.Y);
                }
            }
            else
            {
                if (direction.Y > 0)
                {
                    box = new Box(origin + (direction.X, 0), -direction.X, direction.Y + height);
                    triangle1 = new RightTriangle(origin + (direction.X, 0), -direction.X, direction.Y);
                    triangle2 = new RightTriangle(origin + (0, direction.Y + height), direction.X, -direction.Y);
                }
                else
                {
                    box = new Box(origin + direction, -direction.X, height - direction.Y);
                    triangle1 = new RightTriangle(origin + (0, direction.Y), direction.X, -direction.Y);
                    triangle2 = new RightTriangle(origin + (direction.X, height), -direction.X, direction.Y);
                }
            }
        }
    }
}