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
    public class BusterCharged : Weapon
    {
        private bool firing;
        private bool exploding;
        private bool hitting;

        private int currentAnimationIndex;
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

        internal BusterCharged(GameEngine engine, Player shooter, string name, Vector origin, Direction direction, SpriteSheet sheet) :
            base(engine, shooter, name, origin, direction, sheet)
        {
            CheckCollisionWithWorld = false;

            animationIndices = new int[4];
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
                    animationFrame = animation.CurrentSequenceIndex;
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
                animation.CurrentSequenceIndex = animationFrame != -1 ? animationFrame : 0;
                animation.Animating = animating;
                animation.Visible = true;
            }
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
            currentAnimationIndex = -1;

            vel = Vector.NULL_VECTOR;

            CurrentAnimationIndex = animationIndices[0];
            CurrentAnimation.StartFromBegin();
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

            if (frameSequenceName == "ChargedShotFiring")
                animationIndices[0] = Direction == Direction.LEFT ? animationIndex + 1 : animationIndex;
            else if (frameSequenceName == "ChargedShot")
                animationIndices[1] = Direction == Direction.LEFT ? animationIndex + 1 : animationIndex;
            else if (frameSequenceName == "ChargedShotHit")
                animationIndices[2] = Direction == Direction.LEFT ? animationIndex + 1 : animationIndex;
            else if (frameSequenceName == "ChargedShotExplode")
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
                    //Origin += 14 * Vector.LEFT_VECTOR;
                    vel = CHARGED_SPEED * Vector.LEFT_VECTOR;
                }
                else
                {
                    //Origin += 14 * Vector.RIGHT_VECTOR;
                    vel = CHARGED_SPEED * Vector.RIGHT_VECTOR;
                }

                CurrentAnimationIndex = animationIndices[1];
                CurrentAnimation.StartFromBegin();
            }
            else if (animation.Index == animationIndices[2] || animation.Index == animationIndices[3])
                Kill();
        }
    }
}
