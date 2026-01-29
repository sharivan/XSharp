using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using XSharp.Serialization;

namespace XSharp.Util;

public class BitSet : ISet<int>, IReadOnlySet<int>, ISerializable
{
    public const int BITS_PER_SLOT = 8 * sizeof(ulong);

    private static readonly ulong[] MASK =
    [
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
    ];

    private static readonly ulong[] START_MASK =
    [
        0xffffffffffffffffUL << 0,
        0xffffffffffffffffUL << 1,
        0xffffffffffffffffUL << 2,
        0xffffffffffffffffUL << 3,
        0xffffffffffffffffUL << 4,
        0xffffffffffffffffUL << 5,
        0xffffffffffffffffUL << 6,
        0xffffffffffffffffUL << 7,
        0xffffffffffffffffUL << 8,
        0xffffffffffffffffUL << 9,
        0xffffffffffffffffUL << 10,
        0xffffffffffffffffUL << 11,
        0xffffffffffffffffUL << 12,
        0xffffffffffffffffUL << 13,
        0xffffffffffffffffUL << 14,
        0xffffffffffffffffUL << 15,
        0xffffffffffffffffUL << 16,
        0xffffffffffffffffUL << 17,
        0xffffffffffffffffUL << 18,
        0xffffffffffffffffUL << 19,
        0xffffffffffffffffUL << 20,
        0xffffffffffffffffUL << 21,
        0xffffffffffffffffUL << 22,
        0xffffffffffffffffUL << 23,
        0xffffffffffffffffUL << 24,
        0xffffffffffffffffUL << 25,
        0xffffffffffffffffUL << 26,
        0xffffffffffffffffUL << 27,
        0xffffffffffffffffUL << 28,
        0xffffffffffffffffUL << 29,
        0xffffffffffffffffUL << 30,
        0xffffffffffffffffUL << 31,
        0xffffffffffffffffUL << 32,
        0xffffffffffffffffUL << 33,
        0xffffffffffffffffUL << 34,
        0xffffffffffffffffUL << 35,
        0xffffffffffffffffUL << 36,
        0xffffffffffffffffUL << 37,
        0xffffffffffffffffUL << 38,
        0xffffffffffffffffUL << 39,
        0xffffffffffffffffUL << 40,
        0xffffffffffffffffUL << 41,
        0xffffffffffffffffUL << 42,
        0xffffffffffffffffUL << 43,
        0xffffffffffffffffUL << 44,
        0xffffffffffffffffUL << 45,
        0xffffffffffffffffUL << 46,
        0xffffffffffffffffUL << 47,
        0xffffffffffffffffUL << 48,
        0xffffffffffffffffUL << 49,
        0xffffffffffffffffUL << 50,
        0xffffffffffffffffUL << 51,
        0xffffffffffffffffUL << 52,
        0xffffffffffffffffUL << 53,
        0xffffffffffffffffUL << 54,
        0xffffffffffffffffUL << 55,
        0xffffffffffffffffUL << 56,
        0xffffffffffffffffUL << 57,
        0xffffffffffffffffUL << 58,
        0xffffffffffffffffUL << 59,
        0xffffffffffffffffUL << 60,
        0xffffffffffffffffUL << 61,
        0xffffffffffffffffUL << 62,
        0xffffffffffffffffUL << 63
    ];

