using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MMX.Geometry;

namespace MMX.Engine
{
    public abstract class Enemy : Sprite
    {
        protected Enemy(GameEngine engine, string name, Vector origin, SpriteSheet sheet) : base(engine, name, origin, sheet, false, true)
        {
        }
    }
}
