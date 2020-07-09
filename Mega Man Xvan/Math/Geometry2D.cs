using MMX.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace MMX.Geometry
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

        FixedSingle Area
        {
            get;
        }
    }

    public struct EmptyGeometry : IGeometry
    {
        public const GeometryType type = GeometryType.EMPTY;

        public FixedSingle Length => FixedSingle.ZERO;

        public FixedSingle Area => FixedSingle.ZERO;

        public GeometryType Type => type;
    }

    public struct Union : IGeometry
    {
        public const GeometryType type = GeometryType.UNION;

        public static readonly Union EMPTY_SET = new Union();

        private IGeometry[] parts;

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

        public FixedSingle Area => throw new NotImplementedException();

        /// <summary>
        /// Quantidade de partes disjuntas contidas na união
        /// </summary>
        public int Count
        {
            get
            {
                return parts.Length;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Union))
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
            List<IGeometry> list = set2.parts.ToList<IGeometry>();

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
        public static readonly Vector NULL_VECTOR = new Vector(0, 0); // Vetor nulo
                                                                      /// <summary>
                                                                      /// Vetor leste
                                                                      /// </summary>
        public static readonly Vector LEFT_VECTOR = new Vector(-1, 0);
        /// <summary>
        /// Vetor norte
        /// </summary>
        public static readonly Vector UP_VECTOR = new Vector(0, -1);
        /// <summary>
        /// Vetor oeste
        /// </summary>
        public static readonly Vector RIGHT_VECTOR = new Vector(1, 0);
        /// <summary>
        /// Vetor sul
        /// </summary>
        public static readonly Vector DOWN_VECTOR = new Vector(0, 1);

        private FixedSingle x; // Coordenada x
        private FixedSingle y; // Coordenada y

        /// <summary>
        /// Coordenada x do vetor
        /// </summary>
        public FixedSingle X
        {
            get
            {
                return x;
            }
        }

        /// <summary>
        /// Coordenada y do vetor
        /// </summary>
        public FixedSingle Y
        {
            get
            {
                return y;
            }
        }

        /// <summary>
        /// Módulo/Norma/Comprimento do vetor
        /// </summary>
        public FixedSingle Length
        {
            get
            {
                if (this.x == 0)
                    return this.y.Abs;

                if (this.y == 0)
                    return this.x.Abs;

                FixedDouble x = this.x;
                FixedDouble y = this.y;
                return System.Math.Sqrt(x * x + y * y);
            }
        }

        /// <summary>
        /// Retorna true se este vetor for nulo, false caso contrário
        /// </summary>
        public bool IsNull
        {
            get
            {
                return x == 0 && y == 0;
            }
        }

        public Vector XVector
        {
            get
            {
                return new Vector(X, 0);
            }
        }

        public Vector YVector
        {
            get
            {
                return new Vector(0, Y);
            }
        }

        public FixedSingle Area => FixedSingle.ZERO;

        /// <summary>
        /// Cria um vetor a partir de duas coordenadas
        /// </summary>
        /// <param name="x">Coordenada x</param>
        /// <param name="y">Coordenada y</param>
        public Vector(FixedSingle x, FixedSingle y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector(BinaryReader reader)
        {
            x = new FixedSingle(reader);
            y = new FixedSingle(reader);
        }

        public void Write(BinaryWriter writer)
        {
            x.Write(writer);
            y.Write(writer);
        }

        public override int GetHashCode()
        {
            return (x.GetHashCode() << 16) + y.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector))
                return false;

            Vector other = (Vector) obj;
            return this == other;
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ")";
        }

        /// <summary>
        /// Normaliza o vetor
        /// </summary>
        /// <returns>O vetor normalizado</returns>
        public Vector Versor()
        {
            if (IsNull)
                return NULL_VECTOR;

            FixedSingle abs = Length;
            return new Vector(x / abs, y / abs);
        }

        /// <summary>
        /// Rotaciona o vetor ao redor da origem
        /// </summary>
        /// <param name="angle">Angulo de rotação em radianos</param>
        /// <returns>O vetor rotacionado</returns>
        public Vector Rotate(FixedSingle angle)
        {
            FixedDouble x = this.x;
            FixedDouble y = this.y;
            FixedDouble a = (FixedDouble) angle;

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
            return new Vector(-y, x);
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
            return new Vector(-x, -y);
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
            return new Vector(y, -x);
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
            return new Vector(x.Ceil(), y.Ceil());
        }

        public Vector RoundToFloor()
        {
            return new Vector(x.Floor(), y.Floor());
        }

        public Vector RoundXToCeil()
        {
            return new Vector(x.Ceil(), y);
        }

        public Vector RoundXToFloor()
        {
            return new Vector(x.Floor(), y);
        }

        public Vector RoundYToCeil()
        {
            return new Vector(x, y.Ceil());
        }

        public Vector RoundYToFloor()
        {
            return new Vector(x, y.Floor());
        }

        public Vector Round()
        {
            return new Vector(x.Round(), y.Round());
        }

        public Vector RoundX()
        {
            return new Vector(x.Round(), y);
        }

        public Vector RoundY()
        {
            return new Vector(x, y.Round());
        }

        /// <summary>
        /// Distâcia entre vetores
        /// </summary>
        /// <param name="vec">Vetor no qual será medido a sua distância até este vetor</param>
        /// <returns>A distância entre este vetor e o vetor dado</returns>
        public FixedSingle DistanceTo(Vector vec)
        {
            FixedDouble dx = x - vec.x;
            FixedDouble dy = y - vec.y;
            return System.Math.Sqrt(dx * dx + dy * dy);
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
            return new Vector(vec1.x + vec2.x, vec1.y + vec2.y);
        }

        /// <summary>
        /// Subtração de vetores
        /// </summary>
        /// <param name="vec1">Primeiro vetor</param>
        /// <param name="vec2">Segundo vetor</param>
        /// <returns>Diferença entre os dois vetores</returns>
        public static Vector operator -(Vector vec1, Vector vec2)
        {
            return new Vector(vec1.x - vec2.x, vec1.y - vec2.y);
        }

        /// <summary>
        /// Inverte o sentido do vetor
        /// </summary>
        /// <param name="vec">Vetor</param>
        /// <returns>O oposto do vetor</returns>
        public static Vector operator -(Vector vec)
        {
            return new Vector(-vec.x, -vec.y);
        }

        /// <summary>
        /// Produto de um vetor por um escalar
        /// </summary>
        /// <param name="alpha">Escalar</param>
        /// <param name="vec">Vetor</param>
        /// <returns>O vetor escalado por alpha</returns>
        public static Vector operator *(FixedSingle alpha, Vector vec)
        {
            return new Vector(alpha * vec.x, alpha * vec.y);
        }

        /// <summary>
        /// Produto de um vetor por um escalar
        /// </summary>
        /// <param name="vec">Vetor</param>
        /// <param name="alpha">Escalar</param>
        /// <returns>O vetor escalado por alpha</returns>
        public static Vector operator *(Vector vec, FixedSingle alpha)
        {
            return new Vector(alpha * vec.x, alpha * vec.y);
        }

        /// <summary>
        /// Divisão de vetor por um escalar, o mesmo que multiplicar o vetor pelo inverso do escalar
        /// </summary>
        /// <param name="vec">Vetor</param>
        /// <param name="alpha">Escalar</param>
        /// <returns>O vetor dividido pelo escalar alpha</returns>
        public static Vector operator /(Vector vec, FixedSingle alpha)
        {
            return new Vector(vec.x / alpha, vec.y / alpha);
        }

        /// <summary>
        /// Produto escalar/interno/ponto entre dois vetores
        /// </summary>
        /// <param name="vec1">Primeiro vetor</param>
        /// <param name="vec2">Segundo vetor</param>
        /// <returns>Produto escalar entre os dois vetores</returns>
        public static FixedSingle operator *(Vector vec1, Vector vec2)
        {
            return vec1.x * vec2.x + vec1.y * vec2.y;
        }

        /// <summary>
        /// Igualdade entre vetores
        /// </summary>
        /// <param name="vec1">Primeiro vetor</param>
        /// <param name="vec2">Segundo vetor</param>
        /// <returns>true se os vetores forem iguais, false caso contrário</returns>
        public static bool operator ==(Vector vec1, Vector vec2)
        {
            return vec1.x == vec2.x && vec1.y == vec2.y;
        }

        /// <summary>
        /// Inequalidade entre vetores
        /// </summary>
        /// <param name="vec1">Primeiro vetor</param>
        /// <param name="vec2">Segundo vetor</param>
        /// <returns>true se os vetores forem diferentes, false caso contrário</returns>
        public static bool operator !=(Vector vec1, Vector vec2)
        {
            return vec1.x != vec2.x || vec1.y != vec2.y;
        }
    }

    /// <summary>
    /// Segmento de reta
    /// </summary>
    public struct LineSegment : IGeometry
    {
        public const GeometryType type = GeometryType.LINE_SEGMENT;

        public static readonly LineSegment NULL_SEGMENT = new LineSegment(Vector.NULL_VECTOR, Vector.NULL_VECTOR);

        private Vector start; // Ponto inicial do segmento
        private Vector end; // Ponto final do segmento

        /// <summary>
        /// Cria um segmento de reta a partir de dois pontos
        /// </summary>
        /// <param name="start">Ponto inicial do segmento</param>
        /// <param name="end">Ponto final do segmento</param>
        public LineSegment(Vector start, Vector end)
        {
            this.start = start;
            this.end = end;
        }

        /// <summary>
        /// Ponto inicial do segmento
        /// </summary>
        public Vector Start
        {
            get
            {
                return start;
            }
        }

        /// <summary>
        /// Ponto final do segmento
        /// </summary>
        public Vector End
        {
            get
            {
                return end;
            }
        }

        /// <summary>
        /// Comprimento do segmento
        /// </summary>
        public FixedSingle Length => (end - start).Length;

        /// <summary>
        /// Inverte o sentido do segmento trocando seu ponto inicial com seu ponto final
        /// </summary>
        /// <returns>O segmento de reta invertido</returns>
        public LineSegment Negate()
        {
            return new LineSegment(end, start);
        }

        /// <summary>
        /// Rotaciona um segmento de reta ao redor de um ponto
        /// </summary>
        /// <param name="origin">Centro de rotação</param>
        /// <param name="angle">Algumo de rotação em radianos</param>
        /// <returns>Segmento de reta rotacionado</returns>
        public LineSegment Rotate(Vector origin, FixedSingle angle)
        {
            Vector u = start.Rotate(origin, angle);
            Vector v = end.Rotate(origin, angle);
            return new LineSegment(u, v);
        }

        /// <summary>
        /// Rotaciona um segmento de reta em 90 graus ao redor de um ponto
        /// </summary>
        /// <param name="origin">Centro de rotação</param>
        /// <returns>Segmento de reta rotacionado</returns>
        public LineSegment Rotate90(Vector origin)
        {
            Vector u = start.Rotate90(origin);
            Vector v = end.Rotate90(origin);
            return new LineSegment(u, v);
        }

        /// <summary>
        /// Rotaciona um segmento de reta em 180 graus ao redor de um ponto
        /// </summary>
        /// <param name="origin">Centro de rotação</param>
        /// <returns>Segmento de reta rotacionado</returns>
        public LineSegment Rotate180(Vector origin)
        {
            Vector u = start.Rotate180(origin);
            Vector v = end.Rotate180(origin);
            return new LineSegment(u, v);
        }

        /// <summary>
        /// Rotaciona um segmento de reta em 270 graus ao redor de um ponto
        /// </summary>
        /// <param name="origin">Centro de rotação</param>
        /// <returns>Segmento de reta rotacionado</returns>
        public LineSegment Rotate270(Vector origin)
        {
            Vector u = start.Rotate270(origin);
            Vector v = end.Rotate270(origin);
            return new LineSegment(u, v);
        }

        /// <summary>
        /// Compara a posição de um vetor em relação ao segmento de reta
        /// </summary>
        /// <param name="v">Vetor a ser testado</param>
        /// <returns>1 se o vetor está a esquerda do segmento, -1 se estiver a direta, 0 se for colinear ao segmento</returns>
        public int Compare(Vector v)
        {
            FixedSingle f = (v.Y - start.Y) * (end.X - start.X) - (v.X - start.X) * (end.Y - start.Y);

            if (f > 0)
                return 1;

            if (f < 0)
                return -1;

            return 0;
        }

        /// <summary>
        /// Verifica se o segmento contém o vetor dado
        /// </summary>
        /// <param name="v">Vetor a ser testado</param>
        /// <returns>true se o segmento contém o vetor, false caso contrário</returns>
        public bool Contains(Vector v)
        {
            if (Compare(v) != 0)
                return false;

            FixedSingle mX = FixedSingle.Min(start.X, end.X);
            FixedSingle MX = FixedSingle.Max(start.X, end.X);
            FixedSingle mY = FixedSingle.Min(start.Y, end.Y);
            FixedSingle MY = FixedSingle.Max(start.Y, end.Y);

            return mX <= v.X && v.X <= MX && mY <= v.Y && v.Y <= MY;
        }

        /// <summary>
        /// Verifica se dois segmentos de reta são paralelos
        /// </summary>
        /// <param name="s">Segmento a ser testado</param>
        /// <returns>true se forem paralelos, false caso contrário</returns>
        public bool IsParallel(LineSegment s)
        {
            FixedSingle A1 = end.Y - start.Y;
            FixedSingle B1 = end.X - start.X;
            FixedSingle A2 = s.end.Y - s.start.Y;
            FixedSingle B2 = s.end.X - s.start.X;

            return A1 * B2 == A2 * B1;
        }

        /// <summary>
        /// Obtém a intersecção entre dois segmentos de reta
        /// </summary>
        /// <param name="s">Segmento de reta a ser testado</param>
        /// <returns>A intersecção entre os dois segmentos caso ela exista, ou retorna conjunto vazio caso contrário</returns>
        public GeometryType Intersection(LineSegment s, out Vector resultVector, out LineSegment resultLineSegment)
        {
            resultVector = Vector.NULL_VECTOR;
            resultLineSegment = NULL_SEGMENT;

            if (s == this)
            {
                resultLineSegment = this;
                return GeometryType.LINE_SEGMENT;
            }

            FixedDouble A1 = end.Y - start.Y;
            FixedDouble B1 = end.X - start.X;
            FixedDouble A2 = s.end.Y - s.start.Y;
            FixedDouble B2 = s.end.X - s.start.X;

            FixedDouble D = A1 * B2 - A2 * B1;

            FixedDouble C1 = start.X * end.Y - end.X * start.Y;
            FixedDouble C2 = s.start.X * s.end.Y - s.end.X * s.start.Y;

            if (D == 0)
            {
                if (C1 != 0 || C2 != 0)
                    return GeometryType.EMPTY;

                FixedSingle xmin = FixedSingle.Max(start.X, s.start.X);
                FixedSingle ymin = FixedSingle.Max(start.Y, s.start.Y);
                FixedSingle xmax = FixedSingle.Min(end.X, s.end.X);
                FixedSingle ymax = FixedSingle.Min(end.Y, s.end.Y);

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

            FixedSingle x = (FixedSingle) ((B2 * C1 - B1 * C2) / D);
            FixedSingle y = (FixedSingle) ((A2 * C1 - A1 * C2) / D);
            Vector v = new Vector(x, y);

            if (!Contains(v))
                return GeometryType.EMPTY;

            resultVector = v;
            return GeometryType.VECTOR;
        }

        public Box WrappingBox()
        {
            return new Box(start, Vector.NULL_VECTOR, end - start);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is LineSegment))
            {
                return false;
            }

            var segment = (LineSegment) obj;
            return StrictEquals(segment);
        }

        public bool StrictEquals(LineSegment other)
        {
            return start == other.start && end == other.end;
        }

        public bool UnstrictEquals(LineSegment other)
        {
            return start == other.start && end == other.end || start == other.end && end == other.start;
        }

        public override int GetHashCode()
        {
            var hashCode = 1075529825;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector>.Default.GetHashCode(start);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector>.Default.GetHashCode(end);
            return hashCode;
        }

        public override string ToString()
        {
            return "[" + start + " : " + end + "]";
        }

        public GeometryType Type => type;

        public FixedSingle Area => FixedSingle.ZERO;

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
    public struct Matrix2x2
    {
        /// <summary>
        /// Matriz nula
        /// </summary>
        public static readonly Matrix2x2 NULL_MATRIX = new Matrix2x2(0, 0, 0, 0);
        /// <summary>
        /// Matriz identidade
        /// </summary>
        public static readonly Matrix2x2 IDENTITY = new Matrix2x2(1, 0, 0, 1);

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4 * 4)]
        private int[] elements;

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

        public FixedSingle Element00
        {
            get
            {
                return FixedSingle.FromRawValue(elements[0]);
            }
        }

        public FixedSingle Element01
        {
            get
            {
                return FixedSingle.FromRawValue(elements[1]);
            }
        }

        public FixedSingle Element10
        {
            get
            {
                return FixedSingle.FromRawValue(elements[2]);
            }
        }

        public FixedSingle Element11
        {
            get
            {
                return FixedSingle.FromRawValue(elements[3]);
            }
        }

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
            return new Matrix2x2(Element00, Element10, Element01, Element11);
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
            return new Matrix2x2(m1.Element00 + m2.Element00, m1.Element01 + m2.Element01, m1.Element10 + m2.Element10, m1.Element11 + m2.Element11);
        }

        /// <summary>
        /// Diferença/Subtração entre duas matrizes
        /// </summary>
        /// <param name="m1">Primeira matriz</param>
        /// <param name="m2">Segunda matriz</param>
        /// <returns>Diferença</returns>
        public static Matrix2x2 operator -(Matrix2x2 m1, Matrix2x2 m2)
        {
            return new Matrix2x2(m1.Element00 - m2.Element00, m1.Element01 - m2.Element01, m1.Element10 - m2.Element10, m1.Element11 - m2.Element11);
        }

        /// <summary>
        /// Oposto aditivo de uma matriz
        /// </summary>
        /// <param name="m">Matriz</param>
        /// <returns>Oposto</returns>
        public static Matrix2x2 operator -(Matrix2x2 m)
        {
            return new Matrix2x2(-m.Element00, -m.Element01, -m.Element10, -m.Element11);
        }

        /// <summary>
        /// Produto de uma matriz por um escalar
        /// </summary>
        /// <param name="factor">Escalar</param>
        /// <param name="m">Matriz</param>
        /// <returns>Produto</returns>
        public static Matrix2x2 operator *(FixedSingle factor, Matrix2x2 m)
        {
            return new Matrix2x2(factor * m.Element00, factor * m.Element01, factor * m.Element10, factor * m.Element11);
        }

        /// <summary>
        /// Produto de uma matriz por um escalar
        /// </summary>
        /// <param name="m">Matriz</param>
        /// <param name="factor">Escalar</param>
        /// <returns>Produto</returns>
        public static Matrix2x2 operator *(Matrix2x2 m, FixedSingle factor)
        {
            return new Matrix2x2(m.Element00 * factor, m.Element01 * factor, m.Element10 * factor, m.Element11 * factor);
        }

        /// <summary>
        /// Divisão de uma matriz por um escalar
        /// </summary>
        /// <param name="m">Matriz</param>
        /// <param name="divisor">Escalar</param>
        /// <returns>Divisão</returns>
        public static Matrix2x2 operator /(Matrix2x2 m, FixedSingle divisor)
        {
            return new Matrix2x2(m.Element00 / divisor, m.Element01 / divisor, m.Element10 / divisor, m.Element11 / divisor);
        }

        /// <summary>
        /// Produto entre duas matrizes
        /// </summary>
        /// <param name="m1">Primeira matriz</param>
        /// <param name="m2">Segunda matriz</param>
        /// <returns>Produto matricial</returns>
        public static Matrix2x2 operator *(Matrix2x2 m1, Matrix2x2 m2)
        {
            return new Matrix2x2(m1.Element00 * m2.Element00 + m1.Element01 * m2.Element10, m1.Element00 * m2.Element01 + m1.Element01 * m2.Element11,
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
        public static readonly Box EMPTY_BOX = new Box(0, 0, 0, 0);
        /// <summary>
        /// Retângulo universo
        /// </summary>
        public static readonly Box UNIVERSE_BOX = new Box(Vector.NULL_VECTOR, new Vector(FixedSingle.MIN_VALUE, FixedSingle.MIN_VALUE), new Vector(FixedSingle.MAX_VALUE, FixedSingle.MAX_VALUE));

        private Vector origin; // origen
        private Vector mins; // mínimos
        private Vector maxs; // máximos

        /// <summary>
        /// Cria um retângulo vazio com uma determinada origem
        /// </summary>
        /// <param name="origin">origem do retângulo</param>
        public Box(Vector origin)
        {
            this.origin = origin;
            mins = Vector.NULL_VECTOR;
            maxs = Vector.NULL_VECTOR;
        }

        /// <summary>
        /// Cria um retângulo a partir da origem, mínimos e máximos
        /// </summary>
        /// <param name="origin">Origem</param>
        /// <param name="mins">Mínimos</param>
        /// <param name="maxs">Máximos</param>
        public Box(Vector origin, Vector mins, Vector maxs)
        {
            this.origin = origin;
            this.mins = mins;
            this.maxs = maxs;
        }

        public Box(FixedSingle x, FixedSingle y, FixedSingle width, FixedSingle height) :
            this(new Vector(x, y), width, height)
        {
        }

        public Box(FixedSingle left, FixedSingle top, FixedSingle width, FixedSingle height, OriginPosition originPosition)
        {
            switch (originPosition)
            {
                case OriginPosition.LEFT_TOP:
                    origin = new Vector(left, top);
                    mins = Vector.NULL_VECTOR;
                    maxs = new Vector(width, height);
                    break;

                case OriginPosition.LEFT_MIDDLE:
                    origin = new Vector(left, top + height * FixedSingle.HALF);
                    mins = new Vector(0, -height * FixedSingle.HALF);
                    maxs = new Vector(width, height * FixedSingle.HALF);
                    break;

                case OriginPosition.LEFT_BOTTOM:
                    origin = new Vector(left, top + height);
                    mins = new Vector(0, -height);
                    maxs = new Vector(width, 0);
                    break;

                case OriginPosition.MIDDLE_TOP:
                    origin = new Vector(left + width * FixedSingle.HALF, top);
                    mins = new Vector(-width * FixedSingle.HALF, 0);
                    maxs = new Vector(width * FixedSingle.HALF, height);
                    break;

                case OriginPosition.CENTER:
                    origin = new Vector(left + width * FixedSingle.HALF, top + height * FixedSingle.HALF);
                    mins = new Vector(-width * FixedSingle.HALF, -height * FixedSingle.HALF);
                    maxs = new Vector(width * FixedSingle.HALF, height * FixedSingle.HALF);
                    break;

                case OriginPosition.MIDDLE_BOTTOM:
                    origin = new Vector(left + width * FixedSingle.HALF, top + height);
                    mins = new Vector(-width * FixedSingle.HALF, -height);
                    maxs = new Vector(width * FixedSingle.HALF, 0);
                    break;

                case OriginPosition.RIGHT_TOP:
                    origin = new Vector(left + width, top);
                    mins = new Vector(-width, 0);
                    maxs = new Vector(0, height);
                    break;

                case OriginPosition.RIGHT_MIDDLE:
                    origin = new Vector(left + width, top + height * FixedSingle.HALF);
                    mins = new Vector(-width, -height * FixedSingle.HALF);
                    maxs = new Vector(0, height * FixedSingle.HALF);
                    break;

                case OriginPosition.RIGHT_BOTTOM:
                    origin = new Vector(left + width, top + height);
                    mins = new Vector(-width, -height);
                    maxs = Vector.NULL_VECTOR;
                    break;

                default:
                    throw new ArgumentException("Unrecognized Origin Position.");
            }
        }

        public Box(Vector origin, FixedSingle width, FixedSingle height)
        {
            this.origin = origin;
            mins = Vector.NULL_VECTOR;
            maxs = new Vector(width, height);
        }

        public Box(FixedSingle x, FixedSingle y, FixedSingle left, FixedSingle top, FixedSingle width, FixedSingle height)
        {
            origin = new Vector(x, y);
            mins = new Vector(left - x, top - y);
            maxs = new Vector(left + width - x, top + height - y);
        }

        public Box(Vector v1, Vector v2)
        {
            origin = v1;
            mins = Vector.NULL_VECTOR;
            maxs = v2 - v1;
        }

        public Box(BinaryReader reader)
        {
            origin = new Vector(reader);
            mins = new Vector(reader);
            maxs = new Vector(reader);
        }

        public void Write(BinaryWriter writer)
        {
            origin.Write(writer);
            mins.Write(writer);
            maxs.Write(writer);
        }

        /// <summary>
        /// Trunca as coordenadas do retângulo
        /// </summary>
        /// <returns>Retângulo truncado</returns>
        public Box Truncate()
        {
            Vector mins = origin + this.mins;
            mins = new Vector(mins.X.Floor(), mins.Y.Floor());
            Vector maxs = origin + this.maxs;
            maxs = new Vector(maxs.X.Floor(), maxs.Y.Floor());
            return new Box(mins, Vector.NULL_VECTOR, maxs - mins);
        }

        public override int GetHashCode()
        {
            Vector m = origin + mins;
            Vector M = origin + maxs;

            return 65536 * m.GetHashCode() + M.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is Box))
                return false;

            Box other = (Box) obj;
            return this == other;
        }

        public override string ToString()
        {
            return "[" + origin + " : " + mins + " : " + maxs + "]";
        }

        /// <summary>
        /// Origem do retângulo
        /// </summary>
        public Vector Origin
        {
            get
            {
                return origin;
            }
        }

        public FixedSingle X
        {
            get
            {
                return origin.X;
            }
        }

        public FixedSingle Y
        {
            get
            {
                return origin.Y;
            }
        }

        public FixedSingle Left
        {
            get
            {
                return FixedSingle.Min(origin.X + mins.X, origin.X + maxs.X);
            }
        }

        public FixedSingle Top
        {
            get
            {
                return FixedSingle.Min(origin.Y + mins.Y, origin.Y + maxs.Y);
            }
        }

        public FixedSingle Right
        {
            get
            {
                return FixedSingle.Max(origin.X + mins.X, origin.X + maxs.X);
            }
        }

        public FixedSingle Bottom
        {
            get
            {
                return FixedSingle.Max(origin.Y + mins.Y, origin.Y + maxs.Y);
            }
        }

        public LineSegment LeftSegment
        {
            get
            {
                return new LineSegment(LeftTop, LeftBottom);
            }
        }

        public LineSegment TopSegment
        {
            get
            {
                return new LineSegment(LeftTop, RightTop);
            }
        }

        public LineSegment RightSegment
        {
            get
            {
                return new LineSegment(RightTop, RightBottom);
            }
        }

        public LineSegment BottomSegment
        {
            get
            {
                return new LineSegment(LeftBottom, RightBottom);
            }
        }

        /// <summary>
        /// Extremo superior esquerdo do retângulo (ou mínimos absolutos)
        /// </summary>
        public Vector LeftTop
        {
            get
            {
                return new Vector(Left, Top);
            }
        }

        public Vector LeftMiddle
        {
            get
            {
                return new Vector(Left, (Top + Bottom) / 2);
            }
        }

        public Vector LeftBottom
        {
            get
            {
                return new Vector(Left, Bottom);
            }
        }

        public Vector RightTop
        {
            get
            {
                return new Vector(Right, Top);
            }
        }

        public Vector RightMiddle
        {
            get
            {
                return new Vector(Right, (Top + Bottom) / 2);
            }
        }

        public Vector MiddleTop
        {
            get
            {
                return new Vector((Left + Right) / 2, Top);
            }
        }

        public Vector MiddleBottom
        {
            get
            {
                return new Vector((Left + Right) / 2, Bottom);
            }
        }

        /// <summary>
        /// Extremo inferior direito do retângulo (ou máximos absolutos)
        /// </summary>
        public Vector RightBottom
        {
            get
            {
                return new Vector(Right, Bottom);
            }
        }

        public Vector Center
        {
            get
            {
                return origin + (mins + maxs) / 2;
            }
        }

        /// <summary>
        /// Mínimos relativos
        /// </summary>
        public Vector Mins
        {
            get
            {
                return mins;
            }
        }

        /// <summary>
        /// Máximos relativos
        /// </summary>
        public Vector Maxs
        {
            get
            {
                return maxs;
            }
        }

        public Vector WidthVector
        {
            get
            {
                return new Vector(Width, 0);
            }
        }

        public Vector HeightVector
        {
            get
            {
                return new Vector(0, Height);
            }
        }

        /// <summary>
        /// Vetor correspondente ao tamanho do retângulo contendo sua largura (width) na coordenada x e sua altura (height) na coordenada y
        /// </summary>
        public Vector DiagonalVector
        {
            get
            {
                return new Vector(Width, Height);
            }
        }

        /// <summary>
        /// Largura (base) do retângulo
        /// </summary>
        public FixedSingle Width
        {
            get
            {
                return (maxs.X - mins.X).Abs;
            }
        }

        /// <summary>
        /// Altura do retângulo
        /// </summary>
        public FixedSingle Height
        {
            get
            {
                return (maxs.Y - mins.Y).Abs;
            }
        }

        public Box LeftTopOrigin()
        {
            return new Box(LeftTop, Vector.NULL_VECTOR, DiagonalVector);
        }

        public Box RightBottomOrigin()
        {
            return new Box(RightBottom, -DiagonalVector, Vector.NULL_VECTOR);
        }

        public Box CenterOrigin()
        {
            Vector sv2 = DiagonalVector / 2;
            return new Box(Center, -sv2, sv2);
        }

        public Box RoundOriginToCeil()
        {
            return new Box(origin.RoundToCeil(), mins, maxs);
        }

        public Box RoundOriginXToCeil()
        {
            return new Box(origin.RoundXToCeil(), mins, maxs);
        }

        public Box RoundOriginYToCeil()
        {
            return new Box(origin.RoundYToCeil(), mins, maxs);
        }

        public Box RoundOriginToFloor()
        {
            return new Box(origin.RoundToFloor(), mins, maxs);
        }

        public Box RoundOriginXToFloor()
        {
            return new Box(origin.RoundXToFloor(), mins, maxs);
        }

        public Box RoundOriginYToFloor()
        {
            return new Box(origin.RoundYToFloor(), mins, maxs);
        }

        public Box RoundOrigin()
        {
            return new Box(origin.Round(), mins, maxs);
        }

        public Box RoundOriginX()
        {
            return new Box(origin.RoundX(), mins, maxs);
        }

        public Box RoundOriginY()
        {
            return new Box(origin.RoundY(), mins, maxs);
        }

        /// <summary>
        /// Área do retângulo
        /// </summary>
        /// <returns></returns>
        public FixedSingle Area => Width * Height;

        /// <summary>
        /// Escala o retângulo para a esquerda
        /// </summary>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public Box ScaleLeft(FixedSingle alpha)
        {
            FixedSingle width = Width;
            return new Box(LeftTop + alpha * (width - 1) * Vector.LEFT_VECTOR, Vector.NULL_VECTOR, new Vector(alpha * width, Height));
        }

        public Box ClipLeft(FixedSingle clip)
        {
            return new Box(origin, new Vector(mins.X + clip, mins.Y), maxs);
        }

        /// <summary>
        /// Escala o retângulo para a direita
        /// </summary>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public Box ScaleRight(FixedSingle alpha)
        {
            return new Box(LeftTop, Vector.NULL_VECTOR, new Vector(alpha * Width, Height));
        }

        public Box ClipRight(FixedSingle clip)
        {
            return new Box(origin, mins, new Vector(maxs.X - clip, maxs.Y));
        }

        /// <summary>
        /// Escala o retângulo para cima
        /// </summary>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public Box ScaleTop(FixedSingle alpha)
        {
            FixedSingle height = Height;
            return new Box(LeftTop + alpha * (height - 1) * Vector.UP_VECTOR, Vector.NULL_VECTOR, new Vector(Width, alpha * height));
        }

        public Box ClipTop(FixedSingle clip)
        {
            return new Box(origin, new Vector(mins.X, mins.Y + clip), maxs);
        }

        /// <summary>
        /// Escala o retângulo para baixo
        /// </summary>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public Box ScaleBottom(FixedSingle alpha)
        {
            return new Box(LeftTop, Vector.NULL_VECTOR, new Vector(Width, alpha * Height));
        }

        public Box ClipBottom(FixedSingle clip)
        {
            return new Box(origin, mins, new Vector(maxs.X, maxs.Y - clip));
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
            FixedSingle originX = origin.X;
            x += originX;
            FixedSingle minsX = originX + mins.X;
            FixedSingle maxsX = originX + maxs.X;

            FixedSingle newMinsX = 2 * x - maxsX;
            FixedSingle newMaxsX = 2 * x - minsX;

            return new Box(origin, new Vector(newMinsX - originX, mins.Y), new Vector(newMaxsX - originX, maxs.Y));
        }

        public Box Flip(FixedSingle y)
        {
            FixedSingle originY = origin.Y;
            y += originY;
            FixedSingle minsY = originY + mins.Y;
            FixedSingle maxsY = originY + maxs.Y;

            FixedSingle newMinsY = 2 * y - maxsY;
            FixedSingle newMaxsY = 2 * y - minsY;

            return new Box(origin, new Vector(mins.X, newMinsY - originY), new Vector(maxs.X, newMaxsY - originY));
        }

        public Vector GetNormal(BoxSide side)
        {
            switch (side)
            {
                case BoxSide.LEFT:
                    return Vector.RIGHT_VECTOR;

                case BoxSide.UP:
                    return Vector.DOWN_VECTOR;

                case BoxSide.RIGHT:
                    return Vector.LEFT_VECTOR;

                case BoxSide.DOWN:
                    return Vector.UP_VECTOR;
            }

            return Vector.NULL_VECTOR;
        }

        public LineSegment GetSideSegment(BoxSide side)
        {
            switch (side)
            {
                case BoxSide.LEFT:
                    return new LineSegment(LeftTop, LeftBottom);

                case BoxSide.UP:
                    return new LineSegment(LeftTop, RightTop);

                case BoxSide.RIGHT:
                    return new LineSegment(RightTop, RightBottom);

                case BoxSide.DOWN:
                    return new LineSegment(LeftBottom, RightBottom);
            }

            return LineSegment.NULL_SEGMENT;
        }

        public Box HalfLeft()
        {
            return new Box(origin, mins, new Vector((mins.X + maxs.X) * FixedSingle.HALF, maxs.Y));
        }

        public Box HalfTop()
        {
            return new Box(origin, mins, new Vector(maxs.X, (mins.Y + maxs.Y) * FixedSingle.HALF));
        }

        public Box HalfRight()
        {
            return new Box(origin, new Vector((mins.X + maxs.X) * FixedSingle.HALF, mins.Y), maxs);
        }

        public Box HalfBottom()
        {
            return new Box(origin, new Vector(mins.X, (mins.Y + maxs.Y) * FixedSingle.HALF), maxs);
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
            return new Box(box.origin + vec, box.mins, box.maxs);
        }

        /// <summary>
        /// Translata um retângulo na direção de um vetor
        /// </summary>
        /// <param name="vec">Vetor</param>
        /// /// <param name="box">Retângulo</param>
        /// <returns>Retângulo box translatado na direção de vec</returns>

        public static Box operator +(Vector vec, Box box)
        {
            return new Box(box.origin + vec, box.mins, box.maxs);
        }

        /// <summary>
        /// Translata um retângulo na direção oposta de um vetor
        /// </summary>
        /// <param name="box">Retângulo</param>
        /// <param name="vec">Vetor</param>
        /// <returns>Retângulo box translatado na direção oposta de vec</returns>
        public static Box operator -(Box box, Vector vec)
        {
            return new Box(box.origin - vec, box.mins, box.maxs);
        }

        /// <summary>
        /// Escala um retângulo
        /// </summary>
        /// <param name="factor">Fator de escala</param>
        /// <param name="box">Retângulo</param>
        /// <returns>Retângulo com suas coordenadas e dimensões escaladas por factor</returns>
        public static Box operator *(FixedSingle factor, Box box)
        {
            Vector m = box.origin + box.mins;
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
            Vector m = box.origin + box.mins;
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
            Vector m = box.origin + box.mins;
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

            FixedSingle minX = FixedSingle.Min(lt1.X, lt2.X);
            FixedSingle maxX = FixedSingle.Max(rb1.X, rb2.X);

            FixedSingle minY = FixedSingle.Min(lt1.Y, lt2.Y);
            FixedSingle maxY = FixedSingle.Max(rb1.Y, rb2.Y);

            return new Box(new Vector(box1.mins.X <= box1.maxs.X ? minX : maxX, box1.mins.Y <= box1.maxs.Y ? minY : maxY), box1.mins.X <= box1.maxs.X ? maxX - minX : minX - maxX, box1.mins.Y <= box1.maxs.Y ? maxY - minY : minY - maxY);
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

            FixedSingle minX = FixedSingle.Max(lt1.X, lt2.X);
            FixedSingle maxX = FixedSingle.Min(rb1.X, rb2.X);

            if (maxX < minX)
                return EMPTY_BOX;

            FixedSingle minY = FixedSingle.Max(lt1.Y, lt2.Y);
            FixedSingle maxY = FixedSingle.Min(rb1.Y, rb2.Y);

            if (maxY < minY)
                return EMPTY_BOX;

            return new Box(new Vector(minX, minY), Vector.NULL_VECTOR, new Vector(maxX - minX, maxY - minY));
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
                if(line.Start <= box)
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
            Vector m = box.origin + box.mins;
            Vector M = box.origin + box.maxs;

            Interval interval = Interval.MakeOpenInterval(m.X, M.X);
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
            Vector m = box.origin + box.mins;
            Vector M = box.origin + box.maxs;

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
            return (box1 <= box2) && (box1 != box2);
        }

        /// <summary>
        /// Verifica se um retângulo contém inteiramente outro retângulo
        /// </summary>
        /// <param name="box1">Primeiro retângulo</param>
        /// <param name="box2">Segundo retângulo</param>
        /// <returns>true se box1 contém inteiramente box2 (ou seja, box1 contém box2 mas box1 não é igual a box2), false caso contrário</returns>
        public static bool operator >(Box box1, Box box2)
        {
            return (box2 <= box1) && (box1 != box2);
        }

        /// <summary>
        /// Verifica se dois retângulos são iguais
        /// </summary>
        /// <param name="box1">Primeiro retângulo</param>
        /// <param name="box2">Segundo retângulo</param>
        /// <returns>true se forem iguais, false caso contrário</returns>
        public static bool operator ==(Box box1, Box box2)
        {
            Vector m1 = box1.origin + box1.mins;
            Vector m2 = box2.origin + box2.mins;

            if (m1 != m2)
                return false;

            Vector M1 = box1.origin + box1.maxs;
            Vector M2 = box2.origin + box2.maxs;

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

        public static readonly RightTriangle EMPTY = new RightTriangle(Vector.NULL_VECTOR, 0, 0);

        private Vector origin;
        private FixedSingle hCathetus;
        private FixedSingle vCathetus;

        public Vector Origin
        {
            get
            {
                return origin;
            }
        }

        public Vector HCathetusVertex
        {
            get
            {
                return origin + HCathetusVector;
            }
        }

        public Vector VCathetusVertex
        {
            get
            {
                return origin + VCathetusVector;
            }
        }

        public FixedSingle HCathetus
        {
            get
            {
                return hCathetus.Abs;
            }
        }

        public FixedSingle VCathetus
        {
            get
            {
                return vCathetus.Abs;
            }
        }

        public FixedSingle Hypotenuse
        {
            get
            {
                FixedDouble h = hCathetus;
                FixedDouble v = vCathetus;
                return System.Math.Sqrt(h * h + v * v);
            }
        }

        public Vector HCathetusVector
        {
            get
            {
                return new Vector(hCathetus, 0);
            }
        }

        public Vector VCathetusVector
        {
            get
            {
                return new Vector(0, vCathetus);
            }
        }

        public Vector HypotenuseVector
        {
            get
            {
                return HCathetusVector - VCathetusVector;
            }
        }

        public LineSegment HypotenuseLine
        {
            get
            {
                return new LineSegment(HCathetusVertex, VCathetusVertex);
            }
        }

        public LineSegment HCathetusLine
        {
            get
            {
                return new LineSegment(origin, HCathetusVertex);
            }
        }

        public LineSegment VCathetusLine
        {
            get
            {
                return new LineSegment(origin, VCathetusVertex);
            }
        }

        public Box WrappingBox
        {
            get
            {
                return new Box(FixedSingle.Min(origin.X, origin.X + hCathetus), FixedSingle.Min(origin.Y, origin.Y + vCathetus), HCathetus, VCathetus);
            }
        }

        public FixedSingle Left
        {
            get
            {
                return FixedSingle.Min(origin.X, origin.X + hCathetus);
            }
        }

        public FixedSingle Top
        {
            get
            {
                return FixedSingle.Min(origin.Y, origin.Y + vCathetus);
            }
        }

        public FixedSingle Right
        {
            get
            {
                return FixedSingle.Max(origin.X, origin.X + hCathetus);
            }
        }

        public FixedSingle Bottom
        {
            get
            {
                return FixedSingle.Max(origin.Y, origin.Y + vCathetus);
            }
        }

        public Vector LeftTop
        {
            get
            {
                return new Vector(Left, Top);
            }
        }

        public Vector RightBottom
        {
            get
            {
                return new Vector(Right, Bottom);
            }
        }

        public int HCathetusSign
        {
            get
            {
                return hCathetus.Signal;
            }
        }
        public int VCathetusSign
        {
            get
            {
                return vCathetus.Signal;
            }
        }

        public RightTriangle(Vector origin, FixedSingle hCathetus, FixedSingle vCathetus)
        {
            this.origin = origin;
            this.hCathetus = hCathetus;
            this.vCathetus = vCathetus;
        }

        public RightTriangle Translate(Vector shift)
        {
            return new RightTriangle(origin + shift, hCathetus, vCathetus);
        }

        public RightTriangle Negate()
        {
            return new RightTriangle(-origin, -hCathetus, -vCathetus);
        }

        public FixedSingle Area => FixedSingle.HALF * HCathetus * VCathetus;

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
                        Vector v = new Vector(sign * vCathetus, -sign * hCathetus);
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

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        public bool Contains(Vector v, bool inclusive = true, bool excludeHypotenuse = false)
        {
            if (excludeHypotenuse)
            {
                LineSegment hypotenuseLine = HypotenuseLine;
                if (hypotenuseLine.Contains(v))
                    return false;
            }

            if (hCathetus == 0)
            {
                Interval interval = Interval.MakeClosedInterval(origin.Y, VCathetusVector.Y);
                return interval.Contains(v.Y);
            }

            if (vCathetus == 0)
            {
                Interval interval = Interval.MakeClosedInterval(origin.X, VCathetusVector.X);
                return interval.Contains(v.X);
            }

            return PointInTriangle(v, origin, HCathetusVertex, VCathetusVertex);
        }

        public bool HasIntersectionWith(Box box, bool excludeHypotenuse = false)
        {
            Box intersection = box & WrappingBox;
            if (intersection.Area == 0)
                return false;

            if (Contains(intersection.LeftTop, true, excludeHypotenuse))
                return true;

            if (Contains(intersection.LeftBottom, true, excludeHypotenuse))
                return true;

            if (Contains(intersection.RightTop, true, excludeHypotenuse))
                return true;

            if (Contains(intersection.RightBottom, false, excludeHypotenuse))
                return true;

            return false;
        }

        public override string ToString()
        {
            return "[" + origin + " : " + hCathetus + " : " + vCathetus + "]";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RightTriangle))
            {
                return false;
            }

            var triangle = (RightTriangle) obj;
            return EqualityComparer<Vector>.Default.Equals(origin, triangle.origin) &&
                   EqualityComparer<FixedSingle>.Default.Equals(hCathetus, triangle.hCathetus) &&
                   EqualityComparer<FixedSingle>.Default.Equals(vCathetus, triangle.vCathetus);
        }

        public override int GetHashCode()
        {
            var hashCode = -1211292891;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector>.Default.GetHashCode(origin);
            hashCode = hashCode * -1521134295 + EqualityComparer<FixedSingle>.Default.GetHashCode(hCathetus);
            hashCode = hashCode * -1521134295 + EqualityComparer<FixedSingle>.Default.GetHashCode(vCathetus);
            return hashCode;
        }

        public GeometryType Type => type;

        public FixedSingle Length => HCathetus + VCathetus + Hypotenuse;

        public static bool operator ==(RightTriangle left, RightTriangle right)
        {
            return left.origin == right.origin && left.hCathetus == right.hCathetus && left.vCathetus == right.vCathetus;
        }
        public static bool operator !=(RightTriangle left, RightTriangle right)
        {
            return left.origin != right.origin || left.hCathetus != right.hCathetus || left.vCathetus != right.vCathetus;
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