    private static readonly ulong[] END_MASK =
    [
        0xffffffffffffffffUL >> 63,
        0xffffffffffffffffUL >> 62,
        0xffffffffffffffffUL >> 61,
        0xffffffffffffffffUL >> 60,
        0xffffffffffffffffUL >> 59,
        0xffffffffffffffffUL >> 58,
        0xffffffffffffffffUL >> 57,
        0xffffffffffffffffUL >> 56,
        0xffffffffffffffffUL >> 55,
        0xffffffffffffffffUL >> 54,
        0xffffffffffffffffUL >> 53,
        0xffffffffffffffffUL >> 52,
        0xffffffffffffffffUL >> 51,
        0xffffffffffffffffUL >> 50,
        0xffffffffffffffffUL >> 49,
        0xffffffffffffffffUL >> 48,
        0xffffffffffffffffUL >> 47,
        0xffffffffffffffffUL >> 46,
        0xffffffffffffffffUL >> 45,
        0xffffffffffffffffUL >> 44,
        0xffffffffffffffffUL >> 43,
        0xffffffffffffffffUL >> 42,
        0xffffffffffffffffUL >> 41,
        0xffffffffffffffffUL >> 40,
        0xffffffffffffffffUL >> 39,
        0xffffffffffffffffUL >> 38,
        0xffffffffffffffffUL >> 37,
        0xffffffffffffffffUL >> 36,
        0xffffffffffffffffUL >> 35,
        0xffffffffffffffffUL >> 34,
        0xffffffffffffffffUL >> 33,
        0xffffffffffffffffUL >> 32,
        0xffffffffffffffffUL >> 31,
        0xffffffffffffffffUL >> 30,
        0xffffffffffffffffUL >> 29,
        0xffffffffffffffffUL >> 28,
        0xffffffffffffffffUL >> 27,
        0xffffffffffffffffUL >> 26,
        0xffffffffffffffffUL >> 25,
        0xffffffffffffffffUL >> 24,
        0xffffffffffffffffUL >> 23,
        0xffffffffffffffffUL >> 22,
        0xffffffffffffffffUL >> 21,
        0xffffffffffffffffUL >> 20,
        0xffffffffffffffffUL >> 19,
        0xffffffffffffffffUL >> 18,
        0xffffffffffffffffUL >> 17,
        0xffffffffffffffffUL >> 16,
        0xffffffffffffffffUL >> 15,
        0xffffffffffffffffUL >> 14,
        0xffffffffffffffffUL >> 13,
        0xffffffffffffffffUL >> 12,
        0xffffffffffffffffUL >> 11,
        0xffffffffffffffffUL >> 10,
        0xffffffffffffffffUL >> 9,
        0xffffffffffffffffUL >> 8,
        0xffffffffffffffffUL >> 7,
        0xffffffffffffffffUL >> 6,
        0xffffffffffffffffUL >> 5,
        0xffffffffffffffffUL >> 4,
        0xffffffffffffffffUL >> 3,
        0xffffffffffffffffUL >> 2,
        0xffffffffffffffffUL >> 1,
        0xffffffffffffffffUL >> 0
    ];

    // Types and constants used in the functions below
    private const ulong M1 = 0x5555555555555555;  // Binary: 0101...
    private const ulong M2 = 0x3333333333333333;  // Binary: 00110011..
    private const ulong M4 = 0x0f0f0f0f0f0f0f0f;  // Binary:  4 zeros,  4 ones ...
    private const ulong H01 = 0x0101010101010101; // The sum of 256 to the power of 0,1,2,3...

    private const ulong DeBruijnSequence = 0x37E84A99DAE458F;

    private static readonly int[] MultiplyDeBruijnBitPosition =
    [
        0, 1, 17, 2, 18, 50, 3, 57,
        47, 19, 22, 51, 29, 4, 33, 58,
        15, 48, 20, 27, 25, 23, 52, 41,
        54, 30, 38, 5, 43, 34, 59, 8,
        63, 16, 49, 56, 46, 21, 28, 32,
        14, 26, 24, 40, 53, 37, 42, 7,
        62, 55, 45, 31, 13, 39, 36, 6,
        61, 44, 12, 35, 60, 11, 10, 9,
    ];

    // This uses fewer arithmetic operations than any other known implementation on machines with fast multiplication.
    // This algorithm uses 12 arithmetic operations, one of which is a multiply.
    private static int PopCount64(ulong x)
    {
        x -= x >> 1 & M1;             // Put count of each 2 bits into those 2 bits
        x = (x & M2) + (x >> 2 & M2); // Put count of each 4 bits into those 4 bits 
        x = x + (x >> 4) & M4;        // Put count of each 8 bits into those 8 bits 
        return (int) (x * H01 >> 56); // Returns left 8 bits of x + (x<<8) + (x<<16) + (x<<24) + ... 
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
        return MultiplyDeBruijnBitPosition[(ulong) ((long) b & -(long) b) * DeBruijnSequence >> 58];
    }

    private static string ULongToReversedBinary(ulong value)
    {
        char[] buffer = new char[BITS_PER_SLOT];

        for (int i = 0; i < BITS_PER_SLOT; i++)
        {
            buffer[i] = (char) ('0' + (value & 1));
            value >>= 1;
        }

        return new string(buffer, 0, BITS_PER_SLOT);
    }

    private class BitSetEnumerator : IEnumerator<int>
    {
        private BitSet set;

