using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Reflection.Metadata;

namespace XSharp.Math.Fixed;

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

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
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

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
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
public readonly struct FixedSingle : ISignedNumber<FixedSingle>
{
    public const int FIXED_BITS_COUNT = 16;
    public const int RAW_ONE = 1 << FIXED_BITS_COUNT;

    private const int INT_PART_MASK = -1 << FIXED_BITS_COUNT;
    private const int FRAC_PART_MASK = ~INT_PART_MASK;

    public static readonly FixedSingle MINUS_ONE = FromInt(-1);
    public static readonly FixedSingle ZERO = FromInt(0);
    public static readonly FixedSingle ONE = FromInt(1);
    public static readonly FixedSingle SUBPIXEL = ONE >> 8;    
    public static readonly FixedSingle QUARTER = ONE >> 2;
    public static readonly FixedSingle HALF = ONE >> 1;   
    public static readonly FixedSingle TWO = FromInt(2);
    public static readonly FixedSingle FOUR = FromInt(4);
    public static readonly FixedSingle FIVE = FromInt(5);
    public static readonly FixedSingle TEN = FromInt(10);
    public static readonly FixedSingle ONE_HUNDRED = FromInt(100);
    public static readonly FixedSingle PIXEL = FromInt(256); // subpixels
    public static readonly FixedSingle ONE_TOUSAND = FromInt(1000);
    public static readonly FixedSingle SQRT_2 = new(System.Math.Sqrt(2));
    public static readonly FixedSingle SQRT_2_INVERSE = new(System.Math.Sqrt(0.5));
    public static readonly FixedSingle SQRT_3 = new(System.Math.Sqrt(3));
    public static readonly FixedSingle SQRT_3_INVERSE = new(System.Math.Sqrt(1.0 / 3));
    public static readonly FixedSingle SQRT_5 = new(System.Math.Sqrt(5));
    public static readonly FixedSingle SQRT_5_INVERSE = new(System.Math.Sqrt(0.2));
    public static readonly FixedSingle E = new(System.Math.E);
    public static readonly FixedSingle LN2 = new(System.Math.Log(2));
    public static readonly FixedSingle LN10 = new(System.Math.Log(10));
    public static readonly FixedSingle PI = new(System.Math.PI);
    public static readonly FixedSingle TWO_PI = new(System.Math.PI * 2);
    public static readonly FixedSingle HALF_PI = new(System.Math.PI / 2);
    public static readonly FixedSingle THIRD_PI = new(System.Math.PI / 3);
    public static readonly FixedSingle QUARTER_PI = new(System.Math.PI / 4);
    public static readonly FixedSingle MIN_VALUE = new(int.MinValue);
    public static readonly FixedSingle MIN_POSITIVE_VALUE = new(1);
    public static readonly FixedSingle MAX_VALUE = new(int.MaxValue);

    private const int EXP_LUT_SIZE = 256;
    private static readonly FixedSingle[] expLUT = GenerateExpLUT();

    private static FixedSingle[] GenerateExpLUT()
    {
        var table = new FixedSingle[EXP_LUT_SIZE + 1];
        for (int i = 0; i <= EXP_LUT_SIZE; i++)
        {
            double x = 5.5 * i / EXP_LUT_SIZE;
            int val = (int) (System.Math.Exp(x) * RAW_ONE);
            table[i] = new FixedSingle(val);
        }

        return table;
    }

    private const int LOG_LUT_SIZE = 256;
    private static readonly FixedSingle[] logLUT = GenerateLogLUT();

    private static FixedSingle[] GenerateLogLUT()
    {
        var table = new FixedSingle[LOG_LUT_SIZE + 1];
        for (int i = 1; i <= LOG_LUT_SIZE; i++)
        {
            double x = i;
            int val = (int) (System.Math.Log(x) * RAW_ONE);
            table[i] = new FixedSingle(val);
        }

        return table;
    }

    private const int SIN_LUT_SIZE = 1024;
    private static readonly FixedSingle[] sinLUT = GenerateSinLUT();

    private static FixedSingle[] GenerateSinLUT()
    {
        var table = new FixedSingle[SIN_LUT_SIZE];
        for (int i = 0; i < SIN_LUT_SIZE; i++)
        {
            // angles between 0 e 2π
            double angle = 2.0 * System.Math.PI * i / SIN_LUT_SIZE;
            int value = (int) (System.Math.Sin(angle) * RAW_ONE);
            table[i] = new FixedSingle(value);
        }

        return table;
    }

    private const int ATAN_LUT_SIZE = 1024;
    private static readonly int[] AtanLUT = GenerateAtanLUT();

    private static int[] GenerateAtanLUT()
    {
        int[] lut = new int[ATAN_LUT_SIZE];
        for (int i = 0; i < ATAN_LUT_SIZE; i++)
        {
            double val = i / (double) ATAN_LUT_SIZE * 8.0 - 4.0;
            lut[i] = (int) (System.Math.Atan(val) * RAW_ONE);
        }

        return lut;
    }

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

    public float FloatValue => (float) RawValue / RAW_ONE;

    public double DoubleValue => (double) RawValue / RAW_ONE;

    public FixedSingle Abs
    {
        get
        {
            var mask = RawValue >> 31;
            return new(RawValue + mask ^ mask);
        }
    }

    public int Signal => RawValue == 0 ? 0 : RawValue > 0 ? 1 : -1;

    public static FixedSingle One => ONE;

    public static int Radix => throw new NotImplementedException();

    public static FixedSingle Zero => ZERO;

    public static FixedSingle AdditiveIdentity => ZERO;

    public static FixedSingle MultiplicativeIdentity => ONE;

    public static FixedSingle NegativeOne => MINUS_ONE;

    private FixedSingle(int rawValue)
    {
        RawValue = rawValue;
    }

    public FixedSingle(float value)
    {
        RawValue = (int) (value * RAW_ONE);
    }

    public FixedSingle(double value)
    {
        RawValue = (int) (value * RAW_ONE);
    }

    public FixedSingle(int pixel, double subPixel) : this(pixel + subPixel / 256.0)
    {
    }

    public string ToString(FixedStringFormat format)
    {
        return ToString(format, null, CultureInfo.InvariantCulture);
    }

    public string ToString(FixedStringFormat format, string? decimalFormat, IFormatProvider? formatProvider)
    {
        switch (format)
        {
            case FixedStringFormat.DECIMAL:
                return DoubleValue.ToString(decimalFormat, formatProvider);

            case FixedStringFormat.SUBPIXEL:
                return ((float) RawValue / (1 << FIXED_BITS_COUNT - 8)).ToString(decimalFormat, formatProvider);

            case FixedStringFormat.PIXEL_SUBPIXEL:
            {
                var pixel = IntValue;
                var subPixel = (float) (RawValue & FIXED_BITS_COUNT) / (1 << FIXED_BITS_COUNT - 8);
                if (subPixel == 0)
                    return pixel.ToString(decimalFormat, formatProvider);

                return $"_{pixel.ToString(decimalFormat, formatProvider)}_{subPixel.ToString(decimalFormat, formatProvider)}";
            }

            case FixedStringFormat.INT_FRAC:
            {
                var intPart = IntValue;
                var fracPart = RawFracPart;
                if (fracPart == 0)
                    return intPart.ToString(decimalFormat, formatProvider);

                return $"{intPart.ToString(decimalFormat, formatProvider)}|{fracPart.ToString(decimalFormat, formatProvider)}";
            }

            case FixedStringFormat.RAW:
                return $"|{RawValue.ToString(decimalFormat, formatProvider)}";
        }

        return "?";
    }

    public override string ToString()
    {
        return ToString(FixedStringFormat.DECIMAL, null, CultureInfo.InvariantCulture);
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

    public static FixedSingle FromSubpixels(int subPixels)
    {
        return new FixedSingle(0, subPixels);
    }

    public static FixedSingle FromPixelsAndSubPixels(int pixels, int subPixels)
    {
        return new FixedSingle(pixels, subPixels);
    }

    public static FixedSingle FromInt(int intPart)
    {
        return new FixedSingle(intPart << FIXED_BITS_COUNT);
    }

    public static FixedSingle FromIntAndRawFracPart(int intPart, int rawFracPart)
    {
        return new FixedSingle((intPart << FIXED_BITS_COUNT) + rawFracPart);
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
            ? RawFracPart > 1 << FIXED_BITS_COUNT - 1 ? intPart + 1 : intPart
            : RawFracPart > 1 << FIXED_BITS_COUNT - 1 ? intPart - 1 : intPart;
    }

    public int Round(RoundMode mode)
    {
        return mode switch
        {
            RoundMode.FLOOR => Floor(),
            RoundMode.CEIL => Ceil(),
            RoundMode.TRUNCATE => (int) this,
            RoundMode.NEAREST => Round(),
            _ => throw new ArgumentException("Invalid round mode", nameof(mode))
        };
    }

    public FixedSingle TruncFracPart(int bits = 8)
    {
        return new FixedSingle(RawValue & -1 << FIXED_BITS_COUNT - bits);
    }

    public FixedSingle Clamp(FixedSingle min, FixedSingle max)
    {
        return this < min ? min : this > max ? max : this;
    }

    public FixedSingle Sqrt()
    {
        if (RawValue < 0)
            throw new ArgumentOutOfRangeException("Negative argument");

        /*long val = (long) RawValue << FIXED_BITS_COUNT;
        long res = 0;
        long bit = 1L << (sizeof(int) - 2);

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

        return new FixedSingle((int) res);*/

        if (RawValue == 0)
            return ZERO;

        long l1 = 0;
        long l2 = RAW_ONE;
        do
        {
            l1 ^= l2;
            if (l1 * l1 > RawValue)
                l1 ^= l2;
        }
        while ((l2 >>= 1) != 0L);

        return new((int) (l1 << 8));
    }

    private static FixedSingle NormalizeAngleUnsigned(FixedSingle angle)
    {
        int r = angle.RawValue % TWO_PI.RawValue;
        return r < 0 ? new FixedSingle(r + TWO_PI) : new FixedSingle(r);
    }

    public FixedSingle Exp()
    {
        if (RawValue < 0)
            return ONE / (-this).Exp();

        if (RawValue >= FromInt(6).RawValue)
            throw new ArgumentOutOfRangeException("Overflow");

        int scaled = RawValue * EXP_LUT_SIZE / (int) (5.5 * ONE);
        int index = scaled >> FIXED_BITS_COUNT;
        int frac = scaled & FRAC_PART_MASK;

        var a = expLUT[index];
        var b = expLUT[index + 1];
        var delta = b - a;
        return a + ((delta * new FixedSingle(frac)) >> FIXED_BITS_COUNT);
    }

    public FixedSingle Log()
    {
        return Ln() / LN10;
    }

    public FixedSingle Ln()
    {
        if (RawValue <= 0)
            throw new ArgumentOutOfRangeException("Negative argument");

        int intPart = IntValue;
        int frac = RawValue & FRAC_PART_MASK;

        if (intPart >= LOG_LUT_SIZE)
            throw new ArgumentOutOfRangeException("Overflow");

        var a = logLUT[intPart];
        var b = logLUT[intPart + 1];
        var delta = b - a;
        return a + ((delta * new FixedSingle(frac)) >> FIXED_BITS_COUNT);
    }

    public FixedSingle Sin()
    {
        var angle = NormalizeAngleUnsigned(this);
        int index = (int) ((long) angle.RawValue * SIN_LUT_SIZE / TWO_PI.RawValue);
        return sinLUT[index % SIN_LUT_SIZE];
    }

    public FixedSingle Cos()
    {
        return (this + HALF_PI).Sin();
    }

    public FixedSingle Tan()
    {
        var cos = Cos();
        if (cos == 0)
            throw new DivideByZeroException();

        return Sin() / cos;
    }

    public FixedSingle Asin()
    {
        if (this < -ONE || this > ONE)
            throw new ArgumentOutOfRangeException();

        return (this / (ONE - this * this).Sqrt()).Atan();
    }

    public FixedSingle Acos()
    {
        return HALF_PI - Asin();
    }

    public FixedSingle Atan()
    {
        int index = ((RawValue + RAW_ONE * 4) * (ATAN_LUT_SIZE / 8)) >> FIXED_BITS_COUNT;
        if (index < 0)
            index = 0;

        if (index >= AtanLUT.Length)
            index = AtanLUT.Length - 1;

        return new(AtanLUT[index]);
    }

    public static FixedSingle Atan2(FixedSingle y, FixedSingle x)
    {
        return System.Math.Atan2(y.DoubleValue, x.DoubleValue);
    }

    public FixedSingle Sinh()
    {
        return (Exp() - (-this).Exp()) / TWO;
    }

    public FixedSingle Cosh()
    {
        return (Exp() + (-this).Exp()) / TWO;
    }

    public FixedSingle Tanh()
    {
        var epx = Exp();
        var enx = (-this).Exp();
        return (epx - enx) / (epx + enx);
    }

    public FixedSingle Asinh()
    {
        return (this + (this * this + ONE).Sqrt()).Ln();
    }

    public FixedSingle Acosh()
    {
        return (this + (this * this + ONE).Sqrt()).Ln();
    }

    public FixedSingle Atanh()
    {
        if (this <= -ONE || this >= ONE)
            throw new ArgumentOutOfRangeException();

        return ((ONE + this) / (ONE - this)).Ln() / TWO;
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
        return new((int) (left.RawValue * (long) right.RawValue >> FIXED_BITS_COUNT));
    }

    public static FixedSingle operator /(FixedSingle left, FixedSingle right)
    {
        return new((int) (((long) left.RawValue << FIXED_BITS_COUNT) / right.RawValue));
    }

    public static FixedSingle operator %(FixedSingle left, FixedSingle right)
    {
        return new(left.RawValue % right.RawValue);
    }

    public static FixedSingle operator <<(FixedSingle left, int bits)
    {
        return new(left.RawValue << bits);
    }

    public static FixedSingle operator >>(FixedSingle left, int bits)
    {
        return new(left.RawValue >> bits);
    }

    public static FixedSingle operator --(FixedSingle value)
    {
        return value - 1;
    }

    public static FixedSingle operator ++(FixedSingle value)
    {
        return value + 1;
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
        return FromInt(src);
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

    public int CompareTo(object? obj)
    {
        throw new NotImplementedException();
    }

    static FixedSingle INumberBase<FixedSingle>.Abs(FixedSingle value)
    {
        return value.Abs;
    }

    public static bool IsCanonical(FixedSingle value)
    {
        throw new NotImplementedException();
    }

    public static bool IsComplexNumber(FixedSingle value)
    {
        return false;
    }

    public static bool IsEvenInteger(FixedSingle value)
    {
        return value.IntValue % 2 == 0 && value.FracPart == 0;
    }

    public static bool IsFinite(FixedSingle value)
    {
        return true;
    }

    public static bool IsImaginaryNumber(FixedSingle value)
    {
        return false;
    }

    public static bool IsInfinity(FixedSingle value)
    {
        return false;
    }

    public static bool IsInteger(FixedSingle value)
    {
        return value.FracPart == 0;
    }

    public static bool IsNaN(FixedSingle value)
    {
        return false;
    }

    public static bool IsNegative(FixedSingle value)
    {
        return value < 0;
    }

    public static bool IsNegativeInfinity(FixedSingle value)
    {
        return false;
    }

    public static bool IsNormal(FixedSingle value)
    {
        throw new NotImplementedException();
    }

    public static bool IsOddInteger(FixedSingle value)
    {
        return value.IntValue % 2 != 0 && value.FracPart == 0;
    }

    public static bool IsPositive(FixedSingle value)
    {
        return value > 0;
    }

    public static bool IsPositiveInfinity(FixedSingle value)
    {
        return false;
    }

    public static bool IsRealNumber(FixedSingle value)
    {
        return true;
    }

    public static bool IsSubnormal(FixedSingle value)
    {
        throw new NotImplementedException();
    }

    public static bool IsZero(FixedSingle value)
    {
        return value == 0;
    }

    public static FixedSingle MaxMagnitude(FixedSingle x, FixedSingle y)
    {
        throw new NotImplementedException();
    }

    public static FixedSingle MaxMagnitudeNumber(FixedSingle x, FixedSingle y)
    {
        throw new NotImplementedException();
    }

    public static FixedSingle MinMagnitude(FixedSingle x, FixedSingle y)
    {
        throw new NotImplementedException();
    }

    public static FixedSingle MinMagnitudeNumber(FixedSingle x, FixedSingle y)
    {
        throw new NotImplementedException();
    }

    public static FixedSingle Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static FixedSingle Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out FixedSingle result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out FixedSingle result)
    {
        throw new NotImplementedException();
    }

    public bool Equals(FixedSingle other)
    {
        return this == other;
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return ToString(FixedStringFormat.DECIMAL, format, formatProvider);
    }

    public static FixedSingle Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out FixedSingle result)
    {
        throw new NotImplementedException();
    }

    public static FixedSingle Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out FixedSingle result)
    {
        throw new NotImplementedException();
    }

    static bool INumberBase<FixedSingle>.TryConvertFromChecked<TOther>(TOther value, out FixedSingle result)
    {
        throw new NotImplementedException();
    }

    static bool INumberBase<FixedSingle>.TryConvertFromSaturating<TOther>(TOther value, out FixedSingle result)
    {
        throw new NotImplementedException();
    }

    static bool INumberBase<FixedSingle>.TryConvertFromTruncating<TOther>(TOther value, out FixedSingle result)
    {
        throw new NotImplementedException();
    }

    static bool INumberBase<FixedSingle>.TryConvertToChecked<TOther>(FixedSingle value, out TOther result)
    {
        throw new NotImplementedException();
    }

    static bool INumberBase<FixedSingle>.TryConvertToSaturating<TOther>(FixedSingle value, out TOther result)
    {
        throw new NotImplementedException();
    }

    static bool INumberBase<FixedSingle>.TryConvertToTruncating<TOther>(FixedSingle value, out TOther result)
    {
        throw new NotImplementedException();
    }
}