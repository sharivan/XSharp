using System;
using System.ComponentModel;
using System.Globalization;

namespace XSharp.Math.Fixed;

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

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
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

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        return destinationType == typeof(double)
            ? (double) (FixedDouble) value
            : base.ConvertTo(context, culture, value, destinationType);
    }
}

[TypeConverter(typeof(FixedDoubleTypeConverter))]
public readonly struct FixedDouble : IComparable<FixedDouble>
{
    public const int FIXED_BITS_COUNT = 16;
    public const long RAW_ONE = 1L << FIXED_BITS_COUNT;

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

    public int IntValue => (int) LongValue;

    public long LongValue
    {
        get
        {
            long result = RawValue >> FIXED_BITS_COUNT;
            if (result < 0 && RawFracPart > 0)
                result++;

            return result;
        }
    }

    public long RawFracPart => RawValue & FRAC_PART_MASK;

    public FixedDouble FracPart => this - LongValue;

    public float FloatValue => RawValue / (float) RAW_ONE;

    public double DoubleValue => RawValue / (double) RAW_ONE;

    public FixedDouble Abs
    {
        get
        {
            var mask = RawValue >> 63;
            return new(RawValue + mask ^ mask);
        }
    }

    public int Signal => RawValue == 0 ? 0 : RawValue > 0 ? 1 : -1;

    private FixedDouble(long rawValue)
    {
        RawValue = rawValue;
    }

    public FixedDouble(float value)
    {
        RawValue = (long) (value * RAW_ONE);
    }

    public FixedDouble(double value)
    {
        RawValue = (long) (value * RAW_ONE);
    }

    public string ToString(FixedStringFormat format)
    {
        switch (format)
        {
            case FixedStringFormat.DECIMAL:
                return DoubleValue.ToString(CultureInfo.InvariantCulture);

            case FixedStringFormat.SUBPIXEL:
                return ((double) RawValue / (1 << FIXED_BITS_COUNT - 8)).ToString(CultureInfo.InvariantCulture);

            case FixedStringFormat.PIXEL_SUBPIXEL:
            {
                var pixel = LongValue;
                var subPixel = (double) (RawValue & FIXED_BITS_COUNT) / (1 << FIXED_BITS_COUNT - 8);
                if (subPixel == 0)
                    return pixel.ToString(CultureInfo.InvariantCulture);

                return $"_{pixel.ToString(CultureInfo.InvariantCulture)}_{subPixel.ToString(CultureInfo.InvariantCulture)}";
            }

            case FixedStringFormat.INT_FRAC:
            {
                var intPart = LongValue;
                var fracPart = RawFracPart;
                if (fracPart == 0)
                    return intPart.ToString(CultureInfo.InvariantCulture);

                return $"{intPart.ToString(CultureInfo.InvariantCulture)}|{fracPart.ToString(CultureInfo.InvariantCulture)}";
            }

            case FixedStringFormat.RAW:
                return $"|{RawValue.ToString(CultureInfo.InvariantCulture)}";
        }

        return "?";
    }

    public override string ToString()
    {
        return DoubleValue.ToString(CultureInfo.InvariantCulture);
    }

    public override bool Equals(object obj)
    {
        return obj is FixedDouble @double && @double.RawValue == RawValue;
    }

    public override int GetHashCode()
    {
        return (int) (RawValue >> 16);
    }

    public static FixedDouble FromRawValue(long rawValue)
    {
        return new(rawValue);
    }

    public static FixedDouble FromSubpixels(double subPixels)
    {
        return new FixedDouble(subPixels / 256.0);
    }

    public static FixedDouble FromPixelsAndSubPixels(int pixels, double subPixels)
    {
        return new FixedDouble(pixels + subPixels / 256.0);
    }

    public static FixedDouble FromIntAndRawFracPart(int intPart, long rawFracPart)
    {
        return new FixedDouble((intPart << FIXED_BITS_COUNT) + rawFracPart);
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
            ? RawFracPart > 1 << FIXED_BITS_COUNT - 1 ? intPart + 1 : intPart
            : RawFracPart > 1 << FIXED_BITS_COUNT - 1 ? intPart - 1 : intPart;
    }

    public long Round(RoundMode mode)
    {
        return mode switch
        {
            RoundMode.FLOOR => Floor(),
            RoundMode.CEIL => Ceil(),
            RoundMode.TRUNCATE => (long) this,
            RoundMode.NEAREST => Round(),
            _ => throw new ArgumentException("Invalid round mode", nameof(mode))
        };
    }

    public FixedDouble TruncFracPart(int bits = 8)
    {
        return new FixedDouble(RawValue & -1 << FIXED_BITS_COUNT - bits);
    }

    public FixedDouble Clamp(FixedDouble min, FixedDouble max)
    {
        return this < min ? min : this > max ? max : this;
    }

    public FixedDouble Sqrt()
    {
        if (RawValue < 0)
            throw new ArgumentOutOfRangeException("Negative square root.");

        long val = RawValue << FIXED_BITS_COUNT;
        long res = 0;
        long bit = 1L << sizeof(long) - 2;

        while (bit > val)
            bit >>= 2;

        while (bit != 0)
        {
            if (val >= res + bit)
            {
                val -= res + bit;
                res = (res >> 1) + bit;
            }
            else
            {
                res >>= 1;
            }

            bit >>= 2;
        }

        return new FixedDouble(res);
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
        return new(left.RawValue * right.RawValue >> FIXED_BITS_COUNT);
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
        return FixedSingle.FromRawValue((int) (src.RawValue >> FIXED_BITS_COUNT - FixedSingle.FIXED_BITS_COUNT));
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
        return new((long) src.RawValue << FIXED_BITS_COUNT - FixedSingle.FIXED_BITS_COUNT);
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