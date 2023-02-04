using System;

namespace XSharp.Engine
{
    public static class Utils
    {
        /// <summary>
        /// Coverte uma direção para um número inteiro
        /// </summary>
        /// <param name="direction">Direção</param>
        /// <returns>Número inteiro associado a direção dada</returns>
        public static int DirectionToInt(Direction direction)
        {
            return direction switch
            {
                Direction.NONE => 0,
                Direction.LEFT => 1,
                Direction.UP => 2,
                Direction.RIGHT => 4,
                Direction.DOWN => 8,
                _ => throw new InvalidOperationException("There is no integer liked to direction " + direction.ToString() + "."),
            };
        }

        /// <summary>
        /// Converte um número inteiro para uma direção
        /// </summary>
        /// <param name="value">Número inteiro a ser convertido</param>
        /// <returns>Direção associada ao número inteiro dado</returns>
        public static Direction IntToDirection(int value)
        {
            return value switch
            {
                0 => Direction.NONE,
                1 => Direction.LEFT,
                2 => Direction.UP,
                4 => Direction.RIGHT,
                8 => Direction.DOWN,
                _ => throw new InvalidOperationException("There is no direction liked to value " + value + "."),
            };
        }
    }
}
