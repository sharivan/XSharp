namespace MMX.Engine
{
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
}
