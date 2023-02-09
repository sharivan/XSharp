using System;

namespace XSharp.Engine
{
    public class BitSet
    {
        private long[] bits;

        public int BitCount
        {
            get;
        }

        public BitSet(int bitCount)
        {
            if (bitCount < 0)
                throw new ArgumentException($"Invalid negative bitcount value '{bitCount}'.");

            BitCount = (int) GameEngine.NextHighestPowerOfTwo((uint) bitCount) / sizeof(long);
            bits = new long[BitCount];
        }

        public BitSet() : this(128)
        {
        }

        public void Set(int index)
        {
            int slot = index / BitCount;
            int bit = index - slot;
            bits[slot] |= 1L << bit;
        }

        public void Reset(int index)
        {
            int slot = index / BitCount;
            int bit = index - slot;
            bits[slot] &= ~(1L << bit);
        }

        public bool Test(int index)
        {
            int slot = index / BitCount;
            int bit = index - slot;
            long slotBits = bits[slot];
            return (slotBits & (1L << bit)) != 0;
        }

        public void Clear()
        {
            for (int i = 0; i < bits.Length; i++)
                bits[i] = 0;
        }
    }
}