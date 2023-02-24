using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace XSharp.Engine
{
    public class BitSet
    {
        private const int BITS_PER_SLOT = 8 * sizeof(ulong);

        private static readonly ulong[] MASK =
        {
            1UL << 0,
            1UL << 1,
            1UL << 2,
            1UL << 3,
            1UL << 4,
            1UL << 5,
            1UL << 6,
            1UL << 7,
            1UL << 8,
            1UL << 9,
            1UL << 10,
            1UL << 11,
            1UL << 12,
            1UL << 13,
            1UL << 14,
            1UL << 15,
            1UL << 16,
            1UL << 17,
            1UL << 18,
            1UL << 19,
            1UL << 20,
            1UL << 21,
            1UL << 22,
            1UL << 23,
            1UL << 24,
            1UL << 25,
            1UL << 26,
            1UL << 27,
            1UL << 28,
            1UL << 29,
            1UL << 30,
            1UL << 31,
            1UL << 32,
            1UL << 33,
            1UL << 34,
            1UL << 35,
            1UL << 36,
            1UL << 37,
            1UL << 38,
            1UL << 39,
            1UL << 40,
            1UL << 41,
            1UL << 42,
            1UL << 43,
            1UL << 44,
            1UL << 45,
            1UL << 46,
            1UL << 47,
            1UL << 48,
            1UL << 49,
            1UL << 50,
            1UL << 51,
            1UL << 52,
            1UL << 53,
            1UL << 54,
            1UL << 55,
            1UL << 56,
            1UL << 57,
            1UL << 58,
            1UL << 59,
            1UL << 60,
            1UL << 61,
            1UL << 62,
            1UL << 63
        };

        // Types and constants used in the functions below
        private const ulong M1 = 0x5555555555555555;  // Binary: 0101...
        private const ulong M2 = 0x3333333333333333;  // Binary: 00110011..
        private const ulong M4 = 0x0f0f0f0f0f0f0f0f;  // Binary:  4 zeros,  4 ones ...
        private const ulong H01 = 0x0101010101010101; // The sum of 256 to the power of 0,1,2,3...

        private const ulong DeBruijnSequence = 0x37E84A99DAE458F;

        private static readonly int[] MultiplyDeBruijnBitPosition =
        {
            0, 1, 17, 2, 18, 50, 3, 57,
            47, 19, 22, 51, 29, 4, 33, 58,
            15, 48, 20, 27, 25, 23, 52, 41,
            54, 30, 38, 5, 43, 34, 59, 8,
            63, 16, 49, 56, 46, 21, 28, 32,
            14, 26, 24, 40, 53, 37, 42, 7,
            62, 55, 45, 31, 13, 39, 36, 6,
            61, 44, 12, 35, 60, 11, 10, 9,
        };

        // This uses fewer arithmetic operations than any other known implementation on machines with fast multiplication.
        // This algorithm uses 12 arithmetic operations, one of which is a multiply.
        private static int PopCount64(ulong x)
        {
            x -= (x >> 1) & M1;             // Put count of each 2 bits into those 2 bits
            x = (x & M2) + ((x >> 2) & M2); // Put count of each 4 bits into those 4 bits 
            x = (x + (x >> 4)) & M4;        // Put count of each 8 bits into those 8 bits 
            return (int) ((x * H01) >> 56); // Returns left 8 bits of x + (x<<8) + (x<<16) + (x<<24) + ... 
        }

        /// <summary>
        /// Search the mask data from least significant bit (LSB) to the most significant bit (MSB) for a set bit (1)
        /// using De Bruijn sequence approach. Warning: Will return zero for b = 0.
        /// </summary>
        /// <param name="b">Target number.</param>
        /// <returns>Zero-based position of LSB (from right to left).</returns>
        private static int BitScanForward(ulong b)
        {
            Debug.Assert(b > 0, "Target number should not be zero");
            return MultiplyDeBruijnBitPosition[((ulong) ((long) b & -(long) b) * DeBruijnSequence) >> 58];
        }

        private static string ULongToBinaryString(ulong theNumber)
        {
            return Convert.ToString((long) theNumber, 2).PadLeft(BITS_PER_SLOT, '0');
        }

        private List<ulong> bits;

        public int Count
        {
            get
            {
                int result = 0;
                foreach (var slotBits in bits)
                    result += PopCount64(slotBits);

                return result;
            }
        }

        public int SlotCount
        {
            get => bits.Count;

            set
            {
                int currentSlotCount = SlotCount;
                if (value > currentSlotCount)
                {  
                    for (int i = 0; i < value - currentSlotCount; i++)
                        bits.Add(0);
                }
                else if (value < currentSlotCount)
                    bits.RemoveRange(value, currentSlotCount - value);
            }
        }

        public int BitCount => bits.Count * BITS_PER_SLOT;

        public BitSet(int initialSlotCount = 1)
        {
            if (initialSlotCount < 0)
                throw new ArgumentException($"Invalid negative initial slot count value '{initialSlotCount}'.");

            bits = new List<ulong>();
            SlotCount = initialSlotCount;
        }

        private ulong GetSlot(int slotIndex)
        {
            if (SlotCount <= slotIndex)
                SlotCount = slotIndex + 1;

            return bits[slotIndex];
        }

        private void SetSlotIncluding(int slotIndex, ulong value)
        {
            if (SlotCount <= slotIndex)
                SlotCount = slotIndex + 1;

            bits[slotIndex] |= value;
        }

        private void SetSlotExcluding(int slotIndex, ulong value)
        {
            if (SlotCount <= slotIndex)
                SlotCount = slotIndex + 1;

            bits[slotIndex] &= ~value;
        }

        public bool Set(int index)
        {
            int slotIndex = index / BITS_PER_SLOT;
            int bitIndex = index % BITS_PER_SLOT;
            ulong slotBits = GetSlot(slotIndex);
            ulong bitMask = MASK[bitIndex];

            SetSlotIncluding(slotIndex, bitMask);
            return (slotBits & bitMask) != 0;
        }

        public void Set(BitSet mask)
        {
            if (SlotCount < mask.SlotCount)
                SlotCount = mask.SlotCount;

            for (int i = 0; i < mask.SlotCount; i++)
            {
                ulong maskSlotBits = mask.bits[i];
                bits[i] |= maskSlotBits;
            }
        }

        public bool Reset(int index)
        {
            int slotIndex = index / BITS_PER_SLOT;
            int bitIndex = index % BITS_PER_SLOT;
            ulong slotBits = GetSlot(slotIndex);
            ulong bitMask = MASK[bitIndex];

            SetSlotExcluding(slotIndex, bitMask);
            return (slotBits & bitMask) != 0;
        }

        public void Reset(BitSet mask)
        {
            if (SlotCount < mask.SlotCount)
                SlotCount = mask.SlotCount;

            for (int i = 0; i < mask.SlotCount; i++)
            {
                ulong maskSlotBits = mask.bits[i];
                bits[i] &= ~maskSlotBits;
            }
        }

        public bool Toggle(int index)
        {
            bool test = Test(index);
            if (test)
                Reset(index);
            else
                Set(index);

            return test;
        }

        public void Toggle(BitSet mask)
        {
            if (SlotCount < mask.SlotCount)
                SlotCount = mask.SlotCount;

            for (int i = 0; i < mask.SlotCount; i++)
            {
                ulong maskSlotBits = mask.bits[i];
                ulong slotBits = bits[i];
                bits[i] = (maskSlotBits ^ (maskSlotBits & ~slotBits)) | (slotBits & ~maskSlotBits);
            }
        }

        public bool Test(int index)
        {
            int slotIndex = index / BITS_PER_SLOT;

            if (SlotCount <= slotIndex)
                return false;

            int bitIndex = index % BITS_PER_SLOT;
            ulong slotBits = bits[slotIndex];
            ulong bitMask = MASK[bitIndex];

            return (slotBits & bitMask) != 0;
        }

        public void Union(BitSet other)
        {
            if (SlotCount < other.SlotCount)
                SlotCount = other.SlotCount;

            int count = System.Math.Min(SlotCount, other.SlotCount);
            for (int i = 0; i < count; i++)
            {
                ulong otherSlotBits = other.bits[i];
                bits[i] |= otherSlotBits;
            }
        }

        public void Union(BitSet other, BitSet result)
        {
            result.Clear();
            result.SlotCount = System.Math.Max(SlotCount, other.SlotCount);

            if (SlotCount >= other.SlotCount)
            {
                for (int i = 0; i < other.SlotCount; i++)
                {
                    ulong slotBits = bits[i];
                    ulong otherSlotBits = other.bits[i];
                    result.bits[i] = slotBits | otherSlotBits;
                }

                for (int i = other.SlotCount + 1; i < SlotCount; i++)
                {
                    ulong slotBits = bits[i];
                    result.bits[i] = slotBits;
                }
            }
            else
            {
                for (int i = 0; i < SlotCount; i++)
                {
                    ulong slotBits = bits[i];
                    ulong otherSlotBits = other.bits[i];
                    result.bits[i] = slotBits | otherSlotBits;
                }

                for (int i = SlotCount + 1; i < other.SlotCount; i++)
                {
                    ulong otherSlotBits = other.bits[i];
                    result.bits[i] = otherSlotBits;
                }
            }    
        }

        public void Intersection(BitSet other)
        {
            if (SlotCount < other.SlotCount)
                SlotCount = other.SlotCount;

            int count = System.Math.Min(SlotCount, other.SlotCount);
            for (int i = 0; i < count; i++)
            {
                ulong otherSlotBits = other.bits[i];
                bits[i] &= otherSlotBits;
            }
        }

        public void Intersection(BitSet other, BitSet result)
        {
            result.Clear();
            result.SlotCount = System.Math.Min(SlotCount, other.SlotCount);

            for (int i = 0; i < result.SlotCount; i++)
            {
                ulong slotBits = bits[i];
                ulong otherSlotBits = other.bits[i];
                result.bits[i] = slotBits & otherSlotBits;
            }
        }

        public void Complementary()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                ulong slotBits = bits[i];
                bits[i] = ~slotBits;
            }
        }

        public void Complementary(BitSet result)
        {
            result.Clear();
            result.SlotCount = SlotCount;

            for (int i = 0; i < SlotCount; i++)
            {
                ulong slotBits = bits[i];
                result.bits[i] = ~slotBits;
            }
        }

        public void Difference(BitSet other)
        {
            if (SlotCount < other.SlotCount)
                SlotCount = other.SlotCount;

            int count = System.Math.Min(SlotCount, other.SlotCount);
            for (int i = 0; i < count; i++)
            {
                ulong otherSlotBits = other.bits[i];
                bits[i] &= ~otherSlotBits;
            }
        }

        public void Difference(BitSet other, BitSet result)
        {
            result.Clear();
            result.SlotCount = System.Math.Min(SlotCount, other.SlotCount);

            for (int i = 0; i < result.SlotCount; i++)
            {
                ulong slotBits = bits[i];
                ulong otherSlotBits = other.bits[i];
                result.bits[i] = slotBits & ~otherSlotBits;
            }
        }

        public void Xor(BitSet other)
        {
            if (SlotCount < other.SlotCount)
                SlotCount = other.SlotCount;

            int count = System.Math.Min(SlotCount, other.SlotCount);
            for (int i = 0; i < count; i++)
            {
                ulong otherSlotBits = other.bits[i];
                bits[i] ^= otherSlotBits;
            }
        }

        public void Xor(BitSet other, BitSet result)
        {
            result.Clear();
            result.SlotCount = System.Math.Min(SlotCount, other.SlotCount);

            for (int i = 0; i < result.SlotCount; i++)
            {
                ulong slotBits = bits[i];
                ulong otherSlotBits = other.bits[i];
                result.bits[i] = slotBits ^ otherSlotBits;
            }
        }

        public void Split(BitSet other, BitSet myDiff, BitSet intersection, BitSet otherDiff)
        {
            myDiff.Clear();
            intersection.Clear();
            otherDiff.Clear();

            int count = System.Math.Max(SlotCount, other.SlotCount);

            myDiff.SlotCount = count;
            intersection.SlotCount = count;
            otherDiff.SlotCount = count;

            for (int i = 0; i < count; i++)
            {
                ulong slotBits = i < SlotCount ? bits[i] : 0;
                ulong otherSlotBits = i < other.SlotCount ? other.bits[i] : 0;

                myDiff.bits[i] = slotBits & ~otherSlotBits;
                intersection.bits[i] = slotBits & otherSlotBits;
                otherDiff.bits[i] = ~slotBits & otherSlotBits;
            }
        }

        public int FirstSetBit(int start = 0)
        {
            int startSlotIndex = start / BITS_PER_SLOT;

            if (SlotCount <= startSlotIndex)
                return -1;

            int startBitIndex = start % BITS_PER_SLOT;
            ulong bitMask = ~(MASK[startBitIndex] - 1);

            for (int i = startSlotIndex; i < bits.Count; i++)
            {
                ulong slotBits = bits[i] & bitMask;
                bitMask = ~0UL;

                if (slotBits == 0)
                    continue;

                int bitIndex = BitScanForward(slotBits);
                return i * BITS_PER_SLOT + bitIndex;
            }

            return -1;
        }

        public void Clear()
        {
            for (int i = 0; i < bits.Count; i++)
                bits[i] = 0;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            bool first = true;
            foreach (var slotBits in bits)
            {
                if (first)
                    first = false;
                else
                    builder.Append(" ");

                builder.Append(ULongToBinaryString(slotBits));
            }

            return builder.ToString();
        }
    }
}