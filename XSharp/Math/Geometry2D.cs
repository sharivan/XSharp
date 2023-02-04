using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using XSharp.Math;

namespace XSharp.Geometry
{
    public enum GeometryType
    {
        EMPTY = 0,
        VECTOR = 1,
        LINE_SEGMENT = 2,
        RIGHT_TRIANGLE = 3,
        BOX = 4,
        POLIGON = 5,
        UNION = 6
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

        FixedDouble Area
        {
            get;
        }
    }

    public struct EmptyGeometry : IGeometry
    {
        public const GeometryType type = GeometryType.EMPTY;

        public FixedSingle Length => FixedSingle.ZERO;

        public FixedDouble Area => FixedDouble.ZERO;

        public GeometryType Type => type;
    }

    public struct Union : IGeometry
    {
        public const GeometryType type = GeometryType.UNION;

        public static readonly Union EMPTY_SET = new();

        private readonly IGeometry[] parts;

        /// <summary>
        /// Cria uma união a partir das partes
        /// </summary>
        /// <param name="parts">Partes</param>
        public Union(params IGeometry[] parts)
        {
            this.parts = parts;
        }

        public GeometryType Type => type;

        public FixedSingle Length => throw new NotImplementedException();

        public FixedDouble Area => throw new NotImplementedException();

        /// <summary>
        /// Quantidade de partes disjuntas contidas na união
        /// </summary>
        public int Count => parts.Length;

        public override bool Equals(object obj)
        {
            if (obj is not Union)
            {
                return false;
            }

            var union = (Union) obj;
            return this == union;
        }

        public override int GetHashCode()
        {
            var hashCode = 1480434725;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<IGeometry[]>.Default.GetHashCode(parts);
            return hashCode;
        }

        /// <summary>
        /// Retorna true se a união for vazia, false caso contrário
        /// </summary>
        /// <returns></returns>
        public bool Empty => parts.Length == 0;

