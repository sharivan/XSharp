using MMX.Engine.Entities.Enemies;
using MMX.Geometry;
using MMX.Math;

using static MMX.Engine.Consts;

namespace MMX.Engine.Entities.Weapons
{
    public enum ChargedState
    {
        FIRING = 0,
        SHOOTING = 1,
        HITTING = 2,
        EXPLODING = 3
    }

    public class BusterCharged : Weapon
    {
        private Entity hitEntity;

        new public Player Shooter => (Player) base.Shooter;

        public bool Firing => GetState<ChargedState>() == ChargedState.FIRING;

        public bool Exploding => GetState<ChargedState>() == ChargedState.EXPLODING;

        public bool Hitting => GetState<ChargedState>() == ChargedState.HITTING;

        internal BusterCharged(Player shooter, string name, Vector origin, Direction direction)
            : base(shooter, name, origin, direction, 1)
        {
            SetupStateArray(typeof(ChargedState));
            RegisterState(ChargedState.FIRING, OnStartFiring, null, null, "ChargedShotFiring");
            RegisterState(ChargedState.SHOOTING, OnStartShooting, OnShooting, null, "ChargedShot");
            RegisterState(ChargedState.HITTING, OnStartHitting, null, null, "ChargedShotHit");
            RegisterState(ChargedState.EXPLODING, OnStartExploding, null, null, "ChargedShotExplode");
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

        protected override FixedSingle GetBaseDamage()
        {
            return CHARGED_DAMAGE;
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            CheckCollisionWithWorld = false;
            Velocity = Vector.NULL_VECTOR;

            SetState(ChargedState.FIRING);
        }

        private void OnStartFiring(EntityState state)
        {
            Engine.PlaySound(1, 2);
        }

        private void OnStartShooting(EntityState state)
        {
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
        }

        private void OnShooting(EntityState state, long frameCounter)
        {
            Velocity = Direction == Direction.LEFT ? CHARGED_SPEED * Vector.LEFT_VECTOR : CHARGED_SPEED * Vector.RIGHT_VECTOR;
        }

        private void OnStartHitting(EntityState state)
        {
            if (hitEntity != null)
            {
                Box otherHitbox = hitEntity.Hitbox;
                Vector center = Hitbox.Center;
                FixedSingle x = Direction == Direction.RIGHT ? otherHitbox.Left : otherHitbox.Right;
                FixedSingle y = center.Y < otherHitbox.Top ? otherHitbox.Top : center.Y > otherHitbox.Bottom ? otherHitbox.Bottom : Origin.Y;
                Origin = (x, y);
            }

            Velocity = Vector.NULL_VECTOR;
        }

        private void OnStartExploding(EntityState state)
        {
            Velocity = Vector.NULL_VECTOR;
        }

        public void Explode()
        {
            SetState(ChargedState.EXPLODING);
        }

        public override void Hit(Enemy enemy)
        {
            base.Hit(enemy);

            if (!enemy.Broke)
            {
                hitEntity = enemy;
                SetState(ChargedState.HITTING);
            }
        }

        public override void Dispose()
        {
            Shooter.shots--;
            Shooter.shootingCharged = false;

            base.Dispose();
        }

        protected internal override void OnAnimationEnd(Animation animation)
        {
            base.OnAnimationEnd(animation);

            if (animation.FrameSequenceName != CurrentState.AnimationName)
                return;

            if (Firing)
                SetState(ChargedState.SHOOTING);
            else if (Hitting || Exploding)
                KillOnNextFrame();
        }
    }
}
