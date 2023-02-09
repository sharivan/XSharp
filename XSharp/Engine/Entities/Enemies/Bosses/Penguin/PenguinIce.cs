using XSharp.Geometry;
using XSharp.Math;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin
{
    public class PenguinIce : Enemy
    {
        private FixedSingle speed;
        private bool bumped;

        public Penguin Shooter
        {
            get;
            internal set;
        }

        public bool Bump
        {
            get;
            internal set;
        } = false;

        public PenguinIce()
        {
            Directional = true;
            SpriteSheetIndex = 10;
            ContactDamage = 2;

            SetAnimationNames("Ice");
        }

        protected override Box GetHitbox()
        {
            return PENGUIN_ICE_HITBOX;
        }

        public void Explode()
        {
            Engine.PlaySound(4, 30);

            var fragment = new PenguinIceExplosionEffect()
            {
                Origin = Origin,
                InitialVelocity = (-PENGUIN_ICE_SPEED, -PENGUIN_ICE_SPEED)
            };

            fragment.Spawn();

            fragment = new PenguinIceExplosionEffect()
            {
                Origin = Origin,
                InitialVelocity = (PENGUIN_ICE_SPEED, -PENGUIN_ICE_SPEED)
            };

            fragment.Spawn();

            fragment = new PenguinIceExplosionEffect()
            {
                Origin = Origin,
                InitialVelocity = (-PENGUIN_ICE_SPEED, -PENGUIN_ICE_SPEED * FixedSingle.HALF)
            };

            fragment.Spawn();

            fragment = new PenguinIceExplosionEffect()
            {
                Origin = Origin,
                InitialVelocity = (PENGUIN_ICE_SPEED, -PENGUIN_ICE_SPEED * FixedSingle.HALF)
            };

            fragment.Spawn();

            Kill();
        }

        public override FixedSingle GetGravity()
        {
            return Bump ? base.GetGravity() : 0;
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            Direction = Shooter.Direction;
            Origin = (Shooter.Direction == Shooter.DefaultDirection ? Shooter.Hitbox.Left - PENGUIN_ICE_SHOT_ORIGIN_OFFSET.X : Shooter.Hitbox.Right + PENGUIN_ICE_SHOT_ORIGIN_OFFSET.X, Shooter.Hitbox.Top + PENGUIN_ICE_SHOT_ORIGIN_OFFSET.Y);
            ReflectShots = true;

            bumped = false;
            speed = Bump ? PENGUIN_ICE_SPEED2_X : PENGUIN_ICE_SPEED;

            SetCurrentAnimationByName("Ice");
        }

        protected override void OnLanded()
        {
            base.OnLanded();

            if (!bumped)
            {
                Velocity = (Velocity.X, -PENGUIN_ICE_SPEED2_X);
                bumped = true;
            }
            else
                Velocity = Velocity.XVector;
        }

        protected override void OnBlockedLeft()
        {
            base.OnBlockedLeft();

            Explode();
        }

        protected override void OnBlockedRight()
        {
            base.OnBlockedRight();

            Explode();
        }

        protected override void OnContactDamage(Player player)
        {
            base.OnContactDamage(player);

            Explode();
        }

        protected override void Think()
        {
            base.Think();

            Velocity = (Direction == Direction.RIGHT ? speed : -speed, Velocity.Y);
        }
    }
}