using MMX.Geometry;
using MMX.Math;

using static MMX.Engine.Consts;

namespace MMX.Engine.Entities.Weapons
{
    public class BusterSemiCharged : Weapon
    {
        private readonly int[] animationIndices;

        private bool soundPlayed;

        new public Player Shooter => (Player) base.Shooter;

        public bool Firing { get;
            private set;
        }

        public bool Exploding { get;
            private set;
        }

        public bool Hitting { get;
            private set;
        }

        internal BusterSemiCharged(GameEngine engine, Player shooter, string name, Vector origin, Direction direction, int spriteSheetIndex) :
            base(engine, shooter, name, origin, direction, spriteSheetIndex)
        {
            CheckCollisionWithWorld = false;

            animationIndices = new int[4];
        }

        public override FixedSingle GetGravity() => FixedSingle.ZERO;

        protected override Box GetCollisionBox()
        {
            Animation animation = CurrentAnimation;
            return animation != null ? animation.CurrentFrameCollisionBox : Box.EMPTY_BOX;
        }

        public override void Spawn()
        {
            base.Spawn();

            Firing = true;
            Exploding = false;
            Hitting = false;

            Velocity = Vector.NULL_VECTOR;         

            CurrentAnimationIndex = animationIndices[0];
            CurrentAnimation.StartFromBegin();
        }

        protected override void Think()
        {
            if (!Firing && !Exploding && !Hitting)
            {
                if (!soundPlayed)
                {
                    Engine.PlaySound(1, 1);
                    soundPlayed = true;
                }

                Velocity += new Vector(Velocity.X > 0 ? LEMON_ACCELERATION : -LEMON_ACCELERATION, 0);
                if (Velocity.X.Abs > LEMON_TERMINAL_SPEED)
                    Velocity = new Vector(Velocity.X > 0 ? LEMON_TERMINAL_SPEED : -LEMON_TERMINAL_SPEED, Velocity.Y);
            }

            base.Think();
        }

        public void Explode()
        {
            if (!Exploding)
            {
                Exploding = true;
                Velocity = Vector.NULL_VECTOR;
                CurrentAnimationIndex = animationIndices[3];
                CurrentAnimation.StartFromBegin();
            }
        }

        public void Hit()
        {
            if (!Hitting)
            {
                Hitting = true;
                Velocity = Vector.NULL_VECTOR;
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
                Firing = false;
                
                if (Direction == Direction.LEFT)
                {
                    Origin += 14 * Vector.LEFT_VECTOR;
                    Velocity = SEMI_CHARGED_INITIAL_SPEED * Vector.LEFT_VECTOR;
                }
                else
                {
                    Origin += 14 * Vector.RIGHT_VECTOR;
                    Velocity = SEMI_CHARGED_INITIAL_SPEED * Vector.RIGHT_VECTOR;
                }                

                CurrentAnimationIndex = animationIndices[1];
                CurrentAnimation.StartFromBegin();
            }
            else if (animation.Index == animationIndices[2] || animation.Index == animationIndices[3])
                Kill();
        }
    }
}
