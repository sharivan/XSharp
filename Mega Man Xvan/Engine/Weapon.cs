using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMX.Geometry;
using MMX.Math;

namespace MMX.Engine
{
    public abstract class Weapon : Sprite
    {
        private Sprite shooter;
        private Direction direction;

        public Sprite Shooter
        {
            get
            {
                return shooter;
            }
        }

        public Direction Direction
        {
            get
            {
                return direction;
            }
        }

        protected Weapon(GameEngine engine, Sprite shooter, string name, Vector origin, Direction direction, SpriteSheet sheet) : base(engine, name, origin, sheet, false, true)
        {
            this.shooter = shooter;
            this.direction = direction;

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
