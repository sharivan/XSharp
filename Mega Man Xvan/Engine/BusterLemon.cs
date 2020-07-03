using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMX.Geometry;
using MMX.Math;

using static MMX.Engine.Consts;

namespace MMX.Engine
{
    public class BusterLemon : Weapon
    {
        private bool dashLemon;

        private int currentAnimationIndex;
        private int[] animationIndices;

        private bool reflected;

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

        protected Animation CurrentAnimation
        {
            get
            {
                return GetAnimation(currentAnimationIndex);
            }
        }

        protected int CurrentAnimationIndex
        {
            get
            {
                return currentAnimationIndex;
            }
            set
            {
                Animation animation = CurrentAnimation;
                bool animating;
                int animationFrame;
                if (animation != null)
                {
                    animating = animation.Animating;
                    animationFrame = animation.CurrentFrameSequenceIndex;
                    animation.Stop();
                    animation.Visible = false;
                }
                else
                {
                    animating = false;
                    animationFrame = -1;
                }

                currentAnimationIndex = value;
                animation = CurrentAnimation;
                animation.CurrentFrameSequenceIndex = animationFrame != -1 ? animationFrame : 0;
                animation.Animating = animating;
                animation.Visible = true;
            }
        }

        public override FixedSingle GetGravity()
        {
            return reflected && dashLemon ? GRAVITY :FixedSingle.ZERO;
        }

        protected override Box GetCollisionBox()
        {
            return new Box(Vector.NULL_VECTOR, new Vector(-LEMON_HITBOX_WIDTH * 0.5, -LEMON_HITBOX_HEIGHT * 0.5), new Vector(LEMON_HITBOX_WIDTH * 0.5, LEMON_HITBOX_HEIGHT * 0.5));
        }

        public override void Spawn()
        {
            base.Spawn();

            currentAnimationIndex = -1;

            vel = new Vector(Direction == Direction.LEFT ? (dashLemon ? -LEMON_TERMINAL_SPEED : -LEMON_INITIAL_SPEED) : (dashLemon ? LEMON_TERMINAL_SPEED : LEMON_INITIAL_SPEED), 0);

            CurrentAnimationIndex = animationIndices[0];
            CurrentAnimation.StartFromBegin();
        }

        protected override void Think()
        {
            vel += new Vector(vel.X > 0 ? LEMON_ACCELERATION : -LEMON_ACCELERATION, 0);
            if (vel.X.Abs > LEMON_TERMINAL_SPEED)
                vel = new Vector(vel.X > 0 ? LEMON_TERMINAL_SPEED : -LEMON_TERMINAL_SPEED, vel.Y);

            base.Think();
        }

        public void Reflect()
        {
            reflected = true;
            vel = new Vector(-vel.X, LEMON_REFLECTION_VSPEED);
        }

        protected override void OnDeath()
        {
            Shooter.shotLemons--;

            base.OnDeath();
        }

        protected override void OnCreateAnimation(int animationIndex, ref SpriteSheet sheet, ref string frameSequenceName, ref int initialFrame, ref bool startVisible, ref bool startOn)
        {
            base.OnCreateAnimation(animationIndex, ref sheet, ref frameSequenceName, ref initialFrame, ref startVisible, ref startOn);
            startOn = false;
            startVisible = false;

            if (frameSequenceName == "Shot")
                animationIndices[0] = Direction == Direction.LEFT ? animationIndex + 1 : animationIndex;
            else if (frameSequenceName == "ShotHit")
                animationIndices[1] = Direction == Direction.LEFT ? animationIndex + 1 : animationIndex;
        }
    }
}
