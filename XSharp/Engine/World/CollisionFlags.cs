using System;

namespace XSharp.Engine.World
{
    [Flags]
    public enum CollisionFlags
    {
        NONE = 0,
        BLOCK = 1,
        SPIKE = 2,
        SLOPE = 4,
        LADDER = 8,
        TOP_LADDER = 16,
        UNCLIMBABLE = 32,
        WATER = 64,
        WATER_SURFACE = 128
    }

    public static class CollisionFlagsExtensions
    {
        public static bool CanBlockTheMove(this CollisionFlags flags, Direction direction = Direction.ALL)
        {
            return flags != CollisionFlags.NONE
                && (
                    flags.HasFlag(CollisionFlags.BLOCK)
                    || (direction.HasFlag(Direction.LEFT) || direction.HasFlag(Direction.RIGHT)) && flags.HasFlag(CollisionFlags.UNCLIMBABLE) // TODO : Check if slopes must be checked here too
                    || direction.HasFlag(Direction.DOWN) && (flags.HasFlag(CollisionFlags.TOP_LADDER) || flags.HasFlag(CollisionFlags.SLOPE))
                );
        }
    }
}