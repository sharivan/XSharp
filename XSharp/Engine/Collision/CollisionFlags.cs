using System;

namespace XSharp.Engine.Collision
{
    [Flags]
    public enum CollisionFlags
    {
        NONE = 0,
        BLOCK = 1,
        SLOPE = 2,
        LADDER = 4,
        TOP_LADDER = 8,
        UNCLIMBABLE = 16,
        WATER = 32,
        WATER_SURFACE = 64
    }

    public static class CollisionFlagsExtensions
    {
        public static bool CanBlockTheMove(this CollisionFlags flags, Direction direction = Direction.ALL)
        {
            return flags != CollisionFlags.NONE
                && (
                    flags.HasFlag(CollisionFlags.BLOCK)
                    || (direction.HasFlag(Direction.LEFT) || direction.HasFlag(Direction.RIGHT)) && (flags.HasFlag(CollisionFlags.UNCLIMBABLE) || flags.HasFlag(CollisionFlags.SLOPE)) // TODO : Check if slopes must be really checked here too
                    || direction.HasFlag(Direction.DOWN) && (flags.HasFlag(CollisionFlags.TOP_LADDER) || flags.HasFlag(CollisionFlags.SLOPE))
                );
        }
    }
}