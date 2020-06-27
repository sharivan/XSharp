using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMX.Engine
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
            switch (direction)
            {
                case Direction.NONE:
                    return 0;

                case Direction.LEFT:
                    return 1;

                case Direction.UP:
                    return 2;

                case Direction.RIGHT:
                    return 4;

                case Direction.DOWN:
                    return 8;
            }

            throw new InvalidOperationException("There is no integer liked to direction " + direction.ToString() + ".");
        }

        /// <summary>
        /// Converte um número inteiro para uma direção
        /// </summary>
        /// <param name="value">Número inteiro a ser convertido</param>
        /// <returns>Direção associada ao número inteiro dado</returns>
        public static Direction IntToDirection(int value)
        {
            switch (value)
            {
                case 0:
                    return Direction.NONE;

                case 1:
                    return Direction.LEFT;

                case 2:
                    return Direction.UP;

                case 4:
                    return Direction.RIGHT;

                case 8:
                    return Direction.DOWN;
            }

            throw new InvalidOperationException("There is no direction liked to value " + value + ".");
        }
    }
}
