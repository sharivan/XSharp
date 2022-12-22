namespace MMX.Engine
{
    public enum CollisionSide
    {
        NONE = 0,
        FLOOR = 1,
        LEFT_WALL = 2,
        CEIL = 4,
        RIGHT_WALL = 8,
        INNER = 16,

        CEIL_AND_FLOOR = CEIL | FLOOR,
        ALL_WALLS = LEFT_WALL | RIGHT_WALL,
        ALL_SIDES = FLOOR | LEFT_WALL | CEIL | RIGHT_WALL,
        ALL = ALL_SIDES | INNER
    }
}
