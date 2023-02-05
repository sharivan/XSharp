using System;

namespace XSharp.Engine.Entities
{
    [Flags]
    public enum VectorKind
    {
        NONE = 0,
        ORIGIN = 1,
        BOUDINGBOX_CENTER = 2,
        HITBOX_CENTER = 4,
        ALL = 255
    }

    public static class VectorKindExtensions
    {
        public static VectorKind ToVectorKind(this int index)
        {
            return (VectorKind) (1 << index);
        }

        public static int ToIndex(this VectorKind kind)
        {
            int x = (int) kind;

            // Map a bit value mod
            // 37 to its position
            int[] lookup = {32, 0, 1, 26, 2, 23,
                    27, 0, 3, 16, 24, 30,
                    28, 11, 0, 13, 4, 7,
                    17, 0, 25, 22, 31, 15,
                    29, 10, 12, 6, 0, 21,
                    14, 9, 5, 20, 8, 19, 18};

            // Only difference between
            // (x and -x) is the value
            // of signed magnitude
            // (leftmostbit) negative
            // numbers signed bit is 1
            return lookup[(-x & x) % 37];
        }

        public static bool ContainsFlag(this VectorKind kind, VectorKind flag)
        {
            return ((int) kind & (int) flag) != 0;
        }

        public static bool ContainsFlag(this VectorKind kind, int index)
        {
            return kind.ContainsFlag(index.ToVectorKind());
        }
    }
}