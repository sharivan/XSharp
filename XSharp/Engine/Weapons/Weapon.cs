using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MMX.Geometry;
using MMX.Math;

namespace MMX.Engine.Weapons
{
    public abstract class Weapon : Sprite
    {
        public Sprite Shooter { get; }

        public Direction Direction { get; }

        protected Weapon(GameEngine engine, Sprite shooter, string name, Vector origin, Direction direction, SpriteSheet sheet) : base(engine, name, origin, sheet, true)
        {
            this.Shooter = shooter;
            this.Direction = direction;

            CanGoOutOfMapBounds = true;
        }

        protected override void Think()
        {
            base.Think();

            if (Offscreen)
                Kill();
        }
    }
}
