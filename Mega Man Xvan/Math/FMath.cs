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

        public static readonly FixedSingle MINUS_ONE = new FixedSingle(-1D);
        public static readonly FixedSingle ZERO = new FixedSingle(0);
        public static readonly FixedSingle HALF = new FixedSingle(0.5);
        public static readonly FixedSingle ONE = new FixedSingle(1D);
        public static readonly FixedSingle TWO = new FixedSingle(2D);
        public static readonly FixedSingle MIN_VALUE = new FixedSingle(Int32.MinValue);
        public static readonly FixedSingle MIN_POSITIVE_VALUE = new FixedSingle(1);
        public static readonly FixedSingle MAX_VALUE = new FixedSingle(Int32.MaxValue);

        private int rawValue;

        public int RawValue
        {
            get
            {
                return rawValue;
            }
        }

        public int IntValue
        {
            get
            {
                return rawValue >> FIXED_BITS_COUNT;
            }
        }

        public uint RawFracPart
        {
            get
            {
                return (uint) (rawValue & FRAC_PART_MASK);
            }
        }

        public FixedSingle FracPart
        {
            get
            {
                return new FixedSingle(rawValue & FRAC_PART_MASK);
            }
        }

        public float FloatValue
        {
            get
            {
                return (float) rawValue / FIXED_DIVISOR;
            }
        }

        public double DoubleValue
        {
            get
            {
                return (double) rawValue / FIXED_DIVISOR;
            }
        }

        public FixedSingle Abs
        {
            get
            {
                return new FixedSingle((rawValue + (rawValue >> 31)) ^ (rawValue >> 31));
            }
        }

        public int Signal
        {
            get
            {
                return rawValue == 0 ? 0 : rawValue > 0 ? 1 : -1;
            }
        }

        private FixedSingle(int rawValue)
        {
            this.rawValue = rawValue;
        }

        public FixedSingle(float value)
        {
            rawValue = (int) (value * FIXED_DIVISOR);
        }

        public FixedSingle(double value)
        {
            rawValue = (int) (value * FIXED_DIVISOR);
        }

        public FixedSingle(BinaryReader reader)
        {
            rawValue = reader.ReadInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(rawValue);
        }

        public override string ToString()
        {
            return DoubleValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public override bool Equals(object obj)
        {
            if (obj is FixedSingle)
                return ((FixedSingle) obj).rawValue == rawValue;

            return false;
        }

        public override int GetHashCode()
        {
            return rawValue;
        }

        public static FixedSingle FromRawValue(int rawValue)
        {
            return new FixedSingle(rawValue);
        }

        public int CompareTo(FixedSingle other)
        {
            return this == other ? 0 : this > other ? 1 : -1;
        }

        public int Ceil()
        {
            int intPart = IntValue;

            if (Signal < 0)
                return intPart;

            return RawFracPart > 0 ? intPart + 1 : intPart;
        }

        public int Floor()
        {
            int intPart = IntValue;

            if (Signal >= 0)
                return intPart;

            return RawFracPart > 0 ? intPart - 1 : intPart;
        }

        public int Round()
        {
            int intPart = IntValue;

            if (Signal >= 0)
                return RawFracPart > (1 << (FIXED_BITS_COUNT - 1)) ? intPart + 1 : intPart;

            return RawFracPart > (1 << (FIXED_BITS_COUNT - 1)) ? intPart - 1 : intPart;
        }

        public FixedSingle Sqrt()
        {
            return System.Math.Sqrt(DoubleValue);
        }

        public FixedSingle Exp()
        {
            return System.Math.Exp(DoubleValue);
        }

        public FixedSingle Log()
        {
            return System.Math.Log10(DoubleValue);
        }

        public FixedSingle Ln()
        {
            return System.Math.Log(DoubleValue);
        }

        public FixedSingle Cos()
        {
            return System.Math.Cos(DoubleValue);
        }
        public FixedSingle Cosh()
        {
            return System.Math.Cosh(DoubleValue);
        }

        public FixedSingle Sin()
        {
            return System.Math.Sin(DoubleValue);
        }

        public FixedSingle Sinh()
        {
            return System.Math.Sinh(DoubleValue);
        }

        public FixedSingle Tan()
        {
            return System.Math.Tan(DoubleValue);
        }

        public FixedSingle Tanh()
        {
            return System.Math.Tanh(DoubleValue);
        }

        public FixedSingle Atan()
        {
            return System.Math.Atan(DoubleValue);
        }

        public static FixedSingle Atan2(FixedSingle y, FixedSingle x)
        {
            return System.Math.Atan2(y.DoubleValue, x.DoubleValue);
        }

        public static bool operator ==(FixedSingle left, FixedSingle right)
        {
            return left.rawValue == right.rawValue;
        }

        public static bool operator !=(FixedSingle left, FixedSingle right)
        {
            return left.rawValue != right.rawValue;
        }

        public static bool operator >(FixedSingle left, FixedSingle right)
        {
            return left.rawValue > right.rawValue;
        }

        public static bool operator >=(FixedSingle left, FixedSingle right)
        {
            return left.rawValue >= right.rawValue;
        }

        public static bool operator <(FixedSingle left, FixedSingle right)
        {
            return left.rawValue < right.rawValue;
        }

        public static bool operator <=(FixedSingle left, FixedSingle right)
        {
            return left.rawValue <= right.rawValue;
        }

        public static FixedSingle operator +(FixedSingle value)
        {
            return value;
        }

        public static FixedSingle operator +(FixedSingle left, FixedSingle right)
        {
            return new FixedSingle(left.rawValue + right.rawValue);
        }

        public static FixedSingle operator -(FixedSingle value)
        {
            return new FixedSingle(-value.rawValue);
        }

        public static FixedSingle operator -(FixedSingle left, FixedSingle right)
        {
            return new FixedSingle(left.rawValue - right.rawValue);
        }

        public static FixedSingle operator *(FixedSingle left, FixedSingle right)
        {
            return new FixedSingle((int) (((long) left.rawValue * (long) right.rawValue) >> FIXED_BITS_COUNT));
        }

        public static FixedSingle operator /(FixedSingle left, FixedSingle right)
        {
            return new FixedSingle((int) (((long) left.rawValue << FIXED_BITS_COUNT) / right.rawValue));
        }

        public static explicit operator int(FixedSingle src)
        {
            return src.IntValue;
        }

        public static explicit operator float(FixedSingle src)
        {
            return src.FloatValue;
        }

        public static implicit operator double(FixedSingle src)
        {
            return src.DoubleValue;
        }

        public static implicit operator FixedSingle(int src)
        {
            return new FixedSingle(src << FIXED_BITS_COUNT);
        }

        public static implicit operator FixedSingle(float src)
        {
            return new FixedSingle(src);
        }

        public static implicit operator FixedSingle(double src)
        {
            return new FixedSingle(src);
        }

        public static FixedSingle Min(FixedSingle f1, FixedSingle f2)
        {
            return f1 < f2 ? f1 : f2;
        }

        public static FixedSingle Max(FixedSingle f1, FixedSingle f2)
        {
            return f1 > f2 ? f1 : f2;
        }
    }

    public struct FixedDouble : IComparable<FixedDouble>
    {
        public const int FIXED_BITS_COUNT = 16;
        public const long FIXED_DIVISOR = 1L << FIXED_BITS_COUNT;

        private const long INT_PART_MASK = -1L << FIXED_BITS_COUNT;
        private const long FRAC_PART_MASK = ~INT_PART_MASK;

        public static readonly FixedDouble MINUS_ONE = new FixedDouble(-1D);
        public static readonly FixedDouble ZERO = new FixedDouble(0);
        public static readonly FixedDouble HALF = new FixedDouble(0.5);
        public static readonly FixedDouble ONE = new FixedDouble(1D);
        public static readonly FixedDouble TWO = new FixedDouble(2D);
        public static readonly FixedDouble MIN_VALUE = new FixedDouble(Int64.MinValue);
        public static readonly FixedDouble MIN_POSITIVE_VALUE = new FixedDouble(1L);
        public static readonly FixedDouble MAX_VALUE = new FixedDouble(Int64.MaxValue);

        private long rawValue;

        public long RawValue
        {
            get
            {
                return rawValue;
            }
        }

        public int IntValue
        {
            get
            {
                return (int) (rawValue >> FIXED_BITS_COUNT);
            }
        }

        public long LongValue
        {
            get
            {
                return rawValue >> FIXED_BITS_COUNT;
            }
        }

        public long RawFracPart
        {
            get
            {
                return rawValue & FRAC_PART_MASK;
            }
        }

        public FixedDouble FracPart
        {
            get
            {
                return new FixedDouble(rawValue & FRAC_PART_MASK);
            }
        }

        public float FloatValue
        {
            get
            {
                return rawValue / (float) FIXED_DIVISOR;
            }
        }

        public double DoubleValue
        {
            get
            {
                return rawValue / (double) FIXED_DIVISOR;
            }
        }

        public FixedDouble Abs
        {
            get
            {
                return new FixedDouble((rawValue + (rawValue >> 63)) ^ (rawValue >> 63));
            }
        }

        public int Signal
        {
            get
            {
                return rawValue == 0 ? 0 : rawValue > 0 ? 1 : -1;
            }
        }

        private FixedDouble(long rawValue)
        {
            this.rawValue = rawValue;
        }

        public FixedDouble(float value)
        {
            rawValue = (long) (value * FIXED_DIVISOR);
        }

        public FixedDouble(double value)
        {
            rawValue = (long) (value * FIXED_DIVISOR);
        }

        public FixedDouble(BinaryReader reader)
        {
            rawValue = reader.ReadInt64();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(rawValue);
        }

        public override string ToString()
        {
            return DoubleValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public override bool Equals(object obj)
        {
            if (obj is FixedDouble)
                return ((FixedDouble) obj).rawValue == rawValue;

            return false;
        }

        public override int GetHashCode()
        {
            return (int) rawValue;
        }

        public static FixedDouble FromRawValue(long rawValue)
        {
            return new FixedDouble(rawValue);
        }

        public int CompareTo(FixedDouble other)
        {
            return this == other ? 0 : this > other ? 1 : -1;
        }

        public long Ceil()
        {
            long intPart = LongValue;

            if (Signal < 0)
                return intPart;

            return RawFracPart > 0 ? intPart + 1 : intPart;
        }

        public long Floor()
        {
            long intPart = LongValue;

            if (Signal >= 0)
                return intPart;

            return RawFracPart > 0 ? intPart - 1 : intPart;
        }

        public long Round()
        {
            long intPart = LongValue;

            if (Signal >= 0)
                return RawFracPart > (1 << (FIXED_BITS_COUNT - 1)) ? intPart + 1 : intPart;

            return RawFracPart > (1 << (FIXED_BITS_COUNT - 1)) ? intPart - 1 : intPart;
        }

        public FixedDouble Sqrt()
        {
            return System.Math.Sqrt(DoubleValue);
        }

        public FixedDouble Exp()
        {
            return System.Math.Exp(DoubleValue);
        }

        public FixedDouble Log()
        {
            return System.Math.Log10(DoubleValue);
        }

        public FixedDouble Ln()
        {
            return System.Math.Log(DoubleValue);
        }

        public FixedDouble Cos()
        {
            return System.Math.Cos(DoubleValue);
        }
        public FixedDouble Cosh()
        {
            return System.Math.Cosh(DoubleValue);
        }

        public FixedDouble Sin()
        {
            return System.Math.Sin(DoubleValue);
        }

        public FixedDouble Sinh()
        {
            return System.Math.Sinh(DoubleValue);
        }

        public FixedDouble Tan()
        {
            return System.Math.Tan(DoubleValue);
        }

        public FixedDouble Tanh()
        {
            return System.Math.Tanh(DoubleValue);
        }

        public FixedDouble Atan()
        {
            return System.Math.Atan(DoubleValue);
        }

        public static FixedDouble Atan2(FixedDouble y, FixedDouble x)
        {
            return System.Math.Atan2(y.DoubleValue, x.DoubleValue);
        }

        public static bool operator ==(FixedDouble left, FixedDouble right)
        {
            return left.rawValue == right.rawValue;
        }

        public static bool operator !=(FixedDouble left, FixedDouble right)
        {
            return left.rawValue != right.rawValue;
        }

        public static bool operator >(FixedDouble left, FixedDouble right)
        {
            return left.rawValue > right.rawValue;
        }

        public static bool operator >=(FixedDouble left, FixedDouble right)
        {
            return left.rawValue >= right.rawValue;
        }

        public static bool operator <(FixedDouble left, FixedDouble right)
        {
            return left.rawValue < right.rawValue;
        }

        public static bool operator <=(FixedDouble left, FixedDouble right)
        {
            return left.rawValue <= right.rawValue;
        }

        public static FixedDouble operator +(FixedDouble value)
        {
            return value;
        }

        public static FixedDouble operator +(FixedDouble left, FixedDouble right)
        {
            return new FixedDouble(left.rawValue + right.rawValue);
        }

        public static FixedDouble operator -(FixedDouble value)
        {
            return new FixedDouble(-value.rawValue);
        }

        public static FixedDouble operator -(FixedDouble left, FixedDouble right)
        {
            return new FixedDouble(left.rawValue - right.rawValue);
        }

        public static FixedDouble operator *(FixedDouble left, FixedDouble right)
        {
            return new FixedDouble((left.rawValue * right.rawValue) >> FIXED_BITS_COUNT);
        }

        public static FixedDouble operator /(FixedDouble left, FixedDouble right)
        {
            return new FixedDouble((left.rawValue << FIXED_BITS_COUNT) / right.rawValue);
        }

        public static explicit operator int(FixedDouble src)
        {
            return src.IntValue;
        }

        public static explicit operator long(FixedDouble src)
        {
            return src.LongValue;
        }

        public static explicit operator float(FixedDouble src)
        {
            return src.FloatValue;
        }

        public static explicit operator FixedSingle(FixedDouble src)
        {
            return FixedSingle.FromRawValue((int) (src.rawValue >> (FIXED_BITS_COUNT - FixedSingle.FIXED_BITS_COUNT)));
        }

        public static implicit operator double(FixedDouble src)
        {
            return src.DoubleValue;
        }

        public static implicit operator FixedDouble(int src)
        {
            return new FixedDouble((long) src << FIXED_BITS_COUNT);
        }

        public static implicit operator FixedDouble(long src)
        {
            return new FixedDouble(src << FIXED_BITS_COUNT);
        }

        public static implicit operator FixedDouble(float src)
        {
            return new FixedDouble(src);
        }

        public static implicit operator FixedDouble(FixedSingle src)
        {
            return new FixedDouble((long) src.RawValue << (FIXED_BITS_COUNT - FixedSingle.FIXED_BITS_COUNT));
        }

        public static implicit operator FixedDouble(double src)
        {
            return new FixedDouble(src);
        }

        public static FixedDouble Min(FixedDouble f1, FixedDouble f2)
        {
            return f1 < f2 ? f1 : f2;
        }

        public static FixedDouble Max(FixedDouble f1, FixedDouble f2)
        {
            return f1 > f2 ? f1 : f2;
        }
    }

    public struct Interval
    {
        public static readonly Interval EMPTY = MakeOpenInterval(0, 0);

        private FixedSingle min;
        private bool closedLeft;
        private FixedSingle max;
        private bool closedRight;

        public FixedSingle Min
        {
            get
            {
                return min;
            }
        }

        public bool IsOpenLeft
        {
            get
            {
                return !closedLeft;
            }
        }

        public bool IsClosedLeft
        {
            get
            {
                return closedLeft;
            }
        }

        public bool IsOpenRight
        {
            get
            {
                return !closedRight;
            }
        }

        public bool IsClosedRight
        {
            get
            {
                return closedRight;
            }
        }

        public bool IsClosed
        {
            get
            {
                return closedLeft && closedRight;
            }
        }

        public bool IsOpen
        {
            get
            {
                return !closedLeft && !closedRight;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return IsClosed ? min > max : min >= max;
            }
        }

        public bool IsPoint
        {
            get
            {
                return IsClosed && min == max;
            }
        }

        public FixedSingle Length
        {
            get
            {
                return max - min;
            }
        }

        private Interval(FixedSingle min, bool closedLeft, FixedSingle max, bool closedRight)
        {
            this.min = min;
            this.closedLeft = closedLeft;
            this.max = max;
            this.closedRight = closedRight;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Interval))
                return false;

            Interval interval = (Interval) obj;
            return Equals(interval);
        }

        public bool Equals(Interval other)
        {
            if (IsEmpty && other.IsEmpty)
                return true;

            return min == other.min && closedLeft == other.closedLeft && max == other.max && closedRight == other.closedRight;
        }

        public bool Contains(FixedSingle element, bool inclusive = true)
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

            FixedSingle newMin;
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

            FixedSingle newMax;
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

        public static Interval MakeOpenInterval(FixedSingle v1, FixedSingle v2)
        {
            return new Interval(FixedSingle.Min(v1, v2), false, FixedSingle.Max(v1, v2), false);
        }

        public static Interval MakeClosedInterval(FixedSingle v1, FixedSingle v2)
        {
            return new Interval(FixedSingle.Min(v1, v2), true, FixedSingle.Max(v1, v2), true);
        }

        public static Interval MakeSemiOpenLeftInterval(FixedSingle v1, FixedSingle v2)
        {
            return new Interval(FixedSingle.Min(v1, v2), false, FixedSingle.Max(v1, v2), true);
        }

        public static Interval MakeSemiOpenRightInterval(FixedSingle v1, FixedSingle v2)
        {
            return new Interval(FixedSingle.Min(v1, v2), true, FixedSingle.Max(v1, v2), false);
        }

        public override string ToString()
        {
            if (IsEmpty)
                return "{}";

            return (closedLeft ? "[" : "(") + min + ", " + max + (closedRight ? "]" : ")");
        }

        public override int GetHashCode()
        {
            var hashCode = -1682582155;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<FixedSingle>.Default.GetHashCode(min);
            hashCode = hashCode * -1521134295 + closedLeft.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<FixedSingle>.Default.GetHashCode(max);
            hashCode = hashCode * -1521134295 + closedRight.GetHashCode();
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
    }
}
