﻿namespace XSharp.Math
{
    public readonly struct Interval
    {
        public static readonly Interval EMPTY = MakeOpenInterval(0, 0);

        public FixedSingle Min
        {
            get;
        }

        public FixedSingle Max
        {
            get;
        }

        public bool IsOpenLeft => !IsClosedLeft;

        public bool IsClosedLeft
        {
            get;
        }

        public bool IsOpenRight => !IsClosedRight;

        public bool IsClosedRight
        {
            get;
        }

        public bool IsClosed => IsClosedLeft && IsClosedRight;

        public bool IsOpen => !IsClosedLeft && !IsClosedRight;

        public bool IsEmpty => IsClosed ? Min > Max : Min >= Max;

        public bool IsPoint => IsClosed && Min == Max;

        public FixedSingle Length => Max - Min;

        private Interval(FixedSingle min, bool closedLeft, FixedSingle max, bool closedRight)
        {
            Min = min;
            IsClosedLeft = closedLeft;
            Max = max;
            IsClosedRight = closedRight;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Interval)
                return false;

            var interval = (Interval) obj;
            return Equals(interval);
        }

        private bool CheckMin(FixedSingle element, FixedSingle epslon)
        {
            return IsClosedLeft ? Min - epslon <= element : Min - epslon < element;
        }

        private bool CheckMax(FixedSingle element, FixedSingle epslon)
        {
            return IsClosedRight ? element <= Max + epslon : element < Max + epslon;
        }

        public bool Equals(Interval other)
        {
            return IsEmpty && other.IsEmpty
                || Min == other.Min && IsClosedLeft == other.IsClosedLeft && Max == other.Max && IsClosedRight == other.IsClosedRight;
        }

        public bool Contains(FixedSingle element, FixedSingle epslon, bool includeBounds = true)
        {
            return !includeBounds
                ? Min - epslon < element && element < Max + epslon
                : CheckMin(element, epslon) && CheckMax(element, epslon);
        }

        public bool Contains(FixedSingle element, bool includeBounds = true)
        {
            return Contains(element, 0, includeBounds);
        }

        public bool Contains(Interval interval, bool includeBounds = true)
        {
            return interval.IsEmpty
                ? includeBounds || !IsEmpty
                : !includeBounds
                ? Min < interval.Min && interval.Max < Max
                : IsClosedLeft ? Min > interval.Min : interval.IsClosedLeft ? Min >= interval.Min : Min <= interval.Min
                && !(IsClosedRight ? interval.Max > Max : interval.IsClosedRight ? interval.Max >= Max : interval.Max > Max);
        }

        public Interval Union(Interval other)
        {
            if (other.IsEmpty)
                return this;

            FixedSingle newMin;
            bool newClosedLeft;
            if (Min > other.Min)
            {
                newMin = other.Min;
                newClosedLeft = other.IsClosedLeft;
            }
            else if (Min < other.Min)
            {
                newMin = Min;
                newClosedLeft = IsClosedLeft;
            }
            else
            {
                newMin = Min;
                newClosedLeft = IsClosedLeft || other.IsClosedLeft;
            }

            FixedSingle newMax;
            bool newClosedRight;
            if (Max < other.Max)
            {
                newMax = other.Max;
                newClosedRight = other.IsClosedRight;
            }
            else if (Max > other.Max)
            {
                newMax = Max;
                newClosedRight = IsClosedRight;
            }
            else
            {
                newMax = Max;
                newClosedRight = IsClosedRight || other.IsClosedRight;
            }

            return new Interval(newMin, newClosedLeft, newMax, newClosedRight);
        }

        public Interval Intersection(Interval other)
        {
            if (other.IsEmpty)
                return EMPTY;

            FixedSingle newMin;
            bool newClosedLeft;
            if (Min > other.Min)
            {
                newMin = Min;
                newClosedLeft = IsClosedLeft;
            }
            else if (Min < other.Min)
            {
                newMin = other.Min;
                newClosedLeft = other.IsClosedLeft;
            }
            else
            {
                newMin = Min;
                newClosedLeft = IsClosedLeft && other.IsClosedLeft;
            }

            FixedSingle newMax;
            bool newClosedRight;
            if (Max < other.Max)
            {
                newMax = Max;
                newClosedRight = IsClosedRight;
            }
            else if (Max > other.Max)
            {
                newMax = other.Max;
                newClosedRight = other.IsClosedRight;
            }
            else
            {
                newMax = Max;
                newClosedRight = IsClosedRight && other.IsClosedRight;
            }

            return new Interval(newMin, newClosedLeft, newMax, newClosedRight);
        }

        public bool IsOverlaping(Interval other)
        {
            return !Intersection(other).IsEmpty;
        }

        public static Interval MakeInterval((FixedSingle pos, bool closed) v1, (FixedSingle pos, bool closed) v2)
        {
            bool closedLeft;
            bool closedRight;
            FixedSingle left;
            FixedSingle right;

            if (v1.pos < v2.pos)
            {
                closedLeft = v1.closed;
                left = v1.pos;
                closedRight = v2.closed;
                right = v2.pos;
            }
            else
            {
                closedLeft = v2.closed;
                left = v2.pos;
                closedRight = v1.closed;
                right = v1.pos;
            }

            return new(left, closedLeft, right, closedRight);
        }

        public static Interval MakeOpenInterval(FixedSingle v1, FixedSingle v2)
        {
            return new(FixedSingle.Min(v1, v2), false, FixedSingle.Max(v1, v2), false);
        }

        public static Interval MakeClosedInterval(FixedSingle v1, FixedSingle v2)
        {
            return new(FixedSingle.Min(v1, v2), true, FixedSingle.Max(v1, v2), true);
        }

        public static Interval MakeSemiOpenLeftInterval(FixedSingle v1, FixedSingle v2)
        {
            return new(FixedSingle.Min(v1, v2), false, FixedSingle.Max(v1, v2), true);
        }

        public static Interval MakeSemiOpenRightInterval(FixedSingle v1, FixedSingle v2)
        {
            return new(FixedSingle.Min(v1, v2), true, FixedSingle.Max(v1, v2), false);
        }

        public override string ToString()
        {
            return IsEmpty ? "{}" : (IsClosedLeft ? "[" : "(") + Min + ", " + Max + (IsClosedRight ? "]" : ")");
        }

        public override int GetHashCode()
        {
            int hashCode = 1806637373;
            hashCode = hashCode * -1521134295 + Min.GetHashCode();
            hashCode = hashCode * -1521134295 + Max.GetHashCode();
            hashCode = hashCode * -1521134295 + IsClosedLeft.GetHashCode();
            hashCode = hashCode * -1521134295 + IsClosedRight.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Interval left, Interval right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Interval left, Interval right)
        {
            return !left.Equals(right);
        }

        public static Interval operator |(Interval left, Interval right)
        {
            return left.Union(right);
        }

        public static Interval operator &(Interval left, Interval right)
        {
            return left.Intersection(right);
        }
    }
}