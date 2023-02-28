using System;

namespace XSharp.Math.Geometry
{
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
        public bool Contains(Vector v)
        {
            if (Compare(v) != 0)
                return false;

            var mX = FixedSingle.Min(Start.X, End.X);
            var MX = FixedSingle.Max(Start.X, End.X);
            var mY = FixedSingle.Min(Start.Y, End.Y);
            var MY = FixedSingle.Max(Start.Y, End.Y);

            return mX <= v.X && v.X <= MX && mY <= v.Y && v.Y <= MY;
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

        public GeometryType Intersection(LineSegment s, out LineSegment result)
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

        public bool HasIntersectionWith(LineSegment other)
        {
            return Intersection(other, out _) != GeometryType.EMPTY;
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
                    _ => throw new NotImplementedException()
                };
        }

        public Box WrappingBox()
        {
            return (Start, Vector.NULL_VECTOR, End - Start);
        }

        public override bool Equals(object obj)
        {
            if (obj is not LineSegment)
                return false;

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

        public override string ToString()
        {
            return "[" + Start + " : " + End + "]";
        }

        public override int GetHashCode()
        {
            int hashCode = -1676728671;
            hashCode = hashCode * -1521134295 + Start.GetHashCode();
            hashCode = hashCode * -1521134295 + End.GetHashCode();
            return hashCode;
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
}