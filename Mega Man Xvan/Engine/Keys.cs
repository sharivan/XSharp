using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMX.Engine
{
    public enum Keys
    {
        NONE = 0,
        LEFT = 1,
        UP = 2,
        RIGHT = 4,
        DOWN = 8,
        SHOT = 16,
        JUMP = 32,
        DASH = 64,
        WEAPON = 128,
        LWS = 256,
        RWS = 512,
        START = 1024,
        SELECT = 2048
    }
}
