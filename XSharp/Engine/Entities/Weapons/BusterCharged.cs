using XSharp.Engine.Entities.Enemies;
using XSharp.Math;
using XSharp.Math.Geometry;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Weapons
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

        new public Player Shooter
        {
            get => (Player) base.Shooter;
            internal set => base.Shooter = value;
        }

        public bool Firing => GetState<ChargedState>() == ChargedState.FIRING;

        public bool Exploding => GetState<ChargedState>() == ChargedState.EXPLODING;

        public bool Hitting => GetState<ChargedState>() == ChargedState.HITTING;

        public BusterCharged()
        {
            SpriteSheetName = "X Weapons";

            SetupStateArray(typeof(ChargedState));
            RegisterState(ChargedState.FIRING, OnStartFiring, "ChargedShotFiring");
            RegisterState(ChargedState.SHOOTING, OnStartShooting, OnShooting, null, "ChargedShot");
            RegisterState(ChargedState.HITTING, OnStartHitting, "ChargedShotHit");
            RegisterState(ChargedState.EXPLODING, OnStartExploding, "ChargedShotExplode");
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

            Direction = Shooter.WallSliding ? Shooter.Direction.Oposite() : Shooter.Direction;
            CheckCollisionWithWorld = false;
            Velocity = Vector.NULL_VECTOR;

            SetState(ChargedState.FIRING);
        }

        private void OnStartFiring(EntityState state)
        {
            if (Engine.Boss != null && !Engine.Boss.Exploding)
                Engine.PlaySound(1, "X Charge Shot");
        }

        private void OnStartShooting(EntityState state, EntityState lastState)
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

        private void OnStartHitting(EntityState state, EntityState lastState)
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

        private void OnStartExploding(EntityState state, EntityState lastState)
        {
            Velocity = Vector.NULL_VECTOR;
        }

        public override void Reflect()
        {
            Engine.PlaySound(1, "Enemy Helmet Hit");
            SetState(ChargedState.EXPLODING);
        }

        protected internal override void OnHit(Enemy enemy, FixedSingle damage)
        {
            if (!enemy.Broke && enemy.Health > damage)
            {
                Damage = 0;
                hitEntity = enemy;
                SetState(ChargedState.HITTING);
            }
        }

        protected override void OnDeath()
        {
            Shooter.shots--;
            Shooter.shootingCharged = false;

            base.OnDeath();
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