        object IEnumerator.Current => Current >= 0 ? Current : null;

        public int Current
        {
            get;
            private set;
        } = -1;

        internal BitSetEnumerator(BitSet set)
        {
            this.set = set;
        }

        public void Dispose()
        {
            set = null;
            Current = -1;
            GC.SuppressFinalize(this);
        }

        public bool MoveNext()
        {
            Current = set.FirstSetBit(Current + 1);
            return Current >= 0;
        }

        public void Reset()
        {
            Current = -1;
        }
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
            {
                bits.RemoveRange(value, currentSlotCount - value);
            }
        }
    }

    public int BitCount => SlotCount * BITS_PER_SLOT;

    public bool IsReadOnly => false;

    public bool this[int index] => Test(index);

    public BitSet(int initialSlotCount = 1)
    {
        if (initialSlotCount < 0)
            throw new ArgumentException($"Invalid negative initial slot count value '{initialSlotCount}'.");

        bits = [];
        SlotCount = initialSlotCount;
    }

    public BitSet(BitSet other)
    {
        bits = [];

        if (other != null)
            UnionWith(other);
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
        if (index < 0)
            throw new IndexOutOfRangeException("Invalid negative index '{index}'.");

        int slotIndex = index / BITS_PER_SLOT;
        int bitIndex = index % BITS_PER_SLOT;
        ulong slotBits = GetSlot(slotIndex);
        ulong bitMask = MASK[bitIndex];

        SetSlotIncluding(slotIndex, bitMask);
        return (slotBits & bitMask) != 0;
    }

    public void SetRange(int start)
    {
        SetRange(start, BitCount - start);
    }

    public void SetRange(int start, int count)
    {
        int end = start + count;

        if (start < 0 || end <= 0 || start >= end)
            throw new ArgumentException("Invalid bit range");

        int startSlot = start / BITS_PER_SLOT;
        int endSlot = (end - 1) / BITS_PER_SLOT + 1;

        if (endSlot > SlotCount)
            SlotCount = endSlot;

        const ulong MASK = ~0UL;

        if (endSlot == startSlot + 1)
        {
            bits[startSlot] |= (MASK << (start % BITS_PER_SLOT)) & (MASK >> (BITS_PER_SLOT - (end - 1) % BITS_PER_SLOT - 1));
            return;
        }

        bits[startSlot] |= MASK << (start % BITS_PER_SLOT);
        bits[endSlot - 1] |= MASK >> (BITS_PER_SLOT - (end - 1) % BITS_PER_SLOT - 1);

        for (int i = startSlot + 1; i < endSlot - 1; i++)
            bits[i] |= MASK;
    }

    public bool Reset(int index)
    {
        if (index < 0)
            throw new IndexOutOfRangeException("Invalid negative index '{index}'.");

        int slotIndex = index / BITS_PER_SLOT;
        int bitIndex = index % BITS_PER_SLOT;
        ulong slotBits = GetSlot(slotIndex);
        ulong bitMask = MASK[bitIndex];

        SetSlotExcluding(slotIndex, bitMask);
        return (slotBits & bitMask) != 0;
    }

    public void ResetRange(int start)
    {
        ResetRange(start, BitCount - start);
    }

    public void ResetRange(int start, int count)
    {
        int end = start + count;

        if (start < 0 || end <= 0 || start >= end)
            throw new ArgumentException("Invalid bit range");

        int startSlot = start / BITS_PER_SLOT;
        int endSlot = (end - 1) / BITS_PER_SLOT + 1;

        if (endSlot > SlotCount)
            SlotCount = endSlot;

        const ulong MASK = ~0UL;

        if (endSlot == startSlot + 1)
        {
            bits[startSlot] |= ~(MASK << (start % BITS_PER_SLOT)) & ~(MASK >> (BITS_PER_SLOT - (end - 1) % BITS_PER_SLOT - 1));
            return;
        }

        bits[startSlot] &= ~(MASK << (start % BITS_PER_SLOT));
        bits[endSlot - 1] &= ~(MASK >> (BITS_PER_SLOT - (end - 1) % BITS_PER_SLOT - 1));

        for (int i = startSlot + 1; i < endSlot - 1; i++)
            bits[i] = 0;
    }

    public bool Toggle(int index)
    {
        if (index < 0)
            throw new IndexOutOfRangeException("Invalid negative index '{index}'.");

        bool test = Test(index);
        if (test)
            Reset(index);
        else
            Set(index);

        return test;
    }

    public void ToggleRange(int start)
    {
        ToggleRange(start, BitCount - start);
    }

    public void ToggleRange(int start, int count)
    {
        int end = start + count;

        if (start < 0 || end <= 0 || start >= end)
            throw new ArgumentException("Invalid bit range");

        int startSlot = start / BITS_PER_SLOT;
        int endSlot = (end - 1) / BITS_PER_SLOT + 1;

        if (endSlot > SlotCount)
            SlotCount = endSlot;

        const ulong MASK = ~0UL;
        ulong slotBits;
        ulong START_MASK;

        if (endSlot == startSlot + 1)
        {
            slotBits = bits[startSlot];
            START_MASK = (MASK << (start % BITS_PER_SLOT)) & (MASK >> (BITS_PER_SLOT - (end - 1) % BITS_PER_SLOT - 1));
            bits[startSlot] = ~(START_MASK ^ (START_MASK & ~slotBits)) | (slotBits & ~START_MASK);
            return;
        }

        slotBits = bits[startSlot];
        START_MASK = MASK << (start % BITS_PER_SLOT);
        bits[startSlot] = ~(START_MASK ^ (START_MASK & ~slotBits)) | (slotBits & ~START_MASK);

        slotBits = bits[endSlot - 1];
        ulong END_MASK = MASK >> (BITS_PER_SLOT - (end - 1) % BITS_PER_SLOT - 1);
        bits[endSlot - 1] = ~(END_MASK ^ (END_MASK & ~slotBits)) | (slotBits & ~END_MASK);

        for (int i = startSlot + 1; i < endSlot - 1; i++)
        {
            slotBits = bits[i];
            bits[i] = (MASK ^ (MASK & ~slotBits)) | (slotBits & ~MASK);
        }
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
        if (index < 0)
            return false;

        int slotIndex = index / BITS_PER_SLOT;

        if (SlotCount <= slotIndex)
            return false;

        int bitIndex = index % BITS_PER_SLOT;
        ulong slotBits = bits[slotIndex];
        ulong bitMask = MASK[bitIndex];

        return (slotBits & bitMask) != 0;
    }

    public bool TestRange(int start)
    {
        return TestRange(start, BitCount - start);
    }

    public bool TestRange(int start, int count)
    {
        int end = start + count;

        if (start < 0 || end <= 0 || start >= end)
            return false;

        int startSlot = start / BITS_PER_SLOT;
        int endSlot = (end - 1) / BITS_PER_SLOT + 1;

        if (endSlot > SlotCount)
            return false;

        const ulong MASK = ~0UL;
        ulong START_MASK;
        ulong slotBits;

        if (endSlot == startSlot + 1)
        {
            START_MASK = (MASK << (start % BITS_PER_SLOT)) & (MASK >> (BITS_PER_SLOT - (end - 1) % BITS_PER_SLOT - 1));
            slotBits = bits[startSlot];
            if ((slotBits & START_MASK) != START_MASK)
                return false;

            return true;
        }

        START_MASK = MASK << (start % BITS_PER_SLOT);
        slotBits = bits[startSlot];
        if ((slotBits & START_MASK) != START_MASK)
            return false;

        ulong END_MASK = MASK >> (BITS_PER_SLOT - (end - 1) % BITS_PER_SLOT - 1);
        slotBits = bits[endSlot - 1];
        if ((slotBits & END_MASK) != END_MASK)
            return false;

        for (int i = startSlot + 1; i < endSlot - 1; i++)
        {
            slotBits = bits[i];
            if (slotBits == 0)
                return false;
        }

        return true;
    }

    public int PopCount(int start = 0)
    {
        return PopCount(start, BitCount - start);
    }

    public int PopCount(int start, int count)
    {
        int end = start + count;

        if (start < 0)
            start = 0;

        if (end <= 0 || start >= end)
            return 0;

        int startSlot = start / BITS_PER_SLOT;
        int endSlot = (end - 1) / BITS_PER_SLOT + 1;

        if (endSlot > SlotCount)
            endSlot = SlotCount;

        const ulong MASK = ~0UL;
        ulong START_MASK;
        ulong slotBits;

        if (endSlot == startSlot + 1)
        {
            START_MASK = (MASK << (start % BITS_PER_SLOT)) & (MASK >> (BITS_PER_SLOT - (end - 1) % BITS_PER_SLOT - 1));
            slotBits = bits[startSlot];
            return PopCount64(slotBits & START_MASK);
        }

        START_MASK = MASK << (start % BITS_PER_SLOT);
        slotBits = bits[startSlot];
        int result = PopCount64(slotBits & START_MASK);

        ulong END_MASK = MASK >> (BITS_PER_SLOT - (end - 1) % BITS_PER_SLOT - 1);
        slotBits = bits[endSlot - 1];
        result += PopCount64(slotBits & END_MASK);

        for (int i = startSlot + 1; i < endSlot - 1; i++)
        {
            slotBits = bits[i];
            result += PopCount64(slotBits);
        }

        return result;
    }

    public int FirstSetBit(int start = 0)
    {
        return FirstSetBit(start, BitCount - start);
    }

    public int FirstSetBit(int start, int count)
    {
        int end = start + count;
        if (end <= start)
            return -1;

        int startSlot = start / BITS_PER_SLOT;
        int endSlot = (end - 1) / BITS_PER_SLOT + 1;

        ulong startBitmask = START_MASK[start % BITS_PER_SLOT];
        ulong endBitmask = END_MASK[(end - 1) % BITS_PER_SLOT];

        for (int i = startSlot; i < endSlot; i++)
        {
            ulong slotBits = bits[i];

            if (i == startSlot)
                slotBits &= startBitmask;

            if (i == endSlot - 1)
                slotBits &= endBitmask;

            if (slotBits == 0)
                continue;

            int bitIndex = BitScanForward(slotBits);
            return i * BITS_PER_SLOT + bitIndex;
        }

        return -1;
    }

    public void Clear()
    {
        for (int i = 0; i < SlotCount; i++)
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
                builder.Append(' ');

            builder.Append(ULongToReversedBinary(slotBits));
        }

        return builder.ToString();
    }

    public void Deserialize(ISerializer reader)
    {
        bits ??= [];

        SlotCount = reader.ReadInt();
        for (int i = 0; i < SlotCount; i++)
            bits[i] = reader.ReadULong();
    }

    public void Serialize(ISerializer writer)
    {
        writer.WriteInt(SlotCount);
        foreach (var slotBits in bits)
            writer.WriteULong(slotBits);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(bits);
    }

    public bool Add(int item)
    {
        return Set(item);
    }

    public void ExceptWith(BitSet mask)
    {
        if (SlotCount < mask.SlotCount)
            SlotCount = mask.SlotCount;

        int count = System.Math.Min(SlotCount, mask.SlotCount);
        for (int i = 0; i < count; i++)
        {
            ulong maskSlotBits = mask.bits[i];
            bits[i] &= ~maskSlotBits;
        }
    }

    public void ExceptWith(BitSet other, BitSet result)
    {
        result.SlotCount = System.Math.Min(SlotCount, other.SlotCount);

        for (int i = 0; i < result.SlotCount; i++)
        {
            ulong slotBits = bits[i];
            ulong otherSlotBits = other.bits[i];
            result.bits[i] = slotBits & ~otherSlotBits;
        }
    }

    public void ExceptWith(IEnumerable<int> other)
    {
        if (other is BitSet set)
        {
            ExceptWith(set);
        }
        else
        {
            foreach (var index in other)
                Reset(index);
        }
    }

    public void IntersectWith(BitSet other)
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

    public void IntersectWith(BitSet other, BitSet result)
    {
        result.SlotCount = System.Math.Min(SlotCount, other.SlotCount);

        for (int i = 0; i < result.SlotCount; i++)
        {
            ulong slotBits = bits[i];
            ulong otherSlotBits = other.bits[i];
            result.bits[i] = slotBits & otherSlotBits;
        }
    }

    public void IntersectWith(IEnumerable<int> other)
    {
        if (other is BitSet set)
        {
            IntersectWith(set);
        }
        else
        {
            foreach (var index in this)
            {
                if (!other.Contains(index))
                    Reset(index);
            }
        }
    }

    public bool IsProperSubsetOf(BitSet other)
    {
        bool propper = false;
        for (int i = 0; i < SlotCount; i++)
        {
            ulong slotBits = bits[i];
            ulong otherSlotBits = i < other.SlotCount ? other.bits[i] : 0;

            if ((slotBits & otherSlotBits) != slotBits)
                return false;

            if (slotBits != otherSlotBits)
                propper = true;
        }

        return propper;
    }

    public bool IsProperSubsetOf(IEnumerable<int> other)
    {
        if (other is BitSet set)
            return IsProperSubsetOf(set);

        int count = 0;
        foreach (var index in this)
        {
            if (!other.Contains(index))
                return false;

            count++;
        }

        return count < other.Count();
    }

    public bool IsProperSupersetOf(BitSet other)
    {
        bool propper = false;
        for (int i = 0; i < SlotCount; i++)
        {
            ulong slotBits = bits[i];
            ulong otherSlotBits = i < other.SlotCount ? other.bits[i] : 0;

            if ((slotBits & otherSlotBits) != otherSlotBits)
                return false;

            if (slotBits != otherSlotBits)
                propper = true;
        }

        return propper;
    }

    public bool IsProperSupersetOf(IEnumerable<int> other)
    {
        if (other is BitSet set)
            return IsProperSupersetOf(set);

        int count = 0;
        foreach (var index in this)
        {
            if (!other.Contains(index))
                return false;

            count++;
        }

        return count > other.Count();
    }

    public bool IsSubsetOf(BitSet other)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            ulong slotBits = bits[i];
            ulong otherSlotBits = i < other.SlotCount ? other.bits[i] : 0;
            if ((slotBits & otherSlotBits) != slotBits)
                return false;
        }

        return true;
    }

    public bool IsSubsetOf(IEnumerable<int> other)
    {
        if (other is BitSet set)
            return IsSubsetOf(set);

        foreach (var index in this)
        {
            if (!other.Contains(index))
                return false;
        }

        return true;
    }

    public bool IsSupersetOf(BitSet other)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            ulong slotBits = bits[i];
            ulong otherSlotBits = i < other.SlotCount ? other.bits[i] : 0;
            if ((slotBits & otherSlotBits) != otherSlotBits)
                return false;
        }

        return true;
    }

    public bool IsSupersetOf(IEnumerable<int> other)
    {
        if (other is BitSet set)
            return IsSupersetOf(set);

        foreach (var index in other)
        {
            if (!Test(index))
                return false;
        }

        return true;
    }

    public bool Overlaps(BitSet other)
    {
        int count = System.Math.Min(SlotCount, other.SlotCount);
        for (int i = 0; i < count; i++)
        {
            ulong slotBits = bits[i];
            ulong otherSlotBits = other.bits[i];
            if ((slotBits & otherSlotBits) != 0)
                return true;
        }

        return false;
    }

    public bool Overlaps(IEnumerable<int> other)
    {
        if (other is BitSet set)
            return Overlaps(set);

        foreach (var index in other)
        {
            if (Test(index))
                return true;
        }

        return false;
    }

    public bool Equals(BitSet other)
    {
        int count = System.Math.Max(SlotCount, other.SlotCount);
        for (int i = 0; i < count; i++)
        {
            ulong slotBits = i < SlotCount ? bits[i] : 0;
            ulong otherSlotBits = i < other.SlotCount ? other.bits[i] : 0;
            if (slotBits != otherSlotBits)
                return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is BitSet set && Equals(bits, set.bits);
    }

    public bool SetEquals(IEnumerable<int> other)
    {
        if (other is BitSet set)
            return Equals(set);

        if (Count != other.Count())
            return false;

        foreach (var index in other)
        {
            if (!Test(index))
                return false;
        }

        return true;
    }

    public void SymmetricExceptWith(BitSet other)
    {
        if (SlotCount > other.SlotCount)
            SlotCount = other.SlotCount;

        for (int i = 0; i < SlotCount; i++)
        {
            ulong slotBits = bits[i];
            ulong otherSlotBits = other.bits[i];
            bits[i] = (slotBits & ~otherSlotBits) | (~slotBits & otherSlotBits);
        }
    }

    public void SymmetricExceptWith(BitSet other, BitSet result)
    {
        result.SlotCount = System.Math.Max(SlotCount, other.SlotCount);

        for (int i = 0; i < result.SlotCount; i++)
        {
            ulong slotBits = bits[i];
            ulong otherSlotBits = other.bits[i];
            result.bits[i] = (slotBits & ~otherSlotBits) | (~slotBits & otherSlotBits);
        }
    }

    public void SymmetricExceptWith(IEnumerable<int> other)
    {
        if (other is BitSet set)
        {
            SymmetricExceptWith(set);
        }
        else
        {
            foreach (var index in this)
            {
                foreach (var otherIndex in other)
                {
                    if (index == otherIndex)
                        Reset(index);
                }
            }
        }
    }

    public void UnionWith(BitSet mask)
    {
        if (SlotCount < mask.SlotCount)
            SlotCount = mask.SlotCount;

        for (int i = 0; i < mask.SlotCount; i++)
        {
            ulong maskSlotBits = mask.bits[i];
            bits[i] |= maskSlotBits;
        }
    }

    public void UnionWith(BitSet other, BitSet result)
    {
        result.SlotCount = System.Math.Max(SlotCount, other.SlotCount);

        for (int i = 0; i < result.SlotCount; i++)
        {
            ulong slotBits = i < SlotCount ? bits[i] : 0;
            ulong otherSlotBits = i < other.SlotCount ? other.bits[i] : 0;
            result.bits[i] = slotBits | otherSlotBits;
        }
    }

    public void UnionWith(IEnumerable<int> other)
    {
        if (other is BitSet set)
        {
            UnionWith(set);
        }
        else
        {
            foreach (var index in other)
                Set(index);
        }
    }

    public void Complementary()
    {
        Complementary(0, BitCount);
    }

    public void Complementary(int count)
    {
        Complementary(0, count);
    }

    public void Complementary(int start, int count)
    {
        int end = start + count;

        if (start < 0 || end <= 0 || start >= end)
            throw new ArgumentException("Invalid bit range");

        int startSlot = start / BITS_PER_SLOT;
        int endSlot = (end - 1) / BITS_PER_SLOT + 1;
        int slotDiff = endSlot - SlotCount;

        const ulong MASK = ~0UL;
        ulong slotBits;
        ulong startMask;

        if (slotDiff > 0)
        {
            for (int i = 0; i < slotDiff; i++)
                bits.Add(MASK);
        }

        if (endSlot == startSlot + 1)
        {
            slotBits = bits[startSlot];
            startMask = (MASK << (start % BITS_PER_SLOT)) & (MASK >> (BITS_PER_SLOT - (end - 1) % BITS_PER_SLOT - 1));
            bits[startSlot] = startMask ^ slotBits;
            return;
        }

        slotBits = bits[startSlot];
        startMask = MASK << (start % BITS_PER_SLOT);
        bits[startSlot] = startMask ^ slotBits;

        slotBits = bits[endSlot - 1];
        ulong endMask = MASK >> (BITS_PER_SLOT - (end - 1) % BITS_PER_SLOT - 1);
        bits[endSlot - 1] = endMask ^ slotBits;

        for (int i = startSlot + 1; i < endSlot - 1; i++)
        {
            slotBits = bits[i];
            bits[i] = MASK ^ slotBits;
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

    public void Split(BitSet other, BitSet myDiff, BitSet intersection, BitSet otherDiff)
    {
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

    void ICollection<int>.Add(int item)
    {
        Set(item);
    }

    public bool Contains(int item)
    {
        return Test(item);
    }

    public void CopyTo(int[] array, int arrayIndex)
    {
        int i = arrayIndex;
        foreach (int index in this)
        {
            if (i >= array.Length)
                break;

            array[i++] = index;
        }
    }

    public bool Remove(int item)
    {
        return Reset(item);
    }

    public void ShiftLeft(int bitsToShift)
    {
        ShiftRight(0, BitCount, bitsToShift);
    }

    public void ShiftLeft(int start, int bitsToShift)
    {
        ShiftRight(start, BitCount, bitsToShift);
    }

    // TODO : Fix me!
    public void ShiftLeft(int start, int end, int bitsToShift)
    {
        if (start < 0 || end <= 0 || start >= end)
            throw new ArgumentException("Invalid bit range");

        if (bitsToShift >= end - start)
        {
            ResetRange(start, end - start);
            return;
        }

        int startSlot = start / BITS_PER_SLOT;
        int endSlot = (end - 1) / BITS_PER_SLOT + 1;
        int slotsToShift = endSlot - startSlot;

        if (endSlot > SlotCount)
            SlotCount = endSlot;

        for (int i = endSlot - 1; i >= startSlot; i--)
        {
            ulong shifted = bits[i - slotsToShift] << bitsToShift;

            if (bitsToShift > 0 && i > startSlot)
            {
                ulong carry = bits[i - 1] >> (BITS_PER_SLOT - bitsToShift);
                shifted |= carry;
            }

            bits[i] = shifted;
        }

        ulong mask = ((1UL << (end - start + 1)) - 1UL) << start % BITS_PER_SLOT;
        for (int i = startSlot; i < endSlot; i++)
            bits[i] = (bits[i] & ~mask) | ((bits[i] & mask) << bitsToShift);
    }

    public void ShiftRight(int bitsToShift)
    {
        ShiftRight(0, BitCount, bitsToShift);
    }

    public void ShiftRight(int start, int bitsToShift)
    {
        ShiftRight(start, BitCount, bitsToShift);
    }

    // TODO : Fix me!
    public void ShiftRight(int start, int end, int bitsToShift)
    {
        if (start < 0 || end <= 0 || start >= end)
            throw new ArgumentException("Invalid bit range");

        if (bitsToShift >= end - start)
        {
            ResetRange(start, end - start);
            return;
        }

        int startSlot = start / BITS_PER_SLOT;
        int endSlot = (end - 1) / BITS_PER_SLOT + 1;
        int slotsToShift = endSlot - startSlot;

        if (endSlot > SlotCount)
            SlotCount = endSlot;

        ulong shifted = 0;

        for (int i = endSlot - 1; i >= startSlot; i--)
        {
            ulong carry = i == startSlot ? bits[i] << (BITS_PER_SLOT - bitsToShift) : bits[i] << (BITS_PER_SLOT - bitsToShift) | shifted >> bitsToShift;
            shifted = bits[i] >> bitsToShift;
            bits[i] = carry;
        }

        int mask = (1 << bitsToShift) - 1;
        ulong maskBits = (ulong) mask << (BITS_PER_SLOT - bitsToShift);

        for (int i = endSlot - slotsToShift; i < endSlot; i++)
        {
            bits[i] &= maskBits;
            bits[i] |= shifted << (BITS_PER_SLOT - bitsToShift);
            shifted >>= bitsToShift;
        }
    }

    public void RotateLeft(int start, int bitsToRotate)
    {
        RotateLeft(start, BitCount, bitsToRotate);
    }

    // TODO : Implement me!
    public void RotateLeft(int start, int end, int bitsToRotate)
    {
    }

    public void RotateRight(int start, int bitsToRotate)
    {
        RotateRight(start, BitCount, bitsToRotate);
    }

    // TODO : Implement me!
    public void RotateRight(int start, int end, int bitsToRotate)
    {
    }

    public IEnumerator<int> GetEnumerator()
    {
        return new BitSetEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static bool operator ==(BitSet left, BitSet right)
    {
        return ReferenceEquals(left, right) || left is null ? right is null : left.Equals(right);
    }

    public static bool operator !=(BitSet left, BitSet right)
    {
        return !(left == right);
    }

    public static bool operator <=(BitSet left, BitSet right)
    {
        return ReferenceEquals(left, right) || left is null ? right is null : right.IsSubsetOf(left);
    }

    public static bool operator <(BitSet left, BitSet right)
    {
        return !ReferenceEquals(left, right) && left is null && right is null && right.IsProperSubsetOf(left);
    }

    public static bool operator >=(BitSet left, BitSet right)
    {
        return !(left < right);
    }

    public static bool operator >(BitSet left, BitSet right)
    {
        return !(left <= right);
    }

    public static BitSet operator &(BitSet left, BitSet right)
    {
        var result = new BitSet();
        left.IntersectWith(right, result);
        return result;
    }

    public static BitSet operator |(BitSet left, BitSet right)
    {
        var result = new BitSet();
        left.UnionWith(right, result);
        return result;
    }

    public static BitSet operator ^(BitSet left, BitSet right)
    {
        var result = new BitSet();
        left.SymmetricExceptWith(right, result);
        return result;
    }

    public static BitSet operator ~(BitSet set)
    {
        var result = new BitSet();
        set.Complementary(result);
        return result;
    }

    public static BitSet operator -(BitSet left, BitSet right)
    {
        var result = new BitSet();
        left.ExceptWith(right, result);
        return result;
    }

    public static BitSet operator <<(BitSet set, int count)
    {
        var result = new BitSet(set);
        result.ShiftLeft(count);
        return result;
    }

    public static BitSet operator >>(BitSet set, int count)
    {
        var result = new BitSet(set);
        result.ShiftRight(count);
        return result;
    }
}