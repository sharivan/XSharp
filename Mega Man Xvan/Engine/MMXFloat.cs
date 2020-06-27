using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMX.Engine
{
    public struct MMXFloat
    {
        public const int FIXED_BITS_COUNT = 9;
        public const int FIXED_DIVISOR = 1 << FIXED_BITS_COUNT;

        private const int INT_PART_MASK = -1 << FIXED_BITS_COUNT;
        private const int FRAC_PART_MASK = ~INT_PART_MASK;

        public static readonly MMXFloat MINUS_ONE = new MMXFloat(-1F);
        public static readonly MMXFloat ZERO = new MMXFloat(0);
        public static readonly MMXFloat ONE = new MMXFloat(1F);
        public static readonly MMXFloat TWO = new MMXFloat(2F);
        public static readonly MMXFloat MIN_VALUE = new MMXFloat(Int32.MinValue);
        public static readonly MMXFloat MAX_VALUE = new MMXFloat(Int32.MaxValue);

        private int rawValue;

        public int RawValue
        {
            get
            {
                return rawValue;
            }

            set
            {
                rawValue = value;
            }
        }

        public int IntValue
        {
            get
            {
                return rawValue >> FIXED_BITS_COUNT;
            }

            set
            {
                rawValue = value << FIXED_BITS_COUNT;
            }
        }

        public float FloatValue
        {
            get
            {
                return (float) rawValue / FIXED_DIVISOR;
            }

            set
            {
                rawValue = (int) (value * FIXED_DIVISOR);
            }
        }

        public double DoubleValue
        {
            get
            {
                return (double) rawValue / FIXED_DIVISOR;
            }

            set
            {
                rawValue = (int) (value * FIXED_DIVISOR);
            }
        }

        public MMXFloat Abs
        {
            get
            {
                return new MMXFloat((rawValue + (rawValue >> 31)) ^ (rawValue >> 31));
            }
        }

        public int Signal
        {
            get
            {
                return rawValue == 0 ? 0 : rawValue > 0 ? 1 : -1;
            }
        }

        private MMXFloat(int rawValue)
        {
            this.rawValue = rawValue;
        }

        public MMXFloat(float value)
        {
            rawValue = (int) (value * FIXED_DIVISOR);
        }

        public MMXFloat(double value)
        {
            rawValue = (int) (value * FIXED_DIVISOR);
        }

        public MMXFloat(BinaryReader reader)
        {
            rawValue = reader.ReadInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(rawValue);
        }

        public override string ToString()
        {
            return DoubleValue.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is MMXFloat)
                return ((MMXFloat) obj).rawValue == rawValue;

            return false;
        }

        public override int GetHashCode()
        {
            return rawValue;
        }

        public static MMXFloat FromRawValue(int rawValue)
        {
            return new MMXFloat(rawValue);
        }

        public MMXFloat RoundToCeil()
        {
            int frac = rawValue & FRAC_PART_MASK;
            return new MMXFloat(frac > (1 << (FIXED_BITS_COUNT - 1)) ? (rawValue & INT_PART_MASK) + ONE.rawValue : rawValue);
        }

        public MMXFloat RoundToFloor()
        {
            int frac = rawValue & FRAC_PART_MASK;
            return new MMXFloat(frac <= (1 << (FIXED_BITS_COUNT - 1)) ? rawValue & INT_PART_MASK : rawValue);
        }

        public MMXFloat Round(MMXFloat delta)
        {
            return delta > 0 ? RoundToCeil() : delta < 0 ? RoundToFloor() : this;
        }

        public static bool operator ==(MMXFloat left, MMXFloat right)
        {
            return left.rawValue == right.rawValue;
        }

        public static bool operator !=(MMXFloat left, MMXFloat right)
        {
            return left.rawValue != right.rawValue;
        }

        public static bool operator >(MMXFloat left, MMXFloat right)
        {
            return left.rawValue > right.rawValue;
        }

        public static bool operator >=(MMXFloat left, MMXFloat right)
        {
            return left.rawValue >= right.rawValue;
        }

        public static bool operator <(MMXFloat left, MMXFloat right)
        {
            return left.rawValue < right.rawValue;
        }

        public static bool operator <=(MMXFloat left, MMXFloat right)
        {
            return left.rawValue <= right.rawValue;
        }

        public static MMXFloat operator +(MMXFloat value)
        {
            return value;
        }

        public static MMXFloat operator +(MMXFloat left, MMXFloat right)
        {
            return new MMXFloat(left.rawValue + right.rawValue);
        }

        public static MMXFloat operator -(MMXFloat value)
        {
            return new MMXFloat(-value.rawValue);
        }

        public static MMXFloat operator -(MMXFloat left, MMXFloat right)
        {
            return new MMXFloat(left.rawValue - right.rawValue);
        }

        public static MMXFloat operator *(MMXFloat left, MMXFloat right)
        {
            return new MMXFloat((int) (((long) left.rawValue * (long) right.rawValue) >> FIXED_BITS_COUNT));
        }

        public static MMXFloat operator /(MMXFloat left, MMXFloat right)
        {
            return new MMXFloat((int) (((long) left.rawValue << FIXED_BITS_COUNT) / right.rawValue));
        }

        public static explicit operator int(MMXFloat src)
        {
            return src.IntValue;
        }

        public static explicit operator float(MMXFloat src)
        {
            return src.FloatValue;
        }

        public static implicit operator double(MMXFloat src)
        {
            return src.DoubleValue;
        }

        public static implicit operator MMXFloat(int src)
        {
            return new MMXFloat(src << FIXED_BITS_COUNT);
        }

        public static implicit operator MMXFloat(float src)
        {
            return new MMXFloat(src);
        }

        public static implicit operator MMXFloat(double src)
        {
            return new MMXFloat(src);
        }

        public static MMXFloat Min(MMXFloat f1, MMXFloat f2)
        {
            return f1 < f2 ? f1 : f2;
        }

        public static MMXFloat Max(MMXFloat f1, MMXFloat f2)
        {
            return f1 > f2 ? f1 : f2;
        }
    }
}
