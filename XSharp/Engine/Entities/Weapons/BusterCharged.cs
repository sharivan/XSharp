using MMX.Engine.Entities.Enemies;
using MMX.Geometry;
using MMX.Math;

using static MMX.Engine.Consts;

namespace MMX.Engine.Entities.Weapons
{
    public class BusterCharged : Weapon
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

        internal BusterCharged(GameEngine engine, Player shooter, string name, Vector origin, Direction direction, int spriteSheetIndex) :
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

        public override void OnSpawn()
        {
            base.OnSpawn();

            Firing = true;
            Exploding = false;
            Hitting = false;

            Velocity = Vector.NULL_VECTOR;

            CurrentAnimationIndex = animationIndices[0];
            CurrentAnimation.StartFromBegin();
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

        public void Hit(Entity entity)
        {
            if (!Hitting)
            {
                if (entity != null)
                {
                    Box otherHitbox = entity.HitBox;
                    Vector center = HitBox.Center;
                    FixedSingle x = Direction == Direction.RIGHT ? otherHitbox.Left : otherHitbox.Right;
                    FixedSingle y = center.Y < otherHitbox.Top ? otherHitbox.Top : center.Y > otherHitbox.Bottom ? otherHitbox.Bottom : Origin.Y;
                    Origin = (x, y);
                }

                Hitting = true;
                Velocity = Vector.NULL_VECTOR;
                CurrentAnimationIndex = animationIndices[2];
                CurrentAnimation.StartFromBegin();
            }
        }

        protected override void Think()
        {
            if (!soundPlayed)
            {
                Engine.PlaySound(1, 2);
                soundPlayed = true;
            }

            base.Think();
        }

        public override void Dispose()
        {
            Shooter.shots--;
            Shooter.shootingCharged = false;

            base.Dispose();
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

        protected override void OnStartTouch(Entity entity)
        {
            if (entity is Enemy)
                Hit(entity);

            base.OnStartTouch(entity);
        }

        internal override void OnAnimationEnd(Animation animation)
        {
            if (animation.Index == animationIndices[0])
            {
                Firing = false;

                if (Direction == Direction.LEFT)
                {
                    //Origin += 14 * Vector.LEFT_VECTOR;
                    Velocity = CHARGED_SPEED * Vector.LEFT_VECTOR;
                }
                else
                {
                    //Origin += 14 * Vector.RIGHT_VECTOR;
                    Velocity = CHARGED_SPEED * Vector.RIGHT_VECTOR;
                }

                CurrentAnimationIndex = animationIndices[1];
                CurrentAnimation.StartFromBegin();
            }
            else if (animation.Index == animationIndices[2] || animation.Index == animationIndices[3])
                Kill();
        }
    }
}
