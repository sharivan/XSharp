using System;
using XSharp.Math.Fixed.Geometry;

namespace XSharp.Engine;

[Flags]
public enum Direction
{
    NONE = 0, // Nenhuma
    LEFT = 1, // Esquerda
    UP = 2, // Cima
    RIGHT = 4, // Direita
    DOWN = 8, // Baixo

    LEF_TUP = LEFT | UP,
    LEFT_DOWN = LEFT | DOWN,
    RIGHT_UP = RIGHT | UP,
    RIGHT_DOWN = RIGHT | DOWN,
    BOTH_HORIZONTAL = LEFT | RIGHT,
    BOTH_VERTICAL = UP | DOWN,
    BOTH = LEFT | UP | RIGHT | DOWN
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

    public static Vector GetHorizontalUnitaryVector(this Direction direction)
    {
        return direction == Direction.LEFT ? Vector.LEFT_VECTOR : direction == Direction.RIGHT ? Vector.RIGHT_VECTOR : Vector.NULL_VECTOR;
    }

    public static Vector GetVerticalUnitaryVector(this Direction direction)
    {
        return direction == Direction.UP ? Vector.UP_VECTOR : direction == Direction.DOWN ? Vector.DOWN_VECTOR : Vector.NULL_VECTOR;
    }

    public static int GetHorizontalSignal(this Direction direction)
    {
        return direction == Direction.LEFT ? -1 : direction == Direction.RIGHT ? 1 : 0;
    }

    public static int GetVerticalSignal(this Direction direction)
    {
        return direction == Direction.UP ? -1 : direction == Direction.DOWN ? 1 : 0;
    }

    public static Vector GetUnitaryVector(this Direction direction)
    {
        var huv = direction.GetHorizontalUnitaryVector();
        var vuv = direction.GetVerticalUnitaryVector();
        var s = huv + vuv;
        return s.Versor();
    }
}