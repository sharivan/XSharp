using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MMX.Geometry;

namespace MMX.Engine
{
    public abstract class Boss : Enemy
    {
        protected Boss(GameEngine engine, string name, Vector origin, SpriteSheet sheet) : base(engine, name, origin, sheet)
        {
        }
    }
}
