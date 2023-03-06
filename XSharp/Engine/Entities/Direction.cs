using System;

using XSharp.Math.Geometry;

namespace XSharp.Engine;

[Flags]
public enum Direction
{
    NONE = 0, // Nenhuma
    LEFT = 1, // Esquerda
    UP = 2, // Cima
    RIGHT = 4, // Direita
    DOWN = 8, // Baixo

    LEFTUP = LEFT | UP,
    LEFTDOWN = LEFT | DOWN,
    RIGHTUP = RIGHT | UP,
    RIGHTDOWN = RIGHT | DOWN,
    LEFTRIGHT = LEFT | RIGHT,
    UPDOWN = UP | DOWN,
    ALL = LEFT | UP | RIGHT | DOWN
}

public static class DirectionExtensions
{
    public static Direction Oposite(this Direction direction)
    {
        Direction result = Direction.NONE;

        if (direction.HasFlag(Direction.LEFT))
            result |= Direction.RIGHT;
        else if (direction.HasFlag(Direction.RIGHT))
            return Direction.LEFT;

        if (direction.HasFlag(Direction.UP))
            result |= Direction.DOWN;
        else if (direction.HasFlag(Direction.DOWN))
            return Direction.UP;

        return result;
    }

    public static Direction GetDirection(this Vector v)
    {
        return (v.X > 0 ? Direction.RIGHT : v.X < 0 ? Direction.LEFT : 0) | (v.Y > 0 ? Direction.DOWN : v.Y < 0 ? Direction.UP : 0);
    }
}