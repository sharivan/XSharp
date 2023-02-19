using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace XSharp.Math
{
    public class FixedSingleTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(int)
                || sourceType == typeof(float) 
                || sourceType == typeof(double)
                || sourceType == typeof(FixedSingle)
                || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            return value switch
            {              
                int => new FixedSingle(Convert.ToInt32(value, culture)),                
                float => new FixedSingle(Convert.ToSingle(value, culture)),
                double => new FixedSingle(Convert.ToDouble(value, culture)),
                FixedSingle => value,
                _ => base.ConvertFrom(context, culture, value)
            };
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            return destinationType == typeof(float)
                ? (float) (FixedSingle) value
                : destinationType == typeof(double) 
                ? (double) (FixedSingle) value
                : destinationType == typeof(FixedSingle)
                ? value
                : destinationType == typeof(FixedDouble)
                ? (FixedDouble) (FixedSingle) value
                : base.ConvertTo(context, culture, value, destinationType);
        }
    }

    [TypeConverter(typeof(FixedSingleTypeConverter))]
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
        public static readonly FixedSingle E = new(System.Math.E);
        public static readonly FixedSingle PI = new(System.Math.PI);
        public static readonly FixedSingle MIN_VALUE = new(int.MinValue);
        public static readonly FixedSingle MIN_POSITIVE_VALUE = new(1);
        public static readonly FixedSingle MAX_VALUE = new(int.MaxValue);

        public int RawValue
        {
            get;
        }

        public int IntValue => RawValue >> FIXED_BITS_COUNT;

        public uint RawFracPart => (uint) (RawValue & FRAC_PART_MASK);

        public FixedSingle FracPart => new(RawValue & FRAC_PART_MASK);

        public float FloatValue => (float) RawValue / FIXED_DIVISOR;

        public double DoubleValue => (double) RawValue / FIXED_DIVISOR;

        public FixedSingle Abs => new((RawValue + (RawValue >> 31)) ^ (RawValue >> 31));

        public int Signal => RawValue == 0 ? 0 : RawValue > 0 ? 1 : -1;

        private FixedSingle(int rawValue)
        {
            RawValue = rawValue;
        }

        public FixedSingle(float value)
        {
            RawValue = (int) (value * FIXED_DIVISOR);
        }

        public FixedSingle(double value)
        {
            RawValue = (int) (value * FIXED_DIVISOR);
        }

        public FixedSingle(BinaryReader reader)
        {
            RawValue = reader.ReadInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(RawValue);
        }

        public override string ToString()
        {
            return DoubleValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public override bool Equals(object obj)
        {
            return obj is FixedSingle single && single.RawValue == RawValue;
        }

        public override int GetHashCode()
        {
            return RawValue;
        }

        public static FixedSingle FromRawValue(int rawValue)
        {
            return new(rawValue);
        }

        public int CompareTo(FixedSingle other)
        {
            return this == other ? 0 : this > other ? 1 : -1;
        }

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

        public FixedSingle TruncFracPart(int bits = 8)
        {
            return new FixedSingle(RawValue & (-1 << (FIXED_BITS_COUNT - bits)));
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
            return left.RawValue == right.RawValue;
        }

        public static bool operator !=(FixedSingle left, FixedSingle right)
        {
            return left.RawValue != right.RawValue;
        }

        public static bool operator >(FixedSingle left, FixedSingle right)
        {
            return left.RawValue > right.RawValue;
        }

        public static bool operator >=(FixedSingle left, FixedSingle right)
        {
            return left.RawValue >= right.RawValue;
        }

        public static bool operator <(FixedSingle left, FixedSingle right)
        {
            return left.RawValue < right.RawValue;
        }

        public static bool operator <=(FixedSingle left, FixedSingle right)
        {
            return left.RawValue <= right.RawValue;
        }

        public static FixedSingle operator +(FixedSingle value)
        {
            return value;
        }

        public static FixedSingle operator +(FixedSingle left, FixedSingle right)
        {
            return new(left.RawValue + right.RawValue);
        }

        public static FixedSingle operator -(FixedSingle value)
        {
            return new(-value.RawValue);
        }

        public static FixedSingle operator -(FixedSingle left, FixedSingle right)
        {
            return new(left.RawValue - right.RawValue);
        }

        public static FixedSingle operator *(FixedSingle left, FixedSingle right)
        {
            return new((int) ((left.RawValue * (long) right.RawValue) >> FIXED_BITS_COUNT));
        }

        public static FixedSingle operator /(FixedSingle left, FixedSingle right)
        {
            return new((int) (((long) left.RawValue << FIXED_BITS_COUNT) / right.RawValue));
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
            return new(src << FIXED_BITS_COUNT);
        }

        public static implicit operator FixedSingle(float src)
        {
            return new(src);
        }

        public static implicit operator FixedSingle(double src)
        {
            return new(src);
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

    public class FixedDoubleTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(int)
                || sourceType == typeof(long)
                || sourceType == typeof(float)
                || sourceType == typeof(double)
                || sourceType == typeof(FixedSingle)
                || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            return value switch
            {                
                int => new FixedDouble(Convert.ToInt32(value, culture)),
                long => new FixedDouble(Convert.ToInt64(value, culture)),
                float => new FixedDouble(Convert.ToSingle(value, culture)),
                double => new FixedDouble(Convert.ToDouble(value, culture)),
                FixedSingle => new FixedDouble(Convert.ToSingle(value, culture)),
                _ => base.ConvertFrom(context, culture, value)
            };
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            return destinationType == typeof(double)
                ? (double) (FixedDouble) value
                : base.ConvertTo(context, culture, value, destinationType);
        }
    }

    [TypeConverter(typeof(FixedDoubleTypeConverter))]
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
        public static readonly FixedDouble E = new(System.Math.E);
        public static readonly FixedDouble PI = new(System.Math.PI);
        public static readonly FixedDouble MIN_VALUE = new(long.MinValue);
        public static readonly FixedDouble MIN_POSITIVE_VALUE = new(1L);
        public static readonly FixedDouble MAX_VALUE = new(long.MaxValue);

        public long RawValue
        {
            get;
        }

        public int IntValue => (int) (RawValue >> FIXED_BITS_COUNT);

        public long LongValue => RawValue >> FIXED_BITS_COUNT;

        public long RawFracPart => RawValue & FRAC_PART_MASK;

        public FixedDouble FracPart => new(RawValue & FRAC_PART_MASK);

        public float FloatValue => RawValue / (float) FIXED_DIVISOR;

        public double DoubleValue => RawValue / (double) FIXED_DIVISOR;

        public FixedDouble Abs => new((RawValue + (RawValue >> 63)) ^ (RawValue >> 63));

        public int Signal => RawValue == 0 ? 0 : RawValue > 0 ? 1 : -1;

        private FixedDouble(long rawValue)
        {
            RawValue = rawValue;
        }

        public FixedDouble(float value)
        {
            RawValue = (long) (value * FIXED_DIVISOR);
        }

        public FixedDouble(double value)
        {
            RawValue = (long) (value * FIXED_DIVISOR);
        }

        public FixedDouble(BinaryReader reader)
        {
            RawValue = reader.ReadInt64();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(RawValue);
        }

        public override string ToString()
        {
            return DoubleValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public override bool Equals(object obj)
        {
            return obj is FixedDouble @double && @double.RawValue == RawValue;
        }

        public override int GetHashCode()
        {
            return (int) RawValue;
        }

        public static FixedDouble FromRawValue(long rawValue)
        {
            return new(rawValue);
        }

        public int CompareTo(FixedDouble other)
        {
            return this == other ? 0 : this > other ? 1 : -1;
        }

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

        public FixedSingle TruncFracPart(int bits = 8)
        {
            return new FixedSingle(RawValue & (-1 << (FIXED_BITS_COUNT - bits)));
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
            return left.RawValue == right.RawValue;
        }

        public static bool operator !=(FixedDouble left, FixedDouble right)
        {
            return left.RawValue != right.RawValue;
        }

        public static bool operator >(FixedDouble left, FixedDouble right)
        {
            return left.RawValue > right.RawValue;
        }

        public static bool operator >=(FixedDouble left, FixedDouble right)
        {
            return left.RawValue >= right.RawValue;
        }

        public static bool operator <(FixedDouble left, FixedDouble right)
        {
            return left.RawValue < right.RawValue;
        }

        public static bool operator <=(FixedDouble left, FixedDouble right)
        {
            return left.RawValue <= right.RawValue;
        }

        public static FixedDouble operator +(FixedDouble value)
        {
            return value;
        }

        public static FixedDouble operator +(FixedDouble left, FixedDouble right)
        {
            return new(left.RawValue + right.RawValue);
        }

        public static FixedDouble operator -(FixedDouble value)
        {
            return new(-value.RawValue);
        }

        public static FixedDouble operator -(FixedDouble left, FixedDouble right)
        {
            return new(left.RawValue - right.RawValue);
        }

        public static FixedDouble operator *(FixedDouble left, FixedDouble right)
        {
            return new((left.RawValue * right.RawValue) >> FIXED_BITS_COUNT);
        }

        public static FixedDouble operator /(FixedDouble left, FixedDouble right)
        {
            return new((left.RawValue << FIXED_BITS_COUNT) / right.RawValue);
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
            return FixedSingle.FromRawValue((int) (src.RawValue >> (FIXED_BITS_COUNT - FixedSingle.FIXED_BITS_COUNT)));
        }

        public static implicit operator double(FixedDouble src)
        {
            return src.DoubleValue;
        }

        public static implicit operator FixedDouble(int src)
        {
            return new((long) src << FIXED_BITS_COUNT);
        }

        public static implicit operator FixedDouble(long src)
        {
            return new(src << FIXED_BITS_COUNT);
        }

        public static implicit operator FixedDouble(float src)
        {
            return new(src);
        }

        public static implicit operator FixedDouble(FixedSingle src)
        {
            return new((long) src.RawValue << (FIXED_BITS_COUNT - FixedSingle.FIXED_BITS_COUNT));
        }

        public static implicit operator FixedDouble(double src)
        {
            return new(src);
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
            var hashCode = -1682582155;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<FixedSingle>.Default.GetHashCode(Min);
            hashCode = hashCode * -1521134295 + IsClosedLeft.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<FixedSingle>.Default.GetHashCode(Max);
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
    }
}
