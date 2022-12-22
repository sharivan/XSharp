using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMX.Geometry;
using MMX.Math;

namespace MMX.Engine.Enemies
{
    public class Driller : Enemy
    {
        public enum DrillerState
        {
            IDLE = 0,
            DRILLING = 1,
            JUMPING = 2
        }

        public DrillerState State { get;
            private set;
        }

        public override void Spawn()
        {
            base.Spawn();

            State = DrillerState.IDLE;
            CheckCollisionWithWorld = true;
            CheckCollisionWithSprites = false;
            CurrentAnimationIndex = 0;
        }

        public Driller(GameEngine engine, string name, Vector origin, SpriteSheet sheet) : base(engine, name, origin, sheet) => Health = 2;
    }
}
