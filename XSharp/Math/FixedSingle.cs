using System;
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

        public int IntValue
        {
            get
            {
                int result = RawValue >> FIXED_BITS_COUNT;
                if (result < 0 && RawFracPart > 0)
                    result++;

                return result;
            }
        }

        public uint RawFracPart => (uint) (RawValue & FRAC_PART_MASK);

        public FixedSingle FracPart => this - IntValue;

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

        public FixedSingle Round(RoundMode mode)
        {
            return mode switch
            {
                RoundMode.FLOOR => Floor(),
                RoundMode.CEIL => Ceil(),
                RoundMode.TRUNCATE => (int) this,
                RoundMode.NEAREST => Round(),
                _ => this
            };
        }

        public FixedSingle TruncFracPart(int bits = 8)
        {
            return new FixedSingle(RawValue & (-1 << (FIXED_BITS_COUNT - bits)));
        }

        public FixedSingle Clamp(FixedSingle min, FixedSingle max)
        {
            return this < min ? min : this > max ? max : this;
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
}