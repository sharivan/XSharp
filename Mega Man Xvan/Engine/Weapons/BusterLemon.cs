using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMX.Geometry;
using MMX.Math;

using static MMX.Engine.Consts;

namespace MMX.Engine.Weapons
{
    public class BusterLemon : Weapon
    {
        private bool dashLemon;

        private int[] animationIndices;

        private bool reflected;
        private bool exploding;

        new public Player Shooter
        {
            get
            {
                return (Player) base.Shooter;
            }
        }

        internal BusterLemon(GameEngine engine, Player shooter, string name, Vector origin, Direction direction, bool dashLemon, SpriteSheet sheet) :
            base(engine, shooter, name, origin, direction, sheet)
        {
            this.dashLemon = dashLemon;

            CheckCollisionWithWorld = false;

            animationIndices = new int[2];
        }

        public override FixedSingle GetGravity()
        {
            return reflected && dashLemon && !exploding ? GRAVITY : FixedSingle.ZERO;
        }

        protected override Box GetCollisionBox()
        {
            return new Box(Vector.NULL_VECTOR, new Vector(-LEMON_HITBOX_WIDTH * 0.5, -LEMON_HITBOX_HEIGHT * 0.5), new Vector(LEMON_HITBOX_WIDTH * 0.5, LEMON_HITBOX_HEIGHT * 0.5));
        }

        public override void Spawn()
        {
            base.Spawn();

            vel = new Vector(Direction == Direction.LEFT ? (dashLemon ? -LEMON_TERMINAL_SPEED : -LEMON_INITIAL_SPEED) : (dashLemon ? LEMON_TERMINAL_SPEED : LEMON_INITIAL_SPEED), 0);

            CurrentAnimationIndex = animationIndices[0];
            CurrentAnimation.StartFromBegin();
        }

        protected override void Think()
        {
            if (!exploding)
            {
                vel += new Vector(vel.X > 0 ? LEMON_ACCELERATION : -LEMON_ACCELERATION, 0);
                if (vel.X.Abs > LEMON_TERMINAL_SPEED)
                    vel = new Vector(vel.X > 0 ? LEMON_TERMINAL_SPEED : -LEMON_TERMINAL_SPEED, vel.Y);
            }

            base.Think();
        }

        public void Explode()
        {
            if (!exploding)
            {
                exploding = true;
                vel = Vector.NULL_VECTOR;
                CurrentAnimationIndex = animationIndices[1];
                CurrentAnimation.StartFromBegin();
            }
        }

        public void Reflect()
        {
            if (!reflected && !exploding)
            {
                reflected = true;
                vel = new Vector(-vel.X, LEMON_REFLECTION_VSPEED);
            }
        }

        protected override void OnDeath()
        {
            Shooter.shots--;

            base.OnDeath();
        }

        protected override void OnCreateAnimation(int animationIndex, SpriteSheet sheet, ref string frameSequenceName, ref int initialSequenceIndex, ref bool startVisible, ref bool startOn, ref bool add)
        {
            base.OnCreateAnimation(animationIndex, sheet, ref frameSequenceName, ref initialSequenceIndex, ref startVisible, ref startOn, ref add);
            startOn = false;
            startVisible = false;

            if (frameSequenceName == "LemonShot")
                animationIndices[0] = Direction == Direction.LEFT ? animationIndex + 1 : animationIndex;
            else if (frameSequenceName == "LemonShotExplode")
                animationIndices[1] = Direction == Direction.LEFT ? animationIndex + 1 : animationIndex;
            else
                add = false;
        }

        internal override void OnAnimationEnd(Animation animation)
        {
            if (animation.Index == animationIndices[1])
                Kill();
        }
    }
}
