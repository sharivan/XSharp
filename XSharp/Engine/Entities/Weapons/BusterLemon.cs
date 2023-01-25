using MMX.Engine.Entities.Enemies;
using MMX.Geometry;
using MMX.Math;

using static MMX.Engine.Consts;

namespace MMX.Engine.Entities.Weapons
{
    public class BusterLemon : Weapon
    {
        private readonly bool dashLemon;

        private readonly int[] animationIndices;

        private bool reflected;
        private bool exploding;
        private bool soundPlayed;

        new public Player Shooter => (Player) base.Shooter;

        internal BusterLemon(GameEngine engine, Player shooter, string name, Vector origin, Direction direction, bool dashLemon, int spriteSheetIndex) :
            base(engine, shooter, name, origin, direction, spriteSheetIndex)
        {
            this.dashLemon = dashLemon;

            CheckCollisionWithWorld = false;

            animationIndices = new int[2];
        }

        public override FixedSingle GetGravity() => reflected && dashLemon && !exploding ? GRAVITY : FixedSingle.ZERO;

        protected override Box GetCollisionBox() => new(Vector.NULL_VECTOR, new Vector(-LEMON_HITBOX_WIDTH * 0.5, -LEMON_HITBOX_HEIGHT * 0.5), new Vector(LEMON_HITBOX_WIDTH * 0.5, LEMON_HITBOX_HEIGHT * 0.5));

        public override void OnSpawn()
        {
            base.OnSpawn();

            Velocity = new Vector(Direction == Direction.LEFT ? (dashLemon ? -LEMON_TERMINAL_SPEED : -LEMON_INITIAL_SPEED) : (dashLemon ? LEMON_TERMINAL_SPEED : LEMON_INITIAL_SPEED), 0);

            CurrentAnimationIndex = animationIndices[0];
            CurrentAnimation.StartFromBegin();
        }

        protected override void Think()
        {
            if (!exploding)
            {
                if (!soundPlayed)
                {
                    if (!Shooter.shootingCharged)
                        Engine.PlaySound(1, 0);

                    soundPlayed = true;
                }

                Velocity += new Vector(Velocity.X > 0 ? LEMON_ACCELERATION : -LEMON_ACCELERATION, 0);
                if (Velocity.X.Abs > LEMON_TERMINAL_SPEED)
                    Velocity = new Vector(Velocity.X > 0 ? LEMON_TERMINAL_SPEED : -LEMON_TERMINAL_SPEED, Velocity.Y);
            }

            base.Think();
        }

        public void Explode(Entity entity)
        {
            if (!exploding)
            {
                if (entity != null)
                {
                    Box otherHitbox = entity.HitBox;
                    Vector center = HitBox.Center;
                    FixedSingle x = Direction == Direction.RIGHT ? otherHitbox.Left : otherHitbox.Right;
                    FixedSingle y = center.Y < otherHitbox.Top ? otherHitbox.Top : center.Y > otherHitbox.Bottom ? otherHitbox.Bottom : Origin.Y;
                    Origin = (x, y);
                }

                exploding = true;
                Velocity = Vector.NULL_VECTOR;
                CurrentAnimationIndex = animationIndices[1];
                CurrentAnimation.StartFromBegin();
            }
        }

        public void Reflect()
        {
            if (!reflected && !exploding)
            {
                reflected = true;
                Velocity = new Vector(-Velocity.X, LEMON_REFLECTION_VSPEED);
            }
        }

        protected override void OnDeath()
        {
            Shooter.shots--;

            base.OnDeath();
        }

        protected override void OnStartTouch(Entity entity)
        {
            if (entity is Enemy)
                Explode(entity);

            base.OnStartTouch(entity);
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
