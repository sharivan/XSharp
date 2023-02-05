using System;

namespace XSharp.Engine.Entities
{
    [Flags]
    public enum BoxKind
    {
        NONE = 0,
        BOUDINGBOX = 1,
        HITBOX = 2,
        COLLISIONBOX = 4,
        ALL = 255
    }

    public static class BoxKindExtensions
    {
        public static BoxKind ToBoxKind(this int index)
        {
            return (BoxKind) (1 << index);
        }

        public static int ToIndex(this BoxKind kind)
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

        public static bool ContainsFlag(this BoxKind kind, BoxKind flag)
        {
            return ((int) kind & (int) flag) != 0;
        }

        public static bool ContainsFlag(this BoxKind kind, int index)
        {
            return kind.ContainsFlag(index.ToBoxKind());
        }
    }
}