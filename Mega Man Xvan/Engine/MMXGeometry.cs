using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMX.Engine
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

    public interface MMXGeometry
    {
        GeometryType GetType();
    }

    public sealed class EmptyGeometry : MMXGeometry
    {
        public const GeometryType type = GeometryType.EMPTY;

        GeometryType MMXGeometry.GetType()
        {
            return type;
        }
    }

    public struct MMXUnion : MMXGeometry
    {
        public const GeometryType type = GeometryType.UNION;

        public static readonly MMXUnion EMPTY_SET = new MMXUnion();

        private MMXGeometry[] parts;

        /// <summary>
        /// Cria uma união a partir das partes
        /// </summary>
        /// <param name="parts">Partes</param>
        public MMXUnion(params MMXGeometry[] parts)
        {
            this.parts = parts;
        }

        GeometryType MMXGeometry.GetType()
        {
            return type;
        }

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

        /// <summary>
        /// Retorna true se a união for vazia, false caso contrário
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return parts.Length == 0;
        }

        /// <summary>
        /// Igualdade entre uniões
        /// </summary>
        /// <param name="set1">Primeira união</param>
        /// <param name="set2">Segunda união</param>
        /// <returns>true se as uniões forem iguais, false caso contrário</returns>
        public static bool operator ==(MMXUnion set1, MMXUnion set2)
        {
            List<MMXGeometry> list = set2.parts.ToList<MMXGeometry>();

            for (int i = 0; i < set1.parts.Length; i++)
            {
                MMXGeometry g1 = set1.parts[i];
                bool found = false;

                for (int j = 0; j < list.Count; j++)
                {
                    MMXGeometry g2 = list[j];

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
        public static bool operator !=(MMXUnion set1, MMXUnion set2)
        {
            return !(set1 == set2);
        }
    }

    /// <summary>
    /// Vetor bidimensional
    /// </summary>
    public struct MMXVector : MMXGeometry
    {
        public const GeometryType type = GeometryType.VECTOR;

        /// <summary>
        /// Vetor nulo
        /// </summary>
        public static readonly MMXVector NULL_VECTOR = new MMXVector(0, 0); // Vetor nulo
                                                                      /// <summary>
                                                                      /// Vetor leste
                                                                      /// </summary>
        public static readonly MMXVector LEFT_VECTOR = new MMXVector(-1, 0);
        /// <summary>
        /// Vetor norte
        /// </summary>
        public static readonly MMXVector UP_VECTOR = new MMXVector(0, -1);
        /// <summary>
        /// Vetor oeste
        /// </summary>
        public static readonly MMXVector RIGHT_VECTOR = new MMXVector(1, 0);
        /// <summary>
        /// Vetor sul
        /// </summary>
        public static readonly MMXVector DOWN_VECTOR = new MMXVector(0, 1);

        private MMXFloat x; // Coordenada x
        private MMXFloat y; // Coordenada y

        /// <summary>
        /// Coordenada x do vetor
        /// </summary>
        public MMXFloat X
        {
            get
            {
                return x;
            }
        }

        /// <summary>
        /// Coordenada y do vetor
        /// </summary>
        public MMXFloat Y
        {
            get
            {
                return y;
            }
        }

        /// <summary>
        /// Módulo/Norma/Comprimento do vetor
        /// </summary>
        public MMXFloat Length
        {
            get
            {
                if (x == 0)
                    return y.Abs;

                if (y == 0)
                    return x.Abs;

                return Math.Sqrt((double) x * (double) x + (double) y * (double) y);
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

        public MMXVector XVector
        {
            get
            {
                return new MMXVector(X, 0);
            }
        }

        public MMXVector YVector
        {
            get
            {
                return new MMXVector(0, Y);
            }
        }

        /// <summary>
        /// Cria um vetor a partir de duas coordenadas
        /// </summary>
        /// <param name="x">Coordenada x</param>
        /// <param name="y">Coordenada y</param>
        public MMXVector(MMXFloat x, MMXFloat y)
        {
            this.x = x;
            this.y = y;
        }

        public MMXVector(BinaryReader reader)
        {
            x = new MMXFloat(reader);
            y = new MMXFloat(reader);
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
            if (!(obj is MMXVector))
                return false;

            MMXVector other = (MMXVector) obj;
            return other.x == x && other.y == y;
        }

        public override string ToString()
        {
            return "(" + x + ";" + y + ")";
        }

        /// <summary>
        /// Normaliza o vetor
        /// </summary>
        /// <returns>O vetor normalizado</returns>
        public MMXVector Versor()
        {
            if (IsNull)
                return NULL_VECTOR;

            MMXFloat abs = Length;
            return new MMXVector(x / abs, y / abs);
        }

        /// <summary>
        /// Rotaciona o vetor ao redor da origem
        /// </summary>
        /// <param name="angle">Angulo de rotação em radianos</param>
        /// <returns>O vetor rotacionado</returns>
        public MMXVector Rotate(MMXFloat angle)
        {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);

            return new MMXVector((double) x * cos - (double) y * sin, (double) x * sin + (double) y * cos);
        }

        /// <summary>
        /// Rotaciona o vetor ao redor de um outro vetor
        /// </summary>
        /// <param name="center">Centro de rotação</param>
        /// <param name="angle">Angulo de rotação em radianos</param>
        /// <returns>O vetor rotacionado</returns>
        public MMXVector Rotate(MMXVector center, MMXFloat angle)
        {
            return (this - center).Rotate(angle) + center;
        }

        /// <summary>
        /// Rotaciona um vetor em 90 graus ao redor da origem
        /// </summary>
        /// <returns>O vetor rotacionado</returns>
        public MMXVector Rotate90()
        {
            return new MMXVector(-y, x);
        }

        /// <summary>
        /// Rotaciona um vetor em 90 graus ao redor de outro vetor
        /// </summary>
        /// <param name="center">Centro de rotação</param>
        /// <returns>O vetor rotacionado</returns>
        public MMXVector Rotate90(MMXVector center)
        {
            return (this - center).Rotate90() + center;
        }

        /// <summary>
        /// Rotaciona um vetor em 180 graus ao redor da origem
        /// </summary>
        /// <returns>O vetor rotacionado</returns>
        public MMXVector Rotate180()
        {
            return new MMXVector(-x, -y);
        }

        /// <summary>
        /// Rotaciona um vetor em 180 graus ao redor de outro vetor
        /// </summary>
        /// <param name="center">Centro de rotação</param>
        /// <returns>O vetor rotacionado</returns>
        public MMXVector Rotate180(MMXVector center)
        {
            return (this - center).Rotate180() + center;
        }

        /// <summary>
        /// Rotaciona um vetor em 270 graus ao redor da origem
        /// </summary>
        /// <returns>O vetor rotacionado</returns>
        public MMXVector Rotate270()
        {
            return new MMXVector(y, -x);
        }

        /// <summary>
        /// Rotaciona um vetor em 270 graus ao redor de outro vetor
        /// </summary>
        /// <param name="center">Centro de rotação</param>
        /// <returns>O vetor rotacionado</returns>
        public MMXVector Rotate270(MMXVector center)
        {
            return (this - center).Rotate270() + center;
        }

        public MMXVector RoundToCeil()
        {
            return new MMXVector(x.RoundToCeil(), y.RoundToCeil());
        }

        public MMXVector RoundToFloor()
        {
            return new MMXVector(x.RoundToFloor(), y.RoundToFloor());
        }

        public MMXVector RoundXToCeil()
        {
            return new MMXVector(x.RoundToCeil(), y);
        }

        public MMXVector RoundXToFloor()
        {
            return new MMXVector(x.RoundToFloor(), y);
        }

        public MMXVector RoundYToCeil()
        {
            return new MMXVector(x, y.RoundToCeil());
        }

        public MMXVector RoundYToFloor()
        {
            return new MMXVector(x, y.RoundToFloor());
        }

        public MMXVector Round(MMXFloat dx, MMXFloat dy)
        {
            return new MMXVector(x.Round(dx), y.Round(dy));
        }

        public MMXVector RoundX(MMXFloat dx)
        {
            return new MMXVector(x.Round(dx), y);
        }

        public MMXVector RoundY(MMXFloat dy)
        {
            return new MMXVector(x, y.Round(dy));
        }

        /// <summary>
        /// Distâcia entre vetores
        /// </summary>
        /// <param name="vec">Vetor no qual será medido a sua distância até este vetor</param>
        /// <returns>A distância entre este vetor e o vetor dado</returns>
        public MMXFloat DistanceTo(MMXVector vec)
        {
            double dx = x - vec.x;
            double dy = y - vec.y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        GeometryType MMXGeometry.GetType()
        {
            return type;
        }

        /// <summary>
        /// Adição de vetores
        /// </summary>
        /// <param name="vec1">Primeiro vetor</param>
        /// <param name="vec2">Segundo vetor</param>
        /// <returns>Soma entre os dois vetores</returns>
        public static MMXVector operator +(MMXVector vec1, MMXVector vec2)
        {
            return new MMXVector(vec1.x + vec2.x, vec1.y + vec2.y);
        }

        /// <summary>
        /// Subtração de vetores
        /// </summary>
        /// <param name="vec1">Primeiro vetor</param>
        /// <param name="vec2">Segundo vetor</param>
        /// <returns>Diferença entre os dois vetores</returns>
        public static MMXVector operator -(MMXVector vec1, MMXVector vec2)
        {
            return new MMXVector(vec1.x - vec2.x, vec1.y - vec2.y);
        }

        /// <summary>
        /// Inverte o sentido do vetor
        /// </summary>
        /// <param name="vec">Vetor</param>
        /// <returns>O oposto do vetor</returns>
        public static MMXVector operator -(MMXVector vec)
        {
            return new MMXVector(-vec.x, -vec.y);
        }

        /// <summary>
        /// Produto de um vetor por um escalar
        /// </summary>
        /// <param name="alpha">Escalar</param>
        /// <param name="vec">Vetor</param>
        /// <returns>O vetor escalado por alpha</returns>
        public static MMXVector operator *(MMXFloat alpha, MMXVector vec)
        {
            return new MMXVector(alpha * vec.x, alpha * vec.y);
        }

        /// <summary>
        /// Produto de um vetor por um escalar
        /// </summary>
        /// <param name="vec">Vetor</param>
        /// <param name="alpha">Escalar</param>
        /// <returns>O vetor escalado por alpha</returns>
        public static MMXVector operator *(MMXVector vec, MMXFloat alpha)
        {
            return new MMXVector(alpha * vec.x, alpha * vec.y);
        }

        /// <summary>
        /// Divisão de vetor por um escalar, o mesmo que multiplicar o vetor pelo inverso do escalar
        /// </summary>
        /// <param name="vec">Vetor</param>
        /// <param name="alpha">Escalar</param>
        /// <returns>O vetor dividido pelo escalar alpha</returns>
        public static MMXVector operator /(MMXVector vec, MMXFloat alpha)
        {
            return new MMXVector(vec.x / alpha, vec.y / alpha);
        }

        /// <summary>
        /// Produto escalar/interno/ponto entre dois vetores
        /// </summary>
        /// <param name="vec1">Primeiro vetor</param>
        /// <param name="vec2">Segundo vetor</param>
        /// <returns>Produto escalar entre os dois vetores</returns>
        public static MMXFloat operator *(MMXVector vec1, MMXVector vec2)
        {
            return vec1.x * vec2.x + vec1.y * vec2.y;
        }

        /// <summary>
        /// Igualdade entre vetores
        /// </summary>
        /// <param name="vec1">Primeiro vetor</param>
        /// <param name="vec2">Segundo vetor</param>
        /// <returns>true se os vetores forem iguais, false caso contrário</returns>
        public static bool operator ==(MMXVector vec1, MMXVector vec2)
        {
            return vec1.x == vec2.x && vec1.y == vec2.y;
        }

        /// <summary>
        /// Inequalidade entre vetores
        /// </summary>
        /// <param name="vec1">Primeiro vetor</param>
        /// <param name="vec2">Segundo vetor</param>
        /// <returns>true se os vetores forem diferentes, false caso contrário</returns>
        public static bool operator !=(MMXVector vec1, MMXVector vec2)
        {
            return vec1.x != vec2.x || vec1.y != vec2.y;
        }
    }

    public struct Interval
    {
        public static readonly Interval EMPTY = MakeOpenInterval(0, 0);

        private MMXFloat min;
        private bool closedLeft;
        private MMXFloat max;
        private bool closedRight;

        public MMXFloat Min
        {
            get
            {
                return min;
            }
        }

        public bool ClosedLeft
        {
            get
            {
                return closedLeft;
            }
        }

        public bool ClosedRight
        {
            get
            {
                return closedRight;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return closedLeft && closedRight ? min > max : min >= max;
            }
        }

        public bool IsPoint
        {
            get
            {
                return min == max;
            }
        }

        private Interval(MMXFloat min, bool closedLeft, MMXFloat max, bool closedRight)
        {
            this.min = min;
            this.closedLeft = closedLeft;
            this.max = max;
            this.closedRight = closedRight;
        }

        public bool Equals(Interval other)
        {
            if (IsEmpty && other.IsEmpty)
                return true;

            return min == other.min && closedLeft == other.closedLeft && max == other.max && closedRight == other.closedRight;
        }

        public bool Contains(MMXFloat element, bool inclusive = true)
        {
            if (!inclusive)
                return min < element && element < max;

            if (closedLeft ? min > element : min >= element)
                return false;

            if (closedRight ? element > max : element >= max)
                return false;

            return true;
        }

        public bool Contains(Interval interval, bool inclusive = true)
        {
            if (interval.IsEmpty)
                return !inclusive ? !IsEmpty : true;

            if (!inclusive)
                return min < interval.min && interval.max < max;

            if (closedLeft ? min > interval.min : interval.closedLeft ? min >= interval.min : min > interval.min)
                return false;

            if (closedRight ? interval.max > max : interval.closedRight ? interval.max >= max : interval.max > max)
                return false;

            return true;
        }

        public Interval Intersection(Interval other)
        {
            if (other.IsEmpty)
                return EMPTY;

            MMXFloat newMin;
            bool newClosedLeft;
            if (min > other.min)
            {
                newMin = min;
                newClosedLeft = closedLeft;
            }
            else if (min < other.min)
            {
                newMin = other.min;
                newClosedLeft = other.closedLeft;
            }
            else
            {
                newMin = min;
                newClosedLeft = closedLeft && other.closedLeft;
            }

            MMXFloat newMax;
            bool newClosedRight;
            if (max < other.max)
            {
                newMax = max;
                newClosedRight = closedRight;
            }
            else if (max > other.max)
            {
                newMax = other.max;
                newClosedRight = other.closedRight;
            }
            else
            {
                newMax = max;
                newClosedRight = closedRight && other.closedRight;
            }

            return new Interval(newMin, newClosedLeft, newMax, newClosedRight);
        }

        public static Interval MakeOpenInterval(MMXFloat v1, MMXFloat v2)
        {
            return new Interval(Math.Min(v1, v2), false, Math.Max(v1, v2), false);
        }

        public static Interval MakeClosedInterval(MMXFloat v1, MMXFloat v2)
        {
            return new Interval(Math.Min(v1, v2), true, Math.Max(v1, v2), true);
        }

        public static Interval MakeSemiOpenLeftInterval(MMXFloat v1, MMXFloat v2)
        {
            return new Interval(Math.Min(v1, v2), false, Math.Max(v1, v2), true);
        }

        public static Interval MakeSemiOpenRightInterval(MMXFloat v1, MMXFloat v2)
        {
            return new Interval(Math.Min(v1, v2), true, Math.Max(v1, v2), false);
        }

        public override string ToString()
        {
            return (closedLeft ? "[" : "(") + min + ";" + max + (closedRight ? "]" : ")");
        }

        public static bool operator ==(Interval left, Interval right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Interval left, Interval right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// Segmento de reta
    /// </summary>
    public struct MMXLineSegment : MMXGeometry
    {
        public const GeometryType type = GeometryType.LINE_SEGMENT;

        public static readonly MMXLineSegment NULL_SEGMENT = new MMXLineSegment(MMXVector.NULL_VECTOR, MMXVector.NULL_VECTOR);

        private MMXVector start; // Ponto inicial do segmento
        private MMXVector end; // Ponto final do segmento

        /// <summary>
        /// Cria um segmento de reta a partir de dois pontos
        /// </summary>
        /// <param name="start">Ponto inicial do segmento</param>
        /// <param name="end">Ponto final do segmento</param>
        public MMXLineSegment(MMXVector start, MMXVector end)
        {
            this.start = start;
            this.end = end;
        }

        /// <summary>
        /// Ponto inicial do segmento
        /// </summary>
        public MMXVector Start
        {
            get
            {
                return start;
            }
        }

        /// <summary>
        /// Ponto final do segmento
        /// </summary>
        public MMXVector End
        {
            get
            {
                return end;
            }
        }

        /// <summary>
        /// Comprimento do segmento
        /// </summary>
        public MMXFloat Length
        {
            get
            {
                return (end - start).Length;
            }
        }

        /// <summary>
        /// Inverte o sentido do segmento trocando seu ponto inicial com seu ponto final
        /// </summary>
        /// <returns>O segmento de reta invertido</returns>
        public MMXLineSegment Negate()
        {
            return new MMXLineSegment(end, start);
        }

        /// <summary>
        /// Rotaciona um segmento de reta ao redor de um ponto
        /// </summary>
        /// <param name="origin">Centro de rotação</param>
        /// <param name="angle">Algumo de rotação em radianos</param>
        /// <returns>Segmento de reta rotacionado</returns>
        public MMXLineSegment Rotate(MMXVector origin, MMXFloat angle)
        {
            MMXVector u = start.Rotate(origin, angle);
            MMXVector v = end.Rotate(origin, angle);
            return new MMXLineSegment(u, v);
        }

        /// <summary>
        /// Rotaciona um segmento de reta em 90 graus ao redor de um ponto
        /// </summary>
        /// <param name="origin">Centro de rotação</param>
        /// <returns>Segmento de reta rotacionado</returns>
        public MMXLineSegment Rotate90(MMXVector origin)
        {
            MMXVector u = start.Rotate90(origin);
            MMXVector v = end.Rotate90(origin);
            return new MMXLineSegment(u, v);
        }

        /// <summary>
        /// Rotaciona um segmento de reta em 180 graus ao redor de um ponto
        /// </summary>
        /// <param name="origin">Centro de rotação</param>
        /// <returns>Segmento de reta rotacionado</returns>
        public MMXLineSegment Rotate180(MMXVector origin)
        {
            MMXVector u = start.Rotate180(origin);
            MMXVector v = end.Rotate180(origin);
            return new MMXLineSegment(u, v);
        }

        /// <summary>
        /// Rotaciona um segmento de reta em 270 graus ao redor de um ponto
        /// </summary>
        /// <param name="origin">Centro de rotação</param>
        /// <returns>Segmento de reta rotacionado</returns>
        public MMXLineSegment Rotate270(MMXVector origin)
        {
            MMXVector u = start.Rotate270(origin);
            MMXVector v = end.Rotate270(origin);
            return new MMXLineSegment(u, v);
        }

        /// <summary>
        /// Compara a posição de um vetor em relação ao segmento de reta
        /// </summary>
        /// <param name="v">Vetor a ser testado</param>
        /// <returns>1 se o vetor está a esquerda do segmento, -1 se estiver a direta, 0 se for colinear ao segmento</returns>
        public int Compare(MMXVector v)
        {
            MMXFloat f = (v.Y - start.Y) * (end.X - start.X) - (v.X - start.X) * (end.Y - start.Y);

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
        public bool Contains(MMXVector v)
        {
            if (Compare(v) != 0)
                return false;

            MMXFloat mX = Math.Min(start.X, end.X);
            MMXFloat MX = Math.Max(start.X, end.X);
            MMXFloat mY = Math.Min(start.Y, end.Y);
            MMXFloat MY = Math.Max(start.Y, end.Y);

            return mX <= v.X && v.X <= MX && mY <= v.Y && v.Y <= MY;
        }

        /// <summary>
        /// Verifica se dois segmentos de reta são paralelos
        /// </summary>
        /// <param name="s">Segmento a ser testado</param>
        /// <returns>true se forem paralelos, false caso contrário</returns>
        public bool IsParallel(MMXLineSegment s)
        {
            MMXFloat A1 = end.Y - start.Y;
            MMXFloat B1 = end.X - start.X;
            MMXFloat A2 = s.end.Y - s.start.Y;
            MMXFloat B2 = s.end.X - s.start.X;

            return A1 * B2 == A2 * B1;
        }

        /// <summary>
        /// Obtém a intersecção entre dois segmentos de reta
        /// </summary>
        /// <param name="s">Segmento de reta a ser testado</param>
        /// <returns>A intersecção entre os dois segmentos caso ela exista, ou retorna conjunto vazio caso contrário</returns>
        public GeometryType Intersection(MMXLineSegment s, out MMXVector resultVector, out MMXLineSegment resultLineSegment)
        {
            resultVector = MMXVector.NULL_VECTOR;
            resultLineSegment = MMXLineSegment.NULL_SEGMENT;

            if (s == this)
            {
                resultLineSegment = this;
                return GeometryType.LINE_SEGMENT;
            }

            MMXFloat A1 = end.Y - start.Y;
            MMXFloat B1 = end.X - start.X;
            MMXFloat A2 = s.end.Y - s.start.Y;
            MMXFloat B2 = s.end.X - s.start.X;

            MMXFloat D = A1 * B2 - A2 * B1;

            MMXFloat C1 = start.X * end.Y - end.X * start.Y;
            MMXFloat C2 = s.start.X * s.end.Y - s.end.X * s.start.Y;

            if (D == 0)
            {
                if (C1 != 0 || C2 != 0)
                    return GeometryType.EMPTY;

                MMXFloat xmin = MMXFloat.Max(start.X, s.start.X);
                MMXFloat ymin = MMXFloat.Max(start.Y, s.start.Y);
                MMXFloat xmax = MMXFloat.Min(end.X, s.end.X);
                MMXFloat ymax = MMXFloat.Min(end.Y, s.end.Y);

                if (xmin < xmax)
                {
                    resultLineSegment = new MMXLineSegment(new MMXVector(xmin, ymin), new MMXVector(xmax, ymax));
                    return GeometryType.LINE_SEGMENT;
                }

                if (xmin == xmax)
                {
                    resultVector = new MMXVector(xmin, ymin);
                    return GeometryType.VECTOR;
                }

                return GeometryType.EMPTY;
            }            

            MMXFloat x = (B2 * C1 - B1 * C2) / D;
            MMXFloat y = (A2 * C1 - A1 * C2) / D;
            MMXVector v = new MMXVector(x, y);

            if (!Contains(v))
                return GeometryType.EMPTY;

            resultVector = v;
            return GeometryType.VECTOR;
        }

        public MMXBox WrappingBox()
        {
            return new MMXBox(start, MMXVector.NULL_VECTOR, end - start);
        }

        GeometryType MMXGeometry.GetType()
        {
            return type;
        }

        /// <summary>
        /// Verifica se o vetor v está a direita do seguimento de reta s
        /// </summary>
        /// <param name="v">Vetor</param>
        /// <param name="s">Seguimento de reta</param>
        /// <returns>Resultado da comparação</returns>
        public static bool operator <(MMXVector v, MMXLineSegment s)
        {
            return s.Compare(v) == -1;
        }

        /// <summary>
        /// Verifica se o vetor v está a direita ou é colinear ao seguimento de reta s
        /// </summary>
        /// <param name="v">Vetor</param>
        /// <param name="s">Seguimento de reta</param>
        /// <returns>Resultado da comparação</returns>
        public static bool operator <=(MMXVector v, MMXLineSegment s)
        {
            return s.Compare(v) <= 0;
        }

        /// <summary>
        /// Verifica se o vetor v está a esquerda do seguimento de reta s
        /// </summary>
        /// <param name="v">Vetor</param>
        /// <param name="s">Seguimento de reta</param>
        /// <returns>Resultado da comparação</returns>
        public static bool operator >(MMXVector v, MMXLineSegment s)
        {
            return s.Compare(v) == 1;
        }

        /// <summary>
        /// Verifica se o vetor v está a esquerda ou é colinear ao seguimento de reta s
        /// </summary>
        /// <param name="v">Vetor</param>
        /// <param name="s">Seguimento de reta</param>
        /// <returns>Resultado da comparação</returns>
        public static bool operator >=(MMXVector v, MMXLineSegment s)
        {
            return s.Compare(v) >= 0;
        }

        /// <summary>
        /// O mesmo que v > s
        /// </summary>
        /// <param name="s">Seguimento de reta</param>
        /// <param name="v">Vetor</param>
        /// <returns>v > s</returns>
        public static bool operator <(MMXLineSegment s, MMXVector v)
        {
            return v > s;
        }

        /// <summary>
        /// O mesmo que v >= s
        /// </summary>
        /// <param name="s">Seguimento de reta</param>
        /// <param name="v">Vetor</param>
        /// <returns>v >= s</returns>
        public static bool operator <=(MMXLineSegment s, MMXVector v)
        {
            return v >= s;
        }

        /// <summary>
        /// O mesmo que v < s
        /// </summary>
        /// <param name="s">Seguimento de reta</param>
        /// <param name="v">Vetor</param>
        /// <returns>v < s</returns>
        public static bool operator >(MMXLineSegment s, MMXVector v)
        {
            return v < s;
        }

        /// <summary>
        /// O mesmo que v <= s
        /// </summary>
        /// <param name="s">Seguimento de reta</param>
        /// <param name="v">Vetor</param>
        /// <returns>v <= s</returns>
        public static bool operator >=(MMXLineSegment s, MMXVector v)
        {
            return v <= s;
        }

        /// <summary>
        /// Compara se dois seguimentos de reta são iguais
        /// </summary>
        /// <param name="s1">Primeiro seguimento de reta</param>
        /// <param name="s2">Seguindo seguimento de reta</param>
        /// <returns>true se forem iguais, false caso contrário</returns>
        public static bool operator ==(MMXLineSegment s1, MMXLineSegment s2)
        {
            return s1.start == s2.start && s1.end == s2.end;
        }

        /// <summary>
        /// Compara se dois seguimentos de reta são diferentes
        /// </summary>
        /// <param name="s1">Primeiro seguimento de reta</param>
        /// <param name="s2">Seguindo seguimento de reta</param>
        /// <returns>true se forem diferentes, false caso contrário</returns>
        public static bool operator !=(MMXLineSegment s1, MMXLineSegment s2)
        {
            return s1.start != s2.start || s1.end != s2.end;
        }
    }

    /// <summary>
    /// Uma matriz quadrada de ordem 2
    /// </summary>
    public struct Matrix2x2
    {
        /// <summary>
        /// Matriz nula
        /// </summary>
        public static readonly Matrix2x2 NULL_MATRIX = new Matrix2x2();
        /// <summary>
        /// Matriz identidade
        /// </summary>
        public static readonly Matrix2x2 IDENTITY = new Matrix2x2(1, 0, 0, 1);

        private MMXFloat elements00;
        private MMXFloat elements01;
        private MMXFloat elements10;
        private MMXFloat elements11;

        /// <summary>
        /// Cria uma matriz a partir de um array de valores numéricos
        /// </summary>
        /// <param name="values"></param>
        public Matrix2x2(params MMXFloat[] values)
        {
            elements00 = values[0];
            elements01 = values[1];
            elements10 = values[2];
            elements11 = values[3];
        }

        /// <summary>
        /// Cria uma matriz a partir de dois vetores
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public Matrix2x2(MMXVector v1, MMXVector v2)
        {
            elements00 = v1.X;
            elements01 = v1.Y;
            elements10 = v2.X;
            elements11 = v2.Y;
        }

        public MMXFloat Element00
        {
            get
            {
                return elements00;
            }
        }

        public MMXFloat Element01
        {
            get
            {
                return elements01;
            }
        }

        public MMXFloat Element10
        {
            get
            {
                return elements10;
            }
        }

        public MMXFloat Element11
        {
            get
            {
                return elements11;
            }
        }

        /// <summary>
        /// Calcula o determinante da uma matriz
        /// </summary>
        /// <returns>Determinante</returns>
        public MMXFloat Determinant()
        {
            return elements00 * elements11 - elements10 * elements01;
        }

        /// <summary>
        /// Transpõe a matriz
        /// </summary>
        /// <returns>Transposta da matriz</returns>
        public Matrix2x2 Transpose()
        {
            return new Matrix2x2(elements00, elements10, elements01, elements11);
        }

        /// <summary>
        /// Inverte a matriz
        /// </summary>
        /// <returns>Inversa da matriz</returns>
        public Matrix2x2 Inverse()
        {
            return new Matrix2x2(elements11, -elements01, -elements10, elements00) / Determinant();
        }

        /// <summary>
        /// Soma entre duas matrizes
        /// </summary>
        /// <param name="m1">Primeira matriz</param>
        /// <param name="m2">Segunda matriz</param>
        /// <returns>Soma</returns>
        public static Matrix2x2 operator +(Matrix2x2 m1, Matrix2x2 m2)
        {
            return new Matrix2x2(m1.elements00 + m2.elements00, m1.elements01 + m2.elements01, m1.elements10 + m2.elements10, m1.elements11 + m2.elements11);
        }

        /// <summary>
        /// Diferença/Subtração entre duas matrizes
        /// </summary>
        /// <param name="m1">Primeira matriz</param>
        /// <param name="m2">Segunda matriz</param>
        /// <returns>Diferença</returns>
        public static Matrix2x2 operator -(Matrix2x2 m1, Matrix2x2 m2)
        {
            return new Matrix2x2(m1.elements00 - m2.elements00, m1.elements01 - m2.elements01, m1.elements10 - m2.elements10, m1.elements11 - m2.elements11);
        }

        /// <summary>
        /// Oposto aditivo de uma matriz
        /// </summary>
        /// <param name="m">Matriz</param>
        /// <returns>Oposto</returns>
        public static Matrix2x2 operator -(Matrix2x2 m)
        {
            return new Matrix2x2(-m.elements00, -m.elements01, -m.elements10, -m.elements11);
        }

        /// <summary>
        /// Produto de uma matriz por um escalar
        /// </summary>
        /// <param name="factor">Escalar</param>
        /// <param name="m">Matriz</param>
        /// <returns>Produto</returns>
        public static Matrix2x2 operator *(MMXFloat factor, Matrix2x2 m)
        {
            return new Matrix2x2(factor * m.elements00, factor * m.elements01, factor * m.elements10, factor * m.elements11);
        }

        /// <summary>
        /// Produto de uma matriz por um escalar
        /// </summary>
        /// <param name="m">Matriz</param>
        /// <param name="factor">Escalar</param>
        /// <returns>Produto</returns>
        public static Matrix2x2 operator *(Matrix2x2 m, MMXFloat factor)
        {
            return new Matrix2x2(m.elements00 * factor, m.elements01 * factor, m.elements10 * factor, m.elements11 * factor);
        }

        /// <summary>
        /// Divisão de uma matriz por um escalar
        /// </summary>
        /// <param name="m">Matriz</param>
        /// <param name="divisor">Escalar</param>
        /// <returns>Divisão</returns>
        public static Matrix2x2 operator /(Matrix2x2 m, MMXFloat divisor)
        {
            return new Matrix2x2(m.elements00 / divisor, m.elements01 / divisor, m.elements10 / divisor, m.elements11 / divisor);
        }

        /// <summary>
        /// Produto entre duas matrizes
        /// </summary>
        /// <param name="m1">Primeira matriz</param>
        /// <param name="m2">Segunda matriz</param>
        /// <returns>Produto matricial</returns>
        public static Matrix2x2 operator *(Matrix2x2 m1, Matrix2x2 m2)
        {
            return new Matrix2x2(m1.elements00 * m2.elements00 + m1.elements01 * m2.elements10, m1.elements00 * m2.elements01 + m1.elements01 * m2.elements11,
                                 m1.elements10 * m2.elements00 + m1.elements11 * m2.elements10, m1.elements10 * m2.elements01 + m1.elements11 * m2.elements11);
        }

        /// <summary>
        /// Calcula a matriz de rotação a partir de um angulo dado
        /// </summary>
        /// <param name="angle">Angulo em radianos</param>
        /// <returns>Matriz de rotação</returns>
        public static Matrix2x2 RotationMatrix(MMXFloat angle)
        {
            MMXFloat cos = Math.Cos(angle);
            MMXFloat sin = Math.Sin(angle);
            return new Matrix2x2(cos, -sin, sin, cos);
        }
    }

    public interface MMXShape : MMXGeometry
    {
        MMXFloat Area();
    }

    public enum BoxSide
    {
        LEFT = 0,
        UP = 1,
        RIGHT = 2,
        DOWN = 3
    }

    /// <summary>
    /// Retângulo bidimensional com lados paralelos aos eixos coordenados
    /// </summary>
    public struct MMXBox : MMXShape
    {
        public const GeometryType type = GeometryType.BOX;

        /// <summary>
        /// Retângulo vazio
        /// </summary>
        public static readonly MMXBox EMPTY_BOX = new MMXBox();
        /// <summary>
        /// Retângulo universo
        /// </summary>
        public static readonly MMXBox UNIVERSE_BOX = new MMXBox(MMXVector.NULL_VECTOR, MMXVector.NULL_VECTOR, new MMXVector(MMXFloat.MAX_VALUE, MMXFloat.MAX_VALUE));

        private MMXVector origin; // origen
        private MMXVector mins; // mínimos
        private MMXVector maxs; // máximos

        /// <summary>
        /// Cria um retângulo vazio com uma determinada origem
        /// </summary>
        /// <param name="origin">origem do retângulo</param>
        public MMXBox(MMXVector origin)
        {
            this.origin = origin;
            mins = MMXVector.NULL_VECTOR;
            maxs = MMXVector.NULL_VECTOR;
        }

        /// <summary>
        /// Cria um retângulo a partir da origem, mínimos e máximos
        /// </summary>
        /// <param name="origin">Origem</param>
        /// <param name="mins">Mínimos</param>
        /// <param name="maxs">Máximos</param>
        public MMXBox(MMXVector origin, MMXVector mins, MMXVector maxs)
        {
            this.origin = origin;
            this.mins = mins;
            this.maxs = maxs;
        }

        public MMXBox(MMXFloat x, MMXFloat y, MMXFloat width, MMXFloat height)
        {
            origin = new MMXVector(x, y);
            mins = MMXVector.NULL_VECTOR;
            maxs = new MMXVector(width, height);
        }

        public MMXBox(MMXFloat x, MMXFloat y, MMXFloat left, MMXFloat top, MMXFloat width, MMXFloat height)
        {
            origin = new MMXVector(x, y);
            mins = new MMXVector(left - x, top - y);
            maxs = new MMXVector(left + width - x, top + height - y);
        }

        public MMXBox(BinaryReader reader)
        {
            origin = new MMXVector(reader);
            mins = new MMXVector(reader);
            maxs = new MMXVector(reader);
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
        public MMXBox Truncate()
        {
            MMXVector mins = origin + this.mins;
            mins = new MMXVector(Math.Floor(mins.X), Math.Floor(mins.Y));
            MMXVector maxs = origin + this.maxs;
            maxs = new MMXVector(Math.Floor(maxs.X), Math.Floor(maxs.Y));
            return new MMXBox(mins, MMXVector.NULL_VECTOR, maxs - mins);
        }

        public override int GetHashCode()
        {
            MMXVector m = origin + mins;
            MMXVector M = origin + maxs;

            return 31 * m.GetHashCode() + M.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is MMXBox))
                return false;

            MMXBox other = (MMXBox) obj;

            MMXVector m1 = origin + mins;
            MMXVector M1 = origin + maxs;

            MMXVector m2 = other.origin + other.mins;
            MMXVector M2 = other.origin + other.maxs;

            return m1 == m2 && M1 == M2;
        }

        public override string ToString()
        {
            return "[" + origin + ":" + mins + ":" + maxs + "]";
        }

        /// <summary>
        /// Origem do retângulo
        /// </summary>
        public MMXVector Origin
        {
            get
            {
                return origin;
            }
        }

        public MMXFloat X
        {
            get
            {
                return origin.X;
            }
        }

        public MMXFloat Y
        {
            get
            {
                return origin.Y;
            }
        }

        public MMXFloat Left
        {
            get
            {
                return Math.Min(origin.X + mins.X, origin.X + maxs.X);
            }
        }

        public MMXFloat Top
        {
            get
            {
                return Math.Min(origin.Y + mins.Y, origin.Y + maxs.Y);
            }
        }

        public MMXFloat Right
        {
            get
            {
                return Math.Max(origin.X + mins.X, origin.X + maxs.X);
            }
        }

        public MMXFloat Bottom
        {
            get
            {
                return Math.Max(origin.Y + mins.Y, origin.Y + maxs.Y);
            }
        }

        public MMXLineSegment LeftSegment
        {
            get
            {
                return new MMXLineSegment(LeftTop, LeftBottom);
            }
        }

        public MMXLineSegment TopSegment
        {
            get
            {
                return new MMXLineSegment(LeftTop, RightTop);
            }
        }

        public MMXLineSegment RightSegment
        {
            get
            {
                return new MMXLineSegment(RightTop, RightBottom);
            }
        }

        public MMXLineSegment BottomSegment
        {
            get
            {
                return new MMXLineSegment(LeftBottom, RightBottom);
            }
        }

        /// <summary>
        /// Extremo superior esquerdo do retângulo (ou mínimos absolutos)
        /// </summary>
        public MMXVector LeftTop
        {
            get
            {
                return new MMXVector(Left, Top);
            }
        }

        public MMXVector LeftMiddle
        {
            get
            {
                return new MMXVector(Left, (Top + Bottom) / 2);
            }
        }

        public MMXVector LeftBottom
        {
            get
            {
                return new MMXVector(Left, Bottom);
            }
        }

        public MMXVector RightTop
        {
            get
            {
                return new MMXVector(Right, Top);
            }
        }

        public MMXVector RightMiddle
        {
            get
            {
                return new MMXVector(Right, (Top + Bottom) / 2);
            }
        }

        public MMXVector MiddleTop
        {
            get
            {
                return new MMXVector((Left + Right) / 2, Top);
            }
        }

        public MMXVector MiddleBottom
        {
            get
            {
                return new MMXVector((Left + Right) / 2, Bottom);
            }
        }

        /// <summary>
        /// Extremo inferior direito do retângulo (ou máximos absolutos)
        /// </summary>
        public MMXVector RightBottom
        {
            get
            {
                return new MMXVector(Right, Bottom);
            }
        }

        public MMXVector Center
        {
            get
            {
                return origin + (mins + maxs) / 2;
            }
        }

        /// <summary>
        /// Mínimos relativos
        /// </summary>
        public MMXVector Mins
        {
            get
            {
                return mins;
            }
        }

        /// <summary>
        /// Máximos relativos
        /// </summary>
        public MMXVector Maxs
        {
            get
            {
                return maxs;
            }
        }

        public MMXVector WidthVector
        {
            get
            {
                return new MMXVector(Width, 0);
            }
        }

        public MMXVector HeightVector
        {
            get
            {
                return new MMXVector(0, Height);
            }
        }

        /// <summary>
        /// Vetor correspondente ao tamanho do retângulo contendo sua largura (width) na coordenada x e sua altura (height) na coordenada y
        /// </summary>
        public MMXVector DiagonalVector
        {
            get
            {
                return new MMXVector(Width, Height);
            }
        }

        /// <summary>
        /// Largura (base) do retângulo
        /// </summary>
        public MMXFloat Width
        {
            get
            {
                return (maxs.X - mins.X).Abs;
            }
        }

        /// <summary>
        /// Altura do retângulo
        /// </summary>
        public MMXFloat Height
        {
            get
            {
                return (maxs.Y - mins.Y).Abs;
            }
        }

        public MMXBox LeftTopOrigin()
        {
            return new MMXBox(LeftTop, MMXVector.NULL_VECTOR, DiagonalVector);
        }

        public MMXBox RightBottomOrigin()
        {
            return new MMXBox(RightBottom, -DiagonalVector, MMXVector.NULL_VECTOR);
        }

        public MMXBox CenterOrigin()
        {
            MMXVector sv2 = DiagonalVector / 2;
            return new MMXBox(Center, -sv2, sv2);
        }

        public MMXBox RoundOriginToCeil()
        {
            return new MMXBox(origin.RoundToCeil(), mins, maxs);
        }

        public MMXBox RoundOriginXToCeil()
        {
            return new MMXBox(origin.RoundXToCeil(), mins, maxs);
        }

        public MMXBox RoundOriginYToCeil()
        {
            return new MMXBox(origin.RoundYToCeil(), mins, maxs);
        }

        public MMXBox RoundOriginToFloor()
        {
            return new MMXBox(origin.RoundToFloor(), mins, maxs);
        }

        public MMXBox RoundOriginXToFloor()
        {
            return new MMXBox(origin.RoundXToFloor(), mins, maxs);
        }

        public MMXBox RoundOriginYToFloor()
        {
            return new MMXBox(origin.RoundYToFloor(), mins, maxs);
        }

        public MMXBox RoundOrigin(MMXFloat dx, MMXFloat dy)
        {
            return new MMXBox(origin.Round(dx, dy), mins, maxs);
        }

        public MMXBox RoundOriginX(MMXFloat dx)
        {
            return new MMXBox(origin.RoundX(dx), mins, maxs);
        }

        public MMXBox RoundOriginY(MMXFloat dy)
        {
            return new MMXBox(origin.RoundY(dy), mins, maxs);
        }

        /// <summary>
        /// Área do retângulo
        /// </summary>
        /// <returns></returns>
        public MMXFloat Area()
        {
            return Width * Height;
        }

        /// <summary>
        /// Escala o retângulo para a esquerda
        /// </summary>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public MMXBox ScaleLeft(MMXFloat alpha)
        {
            MMXFloat width = Width;
            return new MMXBox(LeftTop + alpha * (width - 1) * MMXVector.LEFT_VECTOR, MMXVector.NULL_VECTOR, new MMXVector(alpha * width, Height));
        }

        public MMXBox ClipLeft(MMXFloat clip)
        {
            return new MMXBox(origin, new MMXVector(mins.X + clip, mins.Y), maxs);
        }

        /// <summary>
        /// Escala o retângulo para a direita
        /// </summary>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public MMXBox ScaleRight(MMXFloat alpha)
        {
            return new MMXBox(LeftTop, MMXVector.NULL_VECTOR, new MMXVector(alpha * Width, Height));
        }

        public MMXBox ClipRight(MMXFloat clip)
        {
            return new MMXBox(origin, mins, new MMXVector(maxs.X - clip, maxs.Y));
        }

        /// <summary>
        /// Escala o retângulo para cima
        /// </summary>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public MMXBox ScaleTop(MMXFloat alpha)
        {
            MMXFloat height = Height;
            return new MMXBox(LeftTop + alpha * (height - 1) * MMXVector.UP_VECTOR, MMXVector.NULL_VECTOR, new MMXVector(Width, alpha * height));
        }

        public MMXBox ClipTop(MMXFloat clip)
        {
            return new MMXBox(origin, new MMXVector(mins.X, mins.Y + clip), maxs);
        }

        /// <summary>
        /// Escala o retângulo para baixo
        /// </summary>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public MMXBox ScaleBottom(MMXFloat alpha)
        {
            return new MMXBox(LeftTop, MMXVector.NULL_VECTOR, new MMXVector(Width, alpha * Height));
        }

        public MMXBox ClipBottom(MMXFloat clip)
        {
            return new MMXBox(origin, mins, new MMXVector(maxs.X, maxs.Y - clip));
        }

        public MMXBox Mirror()
        {
            return Mirror(0);
        }

        public MMXBox Flip()
        {
            return Flip(0);
        }

        public MMXBox Mirror(MMXFloat x)
        {
            MMXFloat originX = origin.X;
            x += originX;
            MMXFloat minsX = originX + mins.X;
            MMXFloat maxsX = originX + maxs.X;

            MMXFloat newMinsX = 2 * x - maxsX;
            MMXFloat newMaxsX = 2 * x - minsX;

            return new MMXBox(origin, new MMXVector(newMinsX - originX, mins.Y), new MMXVector(newMaxsX - originX, maxs.Y));
        }

        public MMXBox Flip(MMXFloat y)
        {
            MMXFloat originY = origin.Y;
            y += originY;
            MMXFloat minsY = originY + mins.Y;
            MMXFloat maxsY = originY + maxs.Y;

            MMXFloat newMinsY = 2 * y - maxsY;
            MMXFloat newMaxsY = 2 * y - minsY;

            return new MMXBox(origin, new MMXVector(mins.X, newMinsY - originY), new MMXVector(maxs.X, newMaxsY - originY));
        }

        public MMXVector GetNormal(BoxSide side)
        {
            switch (side)
            {
                case BoxSide.LEFT:
                    return MMXVector.RIGHT_VECTOR;

                case BoxSide.UP:
                    return MMXVector.DOWN_VECTOR;

                case BoxSide.RIGHT:
                    return MMXVector.LEFT_VECTOR;

                case BoxSide.DOWN:
                    return MMXVector.UP_VECTOR;
            }

            return MMXVector.NULL_VECTOR;
        }

        public MMXLineSegment GetSideSegment(BoxSide side)
        {
            switch (side)
            {
                case BoxSide.LEFT:
                    return new MMXLineSegment(LeftTop, LeftBottom);

                case BoxSide.UP:
                    return new MMXLineSegment(LeftTop, RightTop);

                case BoxSide.RIGHT:
                    return new MMXLineSegment(RightTop, RightBottom);

                case BoxSide.DOWN:
                    return new MMXLineSegment(LeftBottom, RightBottom);
            }

            return MMXLineSegment.NULL_SEGMENT;
        }

        public MMXBox HalfLeft()
        {
            return new MMXBox(origin, mins, new MMXVector((mins.X + maxs.X) / 2, maxs.Y));
        }

        public MMXBox HalfTop()
        {
            return new MMXBox(origin, mins, new MMXVector(maxs.X, (mins.Y + maxs.Y) / 2));
        }

        public MMXBox HalfRight()
        {
            return new MMXBox(origin, new MMXVector((mins.X + maxs.X) / 2, mins.Y), maxs);
        }

        public MMXBox HalfBottom()
        {
            return new MMXBox(origin, new MMXVector(mins.X, (mins.Y + maxs.Y) / 2), maxs);
        }

        GeometryType MMXGeometry.GetType()
        {
            return type;
        }

        /// <summary>
        /// Translata um retângulo na direção de um vetor
        /// </summary>
        /// <param name="box">Retângulo</param>
        /// <param name="vec">Vetor</param>
        /// <returns>Retângulo box translatado na direção de vec</returns>
        public static MMXBox operator +(MMXBox box, MMXVector vec)
        {
            return new MMXBox(box.origin + vec, box.mins, box.maxs);
        }

        /// <summary>
        /// Translata um retângulo na direção de um vetor
        /// </summary>
        /// <param name="vec">Vetor</param>
        /// /// <param name="box">Retângulo</param>
        /// <returns>Retângulo box translatado na direção de vec</returns>

        public static MMXBox operator +(MMXVector vec, MMXBox box)
        {
            return new MMXBox(box.origin + vec, box.mins, box.maxs);
        }

        /// <summary>
        /// Translata um retângulo na direção oposta de um vetor
        /// </summary>
        /// <param name="box">Retângulo</param>
        /// <param name="vec">Vetor</param>
        /// <returns>Retângulo box translatado na direção oposta de vec</returns>
        public static MMXBox operator -(MMXBox box, MMXVector vec)
        {
            return new MMXBox(box.origin - vec, box.mins, box.maxs);
        }

        /// <summary>
        /// Escala um retângulo
        /// </summary>
        /// <param name="factor">Fator de escala</param>
        /// <param name="box">Retângulo</param>
        /// <returns>Retângulo com suas coordenadas e dimensões escaladas por factor</returns>
        public static MMXBox operator *(MMXFloat factor, MMXBox box)
        {
            MMXVector m = box.origin + box.mins;
            return new MMXBox(m * factor, MMXVector.NULL_VECTOR, box.DiagonalVector * factor);
        }

        /// <summary>
        /// Escala um retângulo
        /// </summary>
        /// <param name="box">Retângulo</param>
        /// /// <param name="factor">Fator de escala</param>
        /// <returns>Retângulo com suas coordenadas e dimensões escaladas por factor</returns>
        public static MMXBox operator *(MMXBox box, MMXFloat factor)
        {
            MMXVector m = box.origin + box.mins;
            return new MMXBox(m * factor, MMXVector.NULL_VECTOR, box.DiagonalVector * factor);
        }

        /// <summary>
        /// Escala um retângulo inversamente (escala pelo inverso do divisor)
        /// </summary>
        /// <param name="box">Retângulo</param>
        /// <param name="divisor">Divisor</param>
        /// <returns>Retângulo com suas coordenadas e dimensões divididas por divisor</returns>
        public static MMXBox operator /(MMXBox box, MMXFloat divisor)
        {
            MMXVector m = box.origin + box.mins;
            return new MMXBox(m / divisor, MMXVector.NULL_VECTOR, box.DiagonalVector / divisor);
        }

        /// <summary>
        /// Faz a união entre dois retângulos que resultará no menor retângulo que contém os retângulos dados
        /// </summary>
        /// <param name="box1">Primeiro retângulo</param>
        /// <param name="box2">Segundo retângulo</param>
        /// <returns>Menor retângulo que contém os dois retângulos dados</returns>
        public static MMXBox operator |(MMXBox box1, MMXBox box2)
        {
            MMXVector m1 = box1.origin + box1.mins;
            MMXVector M1 = box1.origin + box1.maxs;

            MMXVector m2 = box2.origin + box2.mins;
            MMXVector M2 = box2.origin + box2.maxs;

            MMXFloat minsX = Math.Min(m1.X, m2.X);
            MMXFloat maxsX = Math.Max(M1.X, M2.X);

            MMXFloat minsY = Math.Min(m1.Y, m2.Y);
            MMXFloat maxsY = Math.Max(M1.Y, M2.Y);

            return new MMXBox(new MMXVector(minsX, minsY), MMXVector.NULL_VECTOR, new MMXVector(maxsX - minsX, maxsY - minsY));
        }

        /// <summary>
        /// Faz a intersecção de dois retângulos que resultará em um novo retângulo que esteja contido nos dois retângulos dados. Se os dois retângulos dados forem disjuntos então o resultado será um vetor nulo.
        /// </summary>
        /// <param name="box1">Primeiro retângulo</param>
        /// <param name="box2">Segundo retângulo</param>
        /// <returns>Interesecção entre os dois retângulos dados ou um vetor nulo caso a intersecção seja um conjunto vazio</returns>
        public static MMXBox operator &(MMXBox box1, MMXBox box2)
        {
            MMXVector m1 = box1.origin + box1.mins;
            MMXVector M1 = box1.origin + box1.maxs;

            MMXVector m2 = box2.origin + box2.mins;
            MMXVector M2 = box2.origin + box2.maxs;

            MMXFloat minsX = Math.Max(m1.X, m2.X);
            MMXFloat maxsX = Math.Min(M1.X, M2.X);

            if (maxsX < minsX)
                return EMPTY_BOX;

            MMXFloat minsY = Math.Max(m1.Y, m2.Y);
            MMXFloat maxsY = Math.Min(M1.Y, M2.Y);

            if (maxsY < minsY)
                return EMPTY_BOX;

            return new MMXBox(new MMXVector(minsX, minsY), MMXVector.NULL_VECTOR, new MMXVector(maxsX - minsX, maxsY - minsY));
        }

        public static MMXLineSegment operator &(MMXBox box, MMXLineSegment line)
        {
            if (line.Start <= box)
            {                
                if (line.End <= box)
                    return line;

                for (BoxSide side = BoxSide.LEFT; side <= BoxSide.DOWN; side++)
                {
                    MMXLineSegment sideSegment = box.GetSideSegment(side);
                    if (sideSegment.Contains(line.Start))
                        continue;

                    GeometryType type = line.Intersection(sideSegment, out MMXVector v, out MMXLineSegment l);
                    switch (type)
                    {
                        case GeometryType.VECTOR:
                            return new MMXLineSegment(line.Start, v);

                        case GeometryType.LINE_SEGMENT:
                            return l;
                    }
                }

                return MMXLineSegment.NULL_SEGMENT;
            }

            if (line.End <= box)
            {
                if(line.Start <= box)
                    return line;

                for (BoxSide side = BoxSide.LEFT; side <= BoxSide.DOWN; side++)
                {
                    MMXLineSegment sideSegment = box.GetSideSegment(side);
                    if (sideSegment.Contains(line.End))
                        continue;

                    GeometryType type = line.Intersection(sideSegment, out MMXVector v, out MMXLineSegment l);
                    switch (type)
                    {
                        case GeometryType.VECTOR:
                            return new MMXLineSegment(line.End, v);

                        case GeometryType.LINE_SEGMENT:
                            return l;
                    }
                }

                return MMXLineSegment.NULL_SEGMENT;
            }

            MMXVector v1 = MMXVector.NULL_VECTOR;
            for (BoxSide side = BoxSide.LEFT; side <= BoxSide.DOWN; side++)
            {
                GeometryType type = line.Intersection(box.GetSideSegment(side), out v1, out MMXLineSegment l);
                switch (type)
                {
                    case GeometryType.EMPTY:
                        return MMXLineSegment.NULL_SEGMENT;

                    case GeometryType.VECTOR:
                        return new MMXLineSegment(line.End, v1);

                    case GeometryType.LINE_SEGMENT:
                        return l;
                }
            }

            MMXVector v2 = MMXVector.NULL_VECTOR;
            for (BoxSide side = BoxSide.LEFT; side <= BoxSide.DOWN; side++)
            {
                GeometryType type = line.Intersection(box.GetSideSegment(side), out v2, out MMXLineSegment l);
                switch (type)
                {
                    case GeometryType.EMPTY:
                        return MMXLineSegment.NULL_SEGMENT;

                    case GeometryType.VECTOR:
                        return new MMXLineSegment(line.End, v2);

                    case GeometryType.LINE_SEGMENT:
                        return l;
                }
            }

            return new MMXLineSegment(v1, v2);
        }

        public static MMXLineSegment operator &(MMXLineSegment line, MMXBox box)
        {
            return box & line;
        }

        /// <summary>
        /// Verifica se um vetor está contido no interior de um retângulo
        /// </summary>
        /// <param name="vec">Vetor</param>
        /// <param name="box">Retângulo</param>
        /// <returns>true se vec estiver contido no interior box, false caso contrário</returns>
        public static bool operator <(MMXVector vec, MMXBox box)
        {
            MMXVector m = box.origin + box.mins;
            MMXVector M = box.origin + box.maxs;
            return m.X < vec.X && vec.X < M.X && m.Y < vec.Y && vec.Y < M.Y;
        }

        /// <summary>
        /// Verifica se um vetor está contido no de um retângulo
        /// </summary>
        /// <param name="vec">Vetor</param>
        /// <param name="box">Retângulo</param>
        /// <returns>true se vec estiver contido no exterior de box, false caso contrário</returns>
        public static bool operator >(MMXVector vec, MMXBox box)
        {
            return !(vec <= box);
        }

        /// <summary>
        /// Verifica se um retâgulo contém um vetor em seu exterior
        /// </summary>
        /// <param name="box">Retângulo</param>
        /// <param name="vec">Vetor</param>
        /// <returns>true se box contém vec em seu exterior, false caso contrário</returns>
        public static bool operator <(MMXBox box, MMXVector vec)
        {
            return !(box >= vec);
        }

        /// <summary>
        /// Verifica um retângulo contém um vetor em seu interior
        /// </summary>
        /// <param name="box">Retângulo</param>
        /// /// <param name="vec">Vetor</param>
        /// <returns>true box contém vec em seu interior, false caso contrário</returns>
        public static bool operator >(MMXBox box, MMXVector vec)
        {
            return vec < box;
        }

        /// <summary>
        /// Verifica se um vetor está contido no interior ou na borda de um retângulo
        /// </summary>
        /// <param name="vec">Vetor</param>
        /// <param name="box">Retângulo</param>
        /// <returns>true se vec estiver contido no interior ou na borda de box, false caso contrário</returns>
        public static bool operator <=(MMXVector vec, MMXBox box)
        {
            MMXVector m = box.origin + box.mins;
            MMXVector M = box.origin + box.maxs;
            return m.X <= vec.X && vec.X < M.X && m.Y <= vec.Y && vec.Y < M.Y;
        }

        /// <summary>
        /// Verifica se um vetor está contido no exterior ou na borda de um retângulo
        /// </summary>
        /// <param name="vec">Vetor</param>
        /// <param name="box">Retângulo</param>
        /// <returns>true se vec estiver contido no exterior ou na borda de box, false caso contrário</returns>
        public static bool operator >=(MMXVector vec, MMXBox box)
        {
            return !(vec < box);
        }

        /// <summary>
        /// Verifica se um retângulo contém um vetor em seu exterior ou em sua borda
        /// </summary>
        /// <param name="box">Retângulo</param>
        /// <param name="vec">Vetor</param>
        /// <returns>true se box contém vec em seu exterior ou em sua borda, false caso contrário</returns>
        public static bool operator <=(MMXBox box, MMXVector vec)
        {
            return !(box > vec);
        }

        /// <summary>
        /// Verifica um retângulo contém um vetor em seu interior ou emsua borda
        /// </summary>
        /// <param name="box">Retângulo</param>
        /// /// <param name="vec">Vetor</param>
        /// <returns>true box contém vec em seu interior ou em sua borda, false caso contrário</returns>
        public static bool operator >=(MMXBox box, MMXVector vec)
        {
            return vec <= box;
        }

        /// <summary>
        /// Veririca se um retângulo está contido em outro retângulo
        /// </summary>
        /// <param name="box1">Primeiro retângulo</param>
        /// <param name="box2">Segundo retângulo</param>
        /// <returns>true se box1 está contido em box2, falso caso contrário</returns>
        public static bool operator <=(MMXBox box1, MMXBox box2)
        {
            return (box1 & box2) == box1;
        }

        /// <summary>
        /// Verifica se um retângulo contém outro retângulo
        /// </summary>
        /// <param name="box1">Primeiro retângulo</param>
        /// <param name="box2">Segundo retângulo</param>
        /// <returns>true se box1 contém box2, false caso contrário</returns>
        public static bool operator >=(MMXBox box1, MMXBox box2)
        {
            return (box2 & box1) == box2;
        }

        /// <summary>
        /// Veririca se um retângulo está inteiramente contido em outro retângulo
        /// </summary>
        /// <param name="box1">Primeiro retângulo</param>
        /// <param name="box2">Segundo retângulo</param>
        /// <returns>true se box1 está inteiramente contido em box2 (ou seja box1 está em box2 mas box1 não é igual a box2), falso caso contrário</returns>
        public static bool operator <(MMXBox box1, MMXBox box2)
        {
            return (box1 <= box2) && (box1 != box2);
        }

        /// <summary>
        /// Verifica se um retângulo contém inteiramente outro retângulo
        /// </summary>
        /// <param name="box1">Primeiro retângulo</param>
        /// <param name="box2">Segundo retângulo</param>
        /// <returns>true se box1 contém inteiramente box2 (ou seja, box1 contém box2 mas box1 não é igual a box2), false caso contrário</returns>
        public static bool operator >(MMXBox box1, MMXBox box2)
        {
            return (box2 <= box1) && (box1 != box2);
        }

        /// <summary>
        /// Verifica se dois retângulos são iguais
        /// </summary>
        /// <param name="box1">Primeiro retângulo</param>
        /// <param name="box2">Segundo retângulo</param>
        /// <returns>true se forem iguais, false caso contrário</returns>
        public static bool operator ==(MMXBox box1, MMXBox box2)
        {
            MMXVector m1 = box1.origin + box1.mins;
            MMXVector M1 = box1.origin + box1.maxs;

            MMXVector m2 = box2.origin + box2.mins;
            MMXVector M2 = box2.origin + box2.maxs;

            return m1 == m2 && M1 == M2;
        }

        /// <summary>
        /// Verifica se dois retângulos são diferentes
        /// </summary>
        /// <param name="box1">Primeiro retângulo</param>
        /// <param name="box2">Segundo retângulo</param>
        /// <returns>true se forem diferentes, false caso contrário</returns>
        public static bool operator !=(MMXBox box1, MMXBox box2)
        {
            MMXVector m1 = box1.origin + box1.mins;
            MMXVector M1 = box1.origin + box1.maxs;

            MMXVector m2 = box2.origin + box2.mins;
            MMXVector M2 = box2.origin + box2.maxs;

            return m1 != m2 || M1 != M2;
        }
    }

    public enum RightTriangleSide
    {
        HCATHETUS,
        VCATHETUS,
        HYPOTENUSE
    }

    public struct MMXRightTriangle : MMXShape
    {
        public const GeometryType type = GeometryType.RIGHT_TRIANGLE;

        public static readonly MMXRightTriangle EMPTY = new MMXRightTriangle(MMXVector.NULL_VECTOR, 0, 0);

        private MMXVector origin;
        private MMXFloat hCathetus;
        private MMXFloat vCathetus;

        public MMXVector Origin
        {
            get
            {
                return origin;
            }
        }

        public MMXVector HCathetusVertex
        {
            get
            {
                return origin + HCathetusVector;
            }
        }

        public MMXVector VCathetusVertex
        {
            get
            {
                return origin + VCathetusVector;
            }
        }

        public MMXFloat HCathetus
        {
            get
            {
                return hCathetus.Abs;
            }
        }

        public MMXFloat VCathetus
        {
            get
            {
                return vCathetus.Abs;
            }
        }

        public MMXFloat Hypotenuse
        {
            get
            {
                return (MMXFloat) Math.Sqrt(hCathetus * hCathetus + vCathetus * vCathetus);
            }
        }

        public MMXVector HCathetusVector
        {
            get
            {
                return new MMXVector(hCathetus, 0);
            }
        }

        public MMXVector VCathetusVector
        {
            get
            {
                return new MMXVector(0, vCathetus);
            }
        }

        public MMXVector HypotenuseVector
        {
            get
            {
                return HCathetusVector - VCathetusVector;
            }
        }

        public MMXLineSegment HypotenuseLine
        {
            get
            {
                return new MMXLineSegment(HCathetusVertex, VCathetusVertex);
            }
        }

        public MMXLineSegment HCathetusLine
        {
            get
            {
                return new MMXLineSegment(origin, HCathetusVertex);
            }
        }

        public MMXLineSegment VCathetusLine
        {
            get
            {
                return new MMXLineSegment(origin, VCathetusVertex);
            }
        }

        public MMXBox WrappingBox
        {
            get
            {
                return new MMXBox(Math.Min(origin.X, origin.X + hCathetus), Math.Min(origin.Y, origin.Y + vCathetus), HCathetus, VCathetus);
            }
        }

        public MMXFloat Left
        {
            get
            {
                return Math.Min(origin.X, origin.X + hCathetus);
            }
        }

        public MMXFloat Top
        {
            get
            {
                return Math.Min(origin.Y, origin.Y + vCathetus);
            }
        }

        public MMXFloat Right
        {
            get
            {
                return Math.Max(origin.X, origin.X + hCathetus);
            }
        }

        public MMXFloat Bottom
        {
            get
            {
                return Math.Max(origin.Y, origin.Y + vCathetus);
            }
        }

        public MMXVector LeftTop
        {
            get
            {
                return new MMXVector(Left, Top);
            }
        }

        public MMXVector RightBottom
        {
            get
            {
                return new MMXVector(Right, Bottom);
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

        public MMXRightTriangle(MMXVector origin, MMXFloat hCathetus, MMXFloat vCathetus)
        {
            this.origin = origin;
            this.hCathetus = hCathetus;
            this.vCathetus = vCathetus;
        }

        public MMXRightTriangle Translate(MMXVector shift)
        {
            return new MMXRightTriangle(origin + shift, hCathetus, vCathetus);
        }

        public MMXRightTriangle Negate()
        {
            return new MMXRightTriangle(-origin, -hCathetus, -vCathetus);
        }

        public MMXFloat Area()
        {
            return 0.5F * HCathetus * VCathetus;
        }

        public MMXVector GetNormal(RightTriangleSide side)
        {
            switch (side)
            {
                case RightTriangleSide.HCATHETUS:
                    return vCathetus >= 0 ? MMXVector.UP_VECTOR : MMXVector.DOWN_VECTOR;

                case RightTriangleSide.VCATHETUS:
                    return hCathetus >= 0 ? MMXVector.RIGHT_VECTOR : MMXVector.LEFT_VECTOR;

                case RightTriangleSide.HYPOTENUSE:
                    {
                        int sign = vCathetus.Signal == hCathetus.Signal ? 1 : -1;
                        MMXVector v = new MMXVector(sign * vCathetus, -sign * hCathetus);
                        return v.Versor();
                    }
            }

            return MMXVector.NULL_VECTOR;
        }

        private static MMXFloat Sign(MMXVector p1, MMXVector p2, MMXVector p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        private static bool PointInTriangle(MMXVector pt, MMXVector v1, MMXVector v2, MMXVector v3)
        {
            MMXFloat d1, d2, d3;
            bool has_neg, has_pos;

            d1 = Sign(pt, v1, v2);
            d2 = Sign(pt, v2, v3);
            d3 = Sign(pt, v3, v1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        public bool Contains(MMXVector v, bool inclusive = true, bool excludeHypotenuse = false)
        {
            if (excludeHypotenuse)
            {
                MMXLineSegment hypotenuseLine = HypotenuseLine;
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

        public bool HasIntersectionWith(MMXBox box, bool excludeHypotenuse = false)
        {
            MMXBox intersection = box & WrappingBox;
            if (intersection.Area() == 0)
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
            return "[" + origin + ":" + hCathetus + ":" + vCathetus + "]";
        }

        GeometryType MMXGeometry.GetType()
        {
            return type;
        }

        public static bool operator ==(MMXRightTriangle left, MMXRightTriangle right)
        {
            return left.origin == right.origin && left.hCathetus == right.hCathetus && left.vCathetus == right.vCathetus;
        }
        public static bool operator !=(MMXRightTriangle left, MMXRightTriangle right)
        {
            return left.origin != right.origin || left.hCathetus != right.hCathetus || left.vCathetus != right.vCathetus;
        }

        public static MMXRightTriangle operator +(MMXRightTriangle triangle, MMXVector shift)
        {
            return triangle.Translate(shift);
        }

        public static MMXRightTriangle operator +(MMXVector shift, MMXRightTriangle triangle)
        {
            return triangle.Translate(shift);
        }

        public static MMXRightTriangle operator -(MMXRightTriangle triangle)
        {
            return triangle.Negate();
        }

        public static MMXRightTriangle operator -(MMXRightTriangle triangle, MMXVector shift)
        {
            return triangle.Translate(-shift);
        }

        public static MMXRightTriangle operator -(MMXVector shift, MMXRightTriangle triangle)
        {
            return (-triangle).Translate(shift);
        }
    }
}