        /// <summary>
        /// Igualdade entre uniões
        /// </summary>
        /// <param name="set1">Primeira união</param>
        /// <param name="set2">Segunda união</param>
        /// <returns>true se as uniões forem iguais, false caso contrário</returns>
        public static bool operator ==(Union set1, Union set2)
        {
            var list = set2.parts.ToList<IGeometry>();

            for (int i = 0; i < set1.parts.Length; i++)
            {
                IGeometry g1 = set1.parts[i];
                bool found = false;

                for (int j = 0; j < list.Count; j++)
                {
                    IGeometry g2 = list[j];

                    if (g1 == g2)
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

        /// <summary>
        /// Inequalidade entre uniões
        /// </summary>
        /// <param name="set1">Primeira união</param>
        /// <param name="set2">Segunda união</param>
        /// <returns>true se as uniões forem diferentes, false caso contrário</returns>
        public static bool operator !=(Union set1, Union set2)
        {
            return !(set1 == set2);
        }
    }

    /// <summary>
    /// Vetor bidimensional
    /// </summary>
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

        public FixedDouble Area => FixedDouble.ZERO;

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
            if (X.Abs <= epslon && Y.Abs <= epslon)
                return NULL_VECTOR;

            FixedSingle abs = Length;
            return new Vector(X / abs, Y / abs);
        }

        public Vector Versor()
        {
            if (IsNull)
                return NULL_VECTOR;

            FixedSingle abs = Length;
            return new Vector(X / abs, Y / abs);
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

        public GeometryType Type => type;

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
            FixedSingle f = (v.Y - Start.Y) * (End.X - Start.X) - (v.X - Start.X) * (End.Y - Start.Y);

            return f > 0 ? 1 : f < 0 ? -1 : 0;
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

            return mX + epslon <= v.X && v.X <= MX - epslon && mY + epslon <= v.Y && v.Y <= MY - epslon;
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
        public GeometryType Intersection(LineSegment s, FixedSingle epslon, out Vector resultVector, out LineSegment resultLineSegment)
        {
            resultVector = Vector.NULL_VECTOR;
            resultLineSegment = NULL_SEGMENT;

            if (s == this)
            {
                resultLineSegment = this;
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
                    resultLineSegment = new LineSegment(new Vector(xmin, ymin), new Vector(xmax, ymax));
                    return GeometryType.LINE_SEGMENT;
                }

                if (xmin == xmax)
                {
                    resultVector = new Vector(xmin, ymin);
                    return GeometryType.VECTOR;
                }

                return GeometryType.EMPTY;
            }

            var x = (FixedSingle) ((B2 * C1 - B1 * C2) / D);
            var y = (FixedSingle) ((A2 * C1 - A1 * C2) / D);
            var v = new Vector(x, y);

            if (!Contains(v))
                return GeometryType.EMPTY;

            resultVector = v;
            return GeometryType.VECTOR;
        }

        public GeometryType Intersection(LineSegment s, out Vector resultVector, out LineSegment resultLineSegment)
        {
            return Intersection(s, 0, out resultVector, out resultLineSegment);
        }

        public Box WrappingBox()
        {
            return new(Start, Vector.NULL_VECTOR, End - Start);
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

        public FixedDouble Area => FixedDouble.ZERO;

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
        public FixedSingle Determinant()
        {
            return Element00 * Element11 - Element10 * Element01;
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
            return new Matrix2x2(Element01, -Element01, -Element10, Element00) / Determinant();
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
    }

    public enum BoxSide
    {
        LEFT = 0,
        UP = 1,
        RIGHT = 2,
        DOWN = 3
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

    /// <summary>
    /// Retângulo bidimensional com lados paralelos aos eixos coordenados
    /// </summary>
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

        public Box(FixedSingle x, FixedSingle y, FixedSingle left, FixedSingle top, FixedSingle width, FixedSingle height)
        {
            Origin = (x, y);
            Mins = (left - x, top - y);
            Maxs = (left + width - x, top + height - y);
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

        /// <summary>
        /// Origem do retângulo
        /// </summary>
        public Vector Origin
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
            return Mirror(0);
        }

        public Box Flip()
        {
            return Flip(0);
        }

        public Box Mirror(FixedSingle x)
        {
            FixedSingle originX = Origin.X;
            x += originX;
            FixedSingle minsX = originX + Mins.X;
            FixedSingle maxsX = originX + Maxs.X;

            FixedSingle newMinsX = 2 * x - maxsX;
            FixedSingle newMaxsX = 2 * x - minsX;

            return new Box(Origin, new Vector(newMinsX - originX, Mins.Y), new Vector(newMaxsX - originX, Maxs.Y));
        }

        public Box Flip(FixedSingle y)
        {
            FixedSingle originY = Origin.Y;
            y += originY;
            FixedSingle minsY = originY + Mins.Y;
            FixedSingle maxsY = originY + Maxs.Y;

            FixedSingle newMinsY = 2 * y - maxsY;
            FixedSingle newMaxsY = 2 * y - minsY;

            return new Box(Origin, new Vector(Mins.X, newMinsY - originY), new Vector(Maxs.X, newMaxsY - originY));
        }

        public Vector GetNormal(BoxSide side)
        {
            return side switch
            {
                BoxSide.LEFT => Vector.RIGHT_VECTOR,
                BoxSide.UP => Vector.DOWN_VECTOR,
                BoxSide.RIGHT => Vector.LEFT_VECTOR,
                BoxSide.DOWN => Vector.UP_VECTOR,
                _ => Vector.NULL_VECTOR,
            };
        }

        public LineSegment GetSideSegment(BoxSide side)
        {
            return side switch
            {
                BoxSide.LEFT => new LineSegment(LeftTop, LeftBottom),
                BoxSide.UP => new LineSegment(LeftTop, RightTop),
                BoxSide.RIGHT => new LineSegment(RightTop, RightBottom),
                BoxSide.DOWN => new LineSegment(LeftBottom, RightBottom),
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

        public Box Scale(Vector center, FixedSingle scaleX, FixedSingle scaleY)
        {
            return new((Origin - center).Scale(scaleX, scaleY) + center, Mins.Scale(scaleX, scaleY), Maxs.Scale(scaleX, scaleY));
        }

        public Box Scale(Vector center, FixedSingle scale)
        {
            return Scale(center, scale, scale);
        }

        public Box Scale(FixedSingle scaleX, FixedSingle scaleY)
        {
            return Scale(Vector.NULL_VECTOR, scaleX, scaleY);
        }

        public Box Scale(FixedSingle scale)
        {
            return Scale(Vector.NULL_VECTOR, scale, scale);
        }

        public Box ScaleInverse(Vector center, FixedSingle divisorX, FixedSingle divisorY)
        {
            return new((Origin - center).ScaleInverse(divisorX, divisorY) + center, Mins.ScaleInverse(divisorX, divisorY), Maxs.ScaleInverse(divisorX, divisorY));
        }

        public Box ScaleInverse(Vector center, FixedSingle divisor)
        {
            return ScaleInverse(center, divisor, divisor);
        }

        public Box ScaleInverse(FixedSingle divisorX, FixedSingle divisorY)
        {
            return ScaleInverse(Vector.NULL_VECTOR, divisorX, divisorY);
        }

        public Box ScaleInverse(FixedSingle divisor)
        {
            return ScaleInverse(Vector.NULL_VECTOR, divisor, divisor);
        }

        public GeometryType Type => type;

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
            Vector lt1 = box1.LeftTop;
            Vector rb1 = box1.RightBottom;

            Vector lt2 = box2.LeftTop;
            Vector rb2 = box2.RightBottom;

            var left = FixedSingle.Min(lt1.X, lt2.X);
            var right = FixedSingle.Max(rb1.X, rb2.X);

            var top = FixedSingle.Min(lt1.Y, lt2.Y);
            var bottom = FixedSingle.Max(rb1.Y, rb2.Y);

            return new Box(new Vector(box1.Mins.X <= box1.Maxs.X ? left : right, box1.Mins.Y <= box1.Maxs.Y ? top : bottom), box1.Mins.X <= box1.Maxs.X ? right - left : left - right, box1.Mins.Y <= box1.Maxs.Y ? bottom - top : top - bottom);
        }

        /// <summary>
        /// Faz a intersecção de dois retângulos que resultará em um novo retângulo que esteja contido nos dois retângulos dados. Se os dois retângulos dados forem disjuntos então o resultado será um vetor nulo.
        /// </summary>
        /// <param name="box1">Primeiro retângulo</param>
        /// <param name="box2">Segundo retângulo</param>
        /// <returns>Interesecção entre os dois retângulos dados ou um vetor nulo caso a intersecção seja um conjunto vazio</returns>
        public static Box operator &(Box box1, Box box2)
        {
            Vector lt1 = box1.LeftTop;
            Vector rb1 = box1.RightBottom;

            Vector lt2 = box2.LeftTop;
            Vector rb2 = box2.RightBottom;

            var left = FixedSingle.Max(lt1.X, lt2.X);
            var right = FixedSingle.Min(rb1.X, rb2.X);

            if (right < left)
                return EMPTY_BOX;

            var top = FixedSingle.Max(lt1.Y, lt2.Y);
            var bottom = FixedSingle.Min(rb1.Y, rb2.Y);

            return bottom < top ? EMPTY_BOX : new Box(new Vector(left, top), Vector.NULL_VECTOR, new Vector(right - left, bottom - top));
        }

        public static LineSegment operator &(Box box, LineSegment line)
        {
            if (line.Start <= box)
            {
                if (line.End <= box)
                    return line;

                for (BoxSide side = BoxSide.LEFT; side <= BoxSide.DOWN; side++)
                {
                    LineSegment sideSegment = box.GetSideSegment(side);
                    if (sideSegment.Contains(line.Start))
                        continue;

                    GeometryType type = line.Intersection(sideSegment, out Vector v, out LineSegment l);
                    switch (type)
                    {
                        case GeometryType.VECTOR:
                            return new LineSegment(line.Start, v);

                        case GeometryType.LINE_SEGMENT:
                            return l;
                    }
                }

                return LineSegment.NULL_SEGMENT;
            }

            if (line.End <= box)
            {
                if (line.Start <= box)
                    return line;

                for (BoxSide side = BoxSide.LEFT; side <= BoxSide.DOWN; side++)
                {
                    LineSegment sideSegment = box.GetSideSegment(side);
                    if (sideSegment.Contains(line.End))
                        continue;

                    GeometryType type = line.Intersection(sideSegment, out Vector v, out LineSegment l);
                    switch (type)
                    {
                        case GeometryType.VECTOR:
                            return new LineSegment(line.End, v);

                        case GeometryType.LINE_SEGMENT:
                            return l;
                    }
                }

                return LineSegment.NULL_SEGMENT;
            }

            Vector foundVector = Vector.NULL_VECTOR;
            bool found = false;
            BoxSide foundSide = BoxSide.LEFT;
            for (BoxSide side = BoxSide.LEFT; side <= BoxSide.DOWN; side++)
            {
                GeometryType type = line.Intersection(box.GetSideSegment(side), out foundVector, out LineSegment l);
                switch (type)
                {
                    case GeometryType.VECTOR:
                        found = true;
                        foundSide = side;
                        break;

                    case GeometryType.LINE_SEGMENT:
                        return l;
                }
            }

            if (!found)
                return LineSegment.NULL_SEGMENT;

            for (BoxSide side = BoxSide.LEFT; side <= BoxSide.DOWN; side++)
            {
                if (side == foundSide)
                    continue;

                GeometryType type = line.Intersection(box.GetSideSegment(side), out Vector v, out LineSegment l);
                switch (type)
                {
                    case GeometryType.VECTOR:
                        return new LineSegment(foundVector, v);
                }
            }

            return new LineSegment(foundVector, foundVector);
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
            Vector m = box.Origin + box.Mins;
            Vector M = box.Origin + box.Maxs;

            Interval interval = m.X <= M.X ? Interval.MakeSemiOpenRightInterval(m.X, M.X) : Interval.MakeSemiOpenLeftInterval(M.X, m.X);
            if (!interval.Contains(vec.X))
                return false;

            interval = m.Y <= M.Y ? Interval.MakeSemiOpenRightInterval(m.Y, M.Y) : Interval.MakeSemiOpenLeftInterval(M.Y, m.Y);
            return interval.Contains(vec.Y);
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
            return vec <= box;
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

        public static implicit operator Box((Vector, FixedSingle, FixedSingle) tuple)
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

        public static implicit operator (Vector, FixedSingle, FixedSingle)(Box box)
        {
            return (box.LeftTop, box.Width, box.Height);
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
    }

    public enum RightTriangleSide
    {
        HCATHETUS,
        VCATHETUS,
        HYPOTENUSE
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

        public Vector HCathetusVertex => Origin + HCathetusVector;

        public Vector VCathetusVertex => Origin + VCathetusVector;

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

        public LineSegment HypotenuseLine => new(HCathetusVertex, VCathetusVertex);

        public LineSegment HCathetusLine => new(Origin, HCathetusVertex);

        public LineSegment VCathetusLine => new(Origin, VCathetusVertex);

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

        private static FixedSingle Sign(Vector p1, Vector p2, Vector p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        private static bool PointInTriangle(Vector pt, Vector v1, Vector v2, Vector v3)
        {
            FixedSingle d1, d2, d3;
            bool has_neg, has_pos;

            d1 = Sign(pt, v1, v2);
            d2 = Sign(pt, v2, v3);
            d3 = Sign(pt, v3, v1);

            has_neg = d1 < 0 || d2 < 0 || d3 < 0;
            has_pos = d1 > 0 || d2 > 0 || d3 > 0;

            return !(has_neg && has_pos);
        }

        public bool Contains(Vector v, FixedSingle epslon, bool excludeHypotenuse = false)
        {
            if (excludeHypotenuse)
            {
                LineSegment hypotenuseLine = HypotenuseLine;
                if (hypotenuseLine.Contains(v, epslon))
                    return false;
            }

            if (hCathetus == 0)
            {
                var interval = Interval.MakeClosedInterval(Origin.Y, VCathetusVector.Y);
                return interval.Contains(v.Y, epslon);
            }

            if (vCathetus == 0)
            {
                var interval = Interval.MakeClosedInterval(Origin.X, VCathetusVector.X);
                return interval.Contains(v.X, epslon);
            }

            return PointInTriangle(v, Origin, HCathetusVertex + (epslon, 0), VCathetusVertex + (0, epslon));
        }

        public bool Contains(Vector v, bool excludeHypotenuse = false)
        {
            return Contains(v, 0, excludeHypotenuse);
        }

        public bool HasIntersectionWith(Box box, FixedSingle epslon, bool excludeHypotenuse = false)
        {
            Box intersection = box & WrappingBox;
            return intersection.IsValid(epslon)
            && (Contains(intersection.LeftTop, epslon, excludeHypotenuse)
            || Contains(intersection.LeftBottom, epslon, excludeHypotenuse)
            || Contains(intersection.RightTop, epslon, excludeHypotenuse)
            || Contains(intersection.RightBottom, epslon, excludeHypotenuse));
        }

        public bool HasIntersectionWith(Box box, bool excludeHypotenuse = false)
        {
            return HasIntersectionWith(box, 0, excludeHypotenuse);
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
}
