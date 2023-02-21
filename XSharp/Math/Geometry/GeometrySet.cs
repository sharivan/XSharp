using System;
using System.Collections.Generic;
using System.Linq;

namespace XSharp.Math.Geometry
{
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
                return false;

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
}