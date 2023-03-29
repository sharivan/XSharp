using System;

namespace XSharp.Engine.Collision;

[Flags]
public enum CollisionFlags
{
    NONE = 0,
    BLOCK = 1,
    SPIKE = 2,
    SLOPE = 4,
    LADDER = 8,
    WATER = 16,
    SHIFT1 = 32, // used to detect if spike is non lethal, ladder is top ladder, water is water surface or conveyor is left conveyor
    SHIFT2 = 64, // used to detect if a wall is unclimbable
    SHIFT3 = 128, // used to detect if a floor or wall is slippery
    SHIFT4 = 256, // used to detect if is conveyor

    LETHAL_SPIKE = SPIKE,
    NON_LETHAL_SPIKE = SPIKE | SHIFT1,
    CLIMBABLE_WALL = BLOCK | SPIKE,
    CLIMBABLE_BLOCK = BLOCK,
    CLIMBABLE_SPIKE = SPIKE,
    UNCLIMBABLE_WALL = BLOCK | SPIKE | SHIFT2,
    UNCLIMBABLE_BLOCK = BLOCK | SHIFT2,
    UNCLIMBABLE_SPIKE = SPIKE | SHIFT2,
    SLIPPERY = BLOCK | SHIFT3,
    CONVEYOR = BLOCK | SLOPE | SHIFT4,
    BLOCK_CONVEYOR = BLOCK | SHIFT4,
    LEFT_CONVEYOR = CONVEYOR | SHIFT1,
    LEFT_BLOCK_CONVEYOR = BLOCK_CONVEYOR | SHIFT1,
    RIGHT_CONVEYOR = CONVEYOR,
    RIGHT_BLOCK_CONVEYOR = BLOCK_CONVEYOR,
    SLOPE_CONVEYOR = SLOPE | SHIFT4,
    LEFT_SLOPE_CONVEYOR = SLOPE_CONVEYOR | SHIFT1,
    RIGHT_SLOPE_CONVEYOR = SLOPE_CONVEYOR,
    TOP_LADDER = LADDER | SHIFT1,
    WATER_SURFACE = WATER | SHIFT1
}

public static class CollisionFlagsExtensions
{
    public static CollisionFlags GetShiftModifiers(this CollisionFlags flags)
    {
        return flags & (CollisionFlags.SHIFT1 | CollisionFlags.SHIFT2 | CollisionFlags.SHIFT3 | CollisionFlags.SHIFT4);
    }

    public static bool CanBlockTheMove(this CollisionFlags flags, Direction direction = Direction.BOTH)
    {
        return flags != CollisionFlags.NONE
            && (
                flags.HasFlag(CollisionFlags.BLOCK)
                || flags.HasFlag(CollisionFlags.SPIKE)
                || flags.HasFlag(CollisionFlags.CONVEYOR)
                || (direction.HasFlag(Direction.LEFT) || direction.HasFlag(Direction.RIGHT)) && flags.IsSlope() // TODO : Check if slopes must be really checked here too
                || direction.HasFlag(Direction.DOWN) && (flags.IsTopLadder() || flags.IsSlope())
            );
    }

    public static bool IsBlock(this CollisionFlags flags)
    {
        return flags.HasFlag(CollisionFlags.BLOCK);
    }

    public static bool IsSpike(this CollisionFlags flags)
    {
        return flags.HasFlag(CollisionFlags.SPIKE);
    }

    public static bool IsSlope(this CollisionFlags flags)
    {
        return flags.HasFlag(CollisionFlags.SLOPE);
    }

    public static bool IsLadder(this CollisionFlags flags)
    {
        return flags.HasFlag(CollisionFlags.LADDER);
    }

    public static bool IsWater(this CollisionFlags flags)
    {
        return flags.HasFlag(CollisionFlags.WATER);
    }

    public static bool IsLethalSpike(this CollisionFlags flags)
    {
        return flags.HasFlag(CollisionFlags.SPIKE) && !flags.HasFlag(CollisionFlags.SHIFT1);
    }

    public static bool IsNonLethalSpike(this CollisionFlags flags)
    {
        return flags.HasFlag(CollisionFlags.SPIKE) && flags.HasFlag(CollisionFlags.SHIFT1);
    }

    public static bool IsClimbable(this CollisionFlags flags)
    {
        return (flags.HasFlag(CollisionFlags.BLOCK) || flags.HasFlag(CollisionFlags.SPIKE) || flags.HasFlag(CollisionFlags.CONVEYOR)) && !flags.HasFlag(CollisionFlags.SHIFT2);
    }

    public static bool IsUnclimbable(this CollisionFlags flags)
    {
        return (flags.HasFlag(CollisionFlags.BLOCK) || flags.HasFlag(CollisionFlags.SPIKE) || flags.HasFlag(CollisionFlags.CONVEYOR)) && flags.HasFlag(CollisionFlags.SHIFT2);
    }

    public static bool IsSlippery(this CollisionFlags flags)
    {
        return (flags.HasFlag(CollisionFlags.BLOCK) || flags.HasFlag(CollisionFlags.SLOPE)) && flags.HasFlag(CollisionFlags.SHIFT3);
    }

    public static bool IsLeftConveyor(this CollisionFlags flags)
    {
        return (flags.HasFlag(CollisionFlags.BLOCK) || flags.HasFlag(CollisionFlags.SLOPE)) && flags.HasFlag(CollisionFlags.SHIFT4) && flags.HasFlag(CollisionFlags.SHIFT1);
    }

    public static bool IsRightConveyor(this CollisionFlags flags)
    {
        return (flags.HasFlag(CollisionFlags.BLOCK) || flags.HasFlag(CollisionFlags.SLOPE)) && flags.HasFlag(CollisionFlags.SHIFT4) && !flags.HasFlag(CollisionFlags.SHIFT1);
    }

    public static bool IsLeftSlope(this CollisionFlags flags)
    {
        return flags.HasFlag(CollisionFlags.SLOPE) && flags.HasFlag(CollisionFlags.SHIFT4) && flags.HasFlag(CollisionFlags.SHIFT1);
    }

    public static bool IsRightSlope(this CollisionFlags flags)
    {
        return flags.HasFlag(CollisionFlags.SLOPE) && flags.HasFlag(CollisionFlags.SHIFT4) && !flags.HasFlag(CollisionFlags.SHIFT1);
    }

    public static bool IsBottomLadder(this CollisionFlags flags)
    {
        return flags.HasFlag(CollisionFlags.LADDER) && !flags.HasFlag(CollisionFlags.SHIFT1);
    }

    public static bool IsTopLadder(this CollisionFlags flags)
    {
        return flags.HasFlag(CollisionFlags.LADDER) && flags.HasFlag(CollisionFlags.SHIFT1);
    }

    public static bool IsUnderwater(this CollisionFlags flags)
    {
        return flags.HasFlag(CollisionFlags.WATER) && !flags.HasFlag(CollisionFlags.SHIFT1);
    }

    public static bool IsWaterSurface(this CollisionFlags flags)
    {
        return flags.HasFlag(CollisionFlags.WATER) && flags.HasFlag(CollisionFlags.SHIFT1);
    }
}