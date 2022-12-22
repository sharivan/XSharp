using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMX.Engine
{
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
}
