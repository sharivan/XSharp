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
    public class BusterSemiCharged : Weapon
    {
        private bool firing;
        private bool exploding;
        private bool hitting;

        private int[] animationIndices;

        new public Player Shooter
        {
            get
            {
                return (Player) base.Shooter;
            }
        }

        public bool Firing
        {
            get
            {
                return firing;
            }
        }

        public bool Exploding
        {
            get
            {
                return exploding;
            }
        }

        public bool Hitting
        {
            get
            {
                return hitting;
            }
        }

        internal BusterSemiCharged(GameEngine engine, Player shooter, string name, Vector origin, Direction direction, SpriteSheet sheet) :
            base(engine, shooter, name, origin, direction, sheet)
        {
            CheckCollisionWithWorld = false;

            animationIndices = new int[4];
        }

        public override FixedSingle GetGravity()
        {
            return FixedSingle.ZERO;
        }

        protected override Box GetCollisionBox()
        {
            Animation animation = CurrentAnimation;
            return animation != null ? animation.CurrentFrameCollisionBox : Box.EMPTY_BOX;
        }

        public override void Spawn()
        {
            base.Spawn();

            firing = true;
            exploding = false;
            hitting = false;

            vel = Vector.NULL_VECTOR;         

            CurrentAnimationIndex = animationIndices[0];
            CurrentAnimation.StartFromBegin();
        }

        protected override void Think()
        {
            if (!firing && !exploding && !hitting)
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
                CurrentAnimationIndex = animationIndices[3];
                CurrentAnimation.StartFromBegin();
            }
        }

        public void Hit()
        {
            if (!hitting)
            {
                hitting = true;
                vel = Vector.NULL_VECTOR;
                CurrentAnimationIndex = animationIndices[2];
                CurrentAnimation.StartFromBegin();
            }
        }

        protected override void OnDeath()
        {
            Shooter.shots--;
            Shooter.shootingCharged = false;

            base.OnDeath();
        }

        protected override void OnCreateAnimation(int animationIndex, SpriteSheet sheet, ref string frameSequenceName, ref int initialFrame, ref bool startVisible, ref bool startOn, ref bool add)
        {
            base.OnCreateAnimation(animationIndex, sheet, ref frameSequenceName, ref initialFrame, ref startVisible, ref startOn, ref add);
            startOn = false;
            startVisible = false;

            if (frameSequenceName == "SemiChargedShotFiring")
                animationIndices[0] = Direction == Direction.LEFT ? animationIndex + 1 : animationIndex;
            else if (frameSequenceName == "SemiChargedShot")
                animationIndices[1] = Direction == Direction.LEFT ? animationIndex + 1 : animationIndex;
            else if (frameSequenceName == "SemiChargedShotHit")
                animationIndices[2] = Direction == Direction.LEFT ? animationIndex + 1 : animationIndex;
            else if (frameSequenceName == "SemiChargedShotExplode")
                animationIndices[3] = Direction == Direction.LEFT ? animationIndex + 1 : animationIndex;
            else
                add = false;
        }

        internal override void OnAnimationEnd(Animation animation)
        {
            if (animation.Index == animationIndices[0])
            {
                firing = false;
                
                if (Direction == Direction.LEFT)
                {
                    Origin += 14 * Vector.LEFT_VECTOR;
                    vel = SEMI_CHARGED_INITIAL_SPEED * Vector.LEFT_VECTOR;
                }
                else
                {
                    Origin += 14 * Vector.RIGHT_VECTOR;
                    vel = SEMI_CHARGED_INITIAL_SPEED * Vector.RIGHT_VECTOR;
                }                

                CurrentAnimationIndex = animationIndices[1];
                CurrentAnimation.StartFromBegin();
            }
            else if (animation.Index == animationIndices[2] || animation.Index == animationIndices[3])
                Kill();
        }
    }
}
