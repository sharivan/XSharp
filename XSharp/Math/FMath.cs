using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMX.Math
{
    public struct FixedSingle : IComparable<FixedSingle>
    {
        public const int FIXED_BITS_COUNT = 16;
        public const int FIXED_DIVISOR = 1 << FIXED_BITS_COUNT;

        private const int INT_PART_MASK = -1 << FIXED_BITS_COUNT;
        private const int FRAC_PART_MASK = ~INT_PART_MASK;

        public static readonly FixedSingle MINUS_ONE = new(-1D);
        public static readonly FixedSingle ZERO = new(0);
        public static readonly FixedSingle HALF = new(0.5);
        public static readonly FixedSingle ONE = new(1D);
        public static readonly FixedSingle TWO = new(2D);
        public static readonly FixedSingle MIN_VALUE = new(Int32.MinValue);
        public static readonly FixedSingle MIN_POSITIVE_VALUE = new(1);
        public static readonly FixedSingle MAX_VALUE = new(Int32.MaxValue);

        public int RawValue { get; }

        public int IntValue => RawValue >> FIXED_BITS_COUNT;

        public uint RawFracPart => (uint) (RawValue & FRAC_PART_MASK);

        public FixedSingle FracPart => new(RawValue & FRAC_PART_MASK);

        public float FloatValue => (float) RawValue / FIXED_DIVISOR;

        public double DoubleValue => (double) RawValue / FIXED_DIVISOR;

        public FixedSingle Abs => new((RawValue + (RawValue >> 31)) ^ (RawValue >> 31));

        public int Signal => RawValue == 0 ? 0 : RawValue > 0 ? 1 : -1;

        private FixedSingle(int rawValue) => this.RawValue = rawValue;

        public FixedSingle(float value) => RawValue = (int) (value * FIXED_DIVISOR);

        public FixedSingle(double value) => RawValue = (int) (value * FIXED_DIVISOR);

        public FixedSingle(BinaryReader reader) => RawValue = reader.ReadInt32();

        public void Write(BinaryWriter writer) => writer.Write(RawValue);

        public override string ToString() => DoubleValue.ToString(System.Globalization.CultureInfo.InvariantCulture);

        public override bool Equals(object obj) => obj is FixedSingle single && single.RawValue == RawValue;

        public override int GetHashCode() => RawValue;

        public static FixedSingle FromRawValue(int rawValue) => new(rawValue);

        public int CompareTo(FixedSingle other) => this == other ? 0 : this > other ? 1 : -1;

        public int Ceil()
        {
            int intPart = IntValue;

            return Signal < 0 ? intPart : RawFracPart > 0 ? intPart + 1 : intPart;
        }

        public int Floor()
        {
            int intPart = IntValue;

            return Signal >= 0 ? intPart : RawFracPart > 0 ? intPart - 1 : intPart;
        }

        public int Round()
        {
            int intPart = IntValue;

            return Signal >= 0
                ? RawFracPart > 1 << (FIXED_BITS_COUNT - 1) ? intPart + 1 : intPart
                : RawFracPart > 1 << (FIXED_BITS_COUNT - 1) ? intPart - 1 : intPart;
        }

        public FixedSingle Sqrt() => System.Math.Sqrt(DoubleValue);

        public FixedSingle Exp() => System.Math.Exp(DoubleValue);

        public FixedSingle Log() => System.Math.Log10(DoubleValue);

        public FixedSingle Ln() => System.Math.Log(DoubleValue);

        public FixedSingle Cos() => System.Math.Cos(DoubleValue);
        public FixedSingle Cosh() => System.Math.Cosh(DoubleValue);

        public FixedSingle Sin() => System.Math.Sin(DoubleValue);

        public FixedSingle Sinh() => System.Math.Sinh(DoubleValue);

        public FixedSingle Tan() => System.Math.Tan(DoubleValue);

        public FixedSingle Tanh() => System.Math.Tanh(DoubleValue);

        public FixedSingle Atan() => System.Math.Atan(DoubleValue);

        public static FixedSingle Atan2(FixedSingle y, FixedSingle x) => System.Math.Atan2(y.DoubleValue, x.DoubleValue);

        public static bool operator ==(FixedSingle left, FixedSingle right) => left.RawValue == right.RawValue;

        public static bool operator !=(FixedSingle left, FixedSingle right) => left.RawValue != right.RawValue;

        public static bool operator >(FixedSingle left, FixedSingle right) => left.RawValue > right.RawValue;

        public static bool operator >=(FixedSingle left, FixedSingle right) => left.RawValue >= right.RawValue;

        public static bool operator <(FixedSingle left, FixedSingle right) => left.RawValue < right.RawValue;

        public static bool operator <=(FixedSingle left, FixedSingle right) => left.RawValue <= right.RawValue;

        public static FixedSingle operator +(FixedSingle value) => value;

        public static FixedSingle operator +(FixedSingle left, FixedSingle right) => new(left.RawValue + right.RawValue);

        public static FixedSingle operator -(FixedSingle value) => new(-value.RawValue);

        public static FixedSingle operator -(FixedSingle left, FixedSingle right) => new(left.RawValue - right.RawValue);

        public static FixedSingle operator *(FixedSingle left, FixedSingle right) => new((int) (((long) left.RawValue * (long) right.RawValue) >> FIXED_BITS_COUNT));

        public static FixedSingle operator /(FixedSingle left, FixedSingle right) => new((int) (((long) left.RawValue << FIXED_BITS_COUNT) / right.RawValue));

        public static explicit operator int(FixedSingle src) => src.IntValue;

        public static explicit operator float(FixedSingle src) => src.FloatValue;

        public static implicit operator double(FixedSingle src) => src.DoubleValue;

        public static implicit operator FixedSingle(int src) => new(src << FIXED_BITS_COUNT);

        public static implicit operator FixedSingle(float src) => new(src);

        public static implicit operator FixedSingle(double src) => new(src);

        public static FixedSingle Min(FixedSingle f1, FixedSingle f2) => f1 < f2 ? f1 : f2;

        public static FixedSingle Max(FixedSingle f1, FixedSingle f2) => f1 > f2 ? f1 : f2;
    }

    public struct FixedDouble : IComparable<FixedDouble>
    {
        public const int FIXED_BITS_COUNT = 16;
        public const long FIXED_DIVISOR = 1L << FIXED_BITS_COUNT;

        private const long INT_PART_MASK = -1L << FIXED_BITS_COUNT;
        private const long FRAC_PART_MASK = ~INT_PART_MASK;

        public static readonly FixedDouble MINUS_ONE = new(-1D);
        public static readonly FixedDouble ZERO = new(0);
        public static readonly FixedDouble HALF = new(0.5);
        public static readonly FixedDouble ONE = new(1D);
        public static readonly FixedDouble TWO = new(2D);
        public static readonly FixedDouble MIN_VALUE = new(Int64.MinValue);
        public static readonly FixedDouble MIN_POSITIVE_VALUE = new(1L);
        public static readonly FixedDouble MAX_VALUE = new(Int64.MaxValue);

        public long RawValue { get; }

        public int IntValue => (int) (RawValue >> FIXED_BITS_COUNT);

        public long LongValue => RawValue >> FIXED_BITS_COUNT;

        public long RawFracPart => RawValue & FRAC_PART_MASK;

        public FixedDouble FracPart => new(RawValue & FRAC_PART_MASK);

        public float FloatValue => RawValue / (float) FIXED_DIVISOR;

        public double DoubleValue => RawValue / (double) FIXED_DIVISOR;

        public FixedDouble Abs => new((RawValue + (RawValue >> 63)) ^ (RawValue >> 63));

        public int Signal => RawValue == 0 ? 0 : RawValue > 0 ? 1 : -1;

        private FixedDouble(long rawValue) => this.RawValue = rawValue;

        public FixedDouble(float value) => RawValue = (long) (value * FIXED_DIVISOR);

        public FixedDouble(double value) => RawValue = (long) (value * FIXED_DIVISOR);

        public FixedDouble(BinaryReader reader) => RawValue = reader.ReadInt64();

        public void Write(BinaryWriter writer) => writer.Write(RawValue);

        public override string ToString() => DoubleValue.ToString(System.Globalization.CultureInfo.InvariantCulture);

        public override bool Equals(object obj) => obj is FixedDouble @double && @double.RawValue == RawValue;

        public override int GetHashCode() => (int) RawValue;

        public static FixedDouble FromRawValue(long rawValue) => new(rawValue);

        public int CompareTo(FixedDouble other) => this == other ? 0 : this > other ? 1 : -1;

        public long Ceil()
        {
            long intPart = LongValue;

            return Signal < 0 ? intPart : RawFracPart > 0 ? intPart + 1 : intPart;
        }

        public long Floor()
        {
            long intPart = LongValue;

            return Signal >= 0 ? intPart : RawFracPart > 0 ? intPart - 1 : intPart;
        }

        public long Round()
        {
            long intPart = LongValue;

            return Signal >= 0
                ? RawFracPart > 1 << (FIXED_BITS_COUNT - 1) ? intPart + 1 : intPart
                : RawFracPart > 1 << (FIXED_BITS_COUNT - 1) ? intPart - 1 : intPart;
        }

        public FixedDouble Sqrt() => System.Math.Sqrt(DoubleValue);

        public FixedDouble Exp() => System.Math.Exp(DoubleValue);

        public FixedDouble Log() => System.Math.Log10(DoubleValue);

        public FixedDouble Ln() => System.Math.Log(DoubleValue);

        public FixedDouble Cos() => System.Math.Cos(DoubleValue);
        public FixedDouble Cosh() => System.Math.Cosh(DoubleValue);

        public FixedDouble Sin() => System.Math.Sin(DoubleValue);

        public FixedDouble Sinh() => System.Math.Sinh(DoubleValue);

        public FixedDouble Tan() => System.Math.Tan(DoubleValue);

        public FixedDouble Tanh() => System.Math.Tanh(DoubleValue);

        public FixedDouble Atan() => System.Math.Atan(DoubleValue);

        public static FixedDouble Atan2(FixedDouble y, FixedDouble x) => System.Math.Atan2(y.DoubleValue, x.DoubleValue);

        public static bool operator ==(FixedDouble left, FixedDouble right) => left.RawValue == right.RawValue;

        public static bool operator !=(FixedDouble left, FixedDouble right) => left.RawValue != right.RawValue;

        public static bool operator >(FixedDouble left, FixedDouble right) => left.RawValue > right.RawValue;

        public static bool operator >=(FixedDouble left, FixedDouble right) => left.RawValue >= right.RawValue;

        public static bool operator <(FixedDouble left, FixedDouble right) => left.RawValue < right.RawValue;

        public static bool operator <=(FixedDouble left, FixedDouble right) => left.RawValue <= right.RawValue;

        public static FixedDouble operator +(FixedDouble value) => value;

        public static FixedDouble operator +(FixedDouble left, FixedDouble right) => new(left.RawValue + right.RawValue);

        public static FixedDouble operator -(FixedDouble value) => new(-value.RawValue);

        public static FixedDouble operator -(FixedDouble left, FixedDouble right) => new(left.RawValue - right.RawValue);

        public static FixedDouble operator *(FixedDouble left, FixedDouble right) => new((left.RawValue * right.RawValue) >> FIXED_BITS_COUNT);

        public static FixedDouble operator /(FixedDouble left, FixedDouble right) => new((left.RawValue << FIXED_BITS_COUNT) / right.RawValue);

        public static explicit operator int(FixedDouble src) => src.IntValue;

        public static explicit operator long(FixedDouble src) => src.LongValue;

        public static explicit operator float(FixedDouble src) => src.FloatValue;

        public static explicit operator FixedSingle(FixedDouble src) => FixedSingle.FromRawValue((int) (src.RawValue >> (FIXED_BITS_COUNT - FixedSingle.FIXED_BITS_COUNT)));

        public static implicit operator double(FixedDouble src) => src.DoubleValue;

        public static implicit operator FixedDouble(int src) => new((long) src << FIXED_BITS_COUNT);

        public static implicit operator FixedDouble(long src) => new(src << FIXED_BITS_COUNT);

        public static implicit operator FixedDouble(float src) => new(src);

        public static implicit operator FixedDouble(FixedSingle src) => new((long) src.RawValue << (FIXED_BITS_COUNT - FixedSingle.FIXED_BITS_COUNT));

        public static implicit operator FixedDouble(double src) => new(src);

        public static FixedDouble Min(FixedDouble f1, FixedDouble f2) => f1 < f2 ? f1 : f2;

        public static FixedDouble Max(FixedDouble f1, FixedDouble f2) => f1 > f2 ? f1 : f2;
    }

    public readonly struct Interval
    {
        public static readonly Interval EMPTY = MakeOpenInterval(0, 0);
        private readonly FixedSingle max;

        public FixedSingle Min { get; }

        public bool IsOpenLeft => !IsClosedLeft;

        public bool IsClosedLeft { get; }

        public bool IsOpenRight => !IsClosedRight;

        public bool IsClosedRight { get; }

        public bool IsClosed => IsClosedLeft && IsClosedRight;

        public bool IsOpen => !IsClosedLeft && !IsClosedRight;

        public bool IsEmpty => IsClosed ? Min > max : Min >= max;

        public bool IsPoint => IsClosed && Min == max;

        public FixedSingle Length => max - Min;

        private Interval(FixedSingle min, bool closedLeft, FixedSingle max, bool closedRight)
        {
            this.Min = min;
            this.IsClosedLeft = closedLeft;
            this.max = max;
            this.IsClosedRight = closedRight;
        }

        public override bool Equals(object obj)
        {
            if (obj is not Interval)
                return false;

            var interval = (Interval) obj;
            return Equals(interval);
        }

        public bool Equals(Interval other) => IsEmpty && other.IsEmpty
|| Min == other.Min && IsClosedLeft == other.IsClosedLeft && max == other.max && IsClosedRight == other.IsClosedRight;

        public bool Contains(FixedSingle element, bool inclusive = true) => !inclusive
                ? Min < element && element < max
                : IsClosedLeft ? Min > element : Min < element && !(IsClosedRight ? element > max : element >= max);

        public bool Contains(Interval interval, bool inclusive = true) => interval.IsEmpty
                ? inclusive || !IsEmpty
                : !inclusive
                ? Min < interval.Min && interval.max < max
                : IsClosedLeft ? Min > interval.Min : interval.IsClosedLeft ? Min >= interval.Min : Min <= interval.Min
&& !(IsClosedRight ? interval.max > max : interval.IsClosedRight ? interval.max >= max : interval.max > max);

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
            if (max < other.max)
            {
                newMax = max;
                newClosedRight = IsClosedRight;
            }
            else if (max > other.max)
            {
                newMax = other.max;
                newClosedRight = other.IsClosedRight;
            }
            else
            {
                newMax = max;
                newClosedRight = IsClosedRight && other.IsClosedRight;
            }

            return new Interval(newMin, newClosedLeft, newMax, newClosedRight);
        }

        public static Interval MakeOpenInterval(FixedSingle v1, FixedSingle v2) => new(FixedSingle.Min(v1, v2), false, FixedSingle.Max(v1, v2), false);

        public static Interval MakeClosedInterval(FixedSingle v1, FixedSingle v2) => new(FixedSingle.Min(v1, v2), true, FixedSingle.Max(v1, v2), true);

        public static Interval MakeSemiOpenLeftInterval(FixedSingle v1, FixedSingle v2) => new(FixedSingle.Min(v1, v2), false, FixedSingle.Max(v1, v2), true);

        public static Interval MakeSemiOpenRightInterval(FixedSingle v1, FixedSingle v2) => new(FixedSingle.Min(v1, v2), true, FixedSingle.Max(v1, v2), false);

        public override string ToString() => IsEmpty ? "{}" : (IsClosedLeft ? "[" : "(") + Min + ", " + max + (IsClosedRight ? "]" : ")");

        public override int GetHashCode()
        {
            var hashCode = -1682582155;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<FixedSingle>.Default.GetHashCode(Min);
            hashCode = hashCode * -1521134295 + IsClosedLeft.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<FixedSingle>.Default.GetHashCode(max);
            hashCode = hashCode * -1521134295 + IsClosedRight.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Interval left, Interval right) => left.Equals(right);

        public static bool operator !=(Interval left, Interval right) => !left.Equals(right);
    }
}
