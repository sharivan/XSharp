using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMX.Engine
{
    public enum CollisionFlags
    {
        NONE = 0,
        BLOCK = 1,
        SLOPE = 2,
        LADDER = 4,
        TOP_LADDER = 8
    }
}
