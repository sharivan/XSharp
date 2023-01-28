﻿using MMX.Engine.Entities.Enemies;
using MMX.Geometry;
using MMX.Math;
using static MMX.Engine.Consts;

namespace MMX.Engine.Entities.Weapons
{
    public enum SemiChargedState
    {
        FIRING = 0,
        SHOOTING = 1,
        HITTING = 2,
        EXPLODING = 3
    }

    public class BusterSemiCharged : Weapon
    {
        private Entity hitEntity;

        new public Player Shooter => (Player) base.Shooter;

        public bool Firing => GetState<SemiChargedState>() == SemiChargedState.FIRING;

        public bool Exploding => GetState<SemiChargedState>() == SemiChargedState.EXPLODING;

        public bool Hitting => GetState<SemiChargedState>() == SemiChargedState.HITTING;

        internal BusterSemiCharged(GameEngine engine, Player shooter, string name, Vector origin, Direction direction) :
            base(engine, shooter, name, origin, direction, 1)
        {
            SetupStateArray(typeof(SemiChargedState));
            RegisterState(SemiChargedState.FIRING, OnStartFiring, null, null, "SemiChargedShotFiring");
            RegisterState(SemiChargedState.SHOOTING, OnStartShooting, OnShooting, null, "SemiChargedShot");
            RegisterState(SemiChargedState.HITTING, OnStartHitting, null, null, "SemiChargedShotHit");
            RegisterState(SemiChargedState.EXPLODING, OnStartExploding, null, null, "SemiChargedShotExplode");
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

        internal override void OnSpawn()
        {
            base.OnSpawn();

            CheckCollisionWithWorld = false;
            Velocity = Vector.NULL_VECTOR;

            SetState(SemiChargedState.FIRING);
        }

        private void OnStartFiring(EntityState state)
        {
            Engine.PlaySound(1, 1);
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
            Velocity += new Vector(Velocity.X > 0 ? LEMON_ACCELERATION : -LEMON_ACCELERATION, 0);
            if (Velocity.X.Abs > LEMON_TERMINAL_SPEED)
                Velocity = new Vector(Velocity.X > 0 ? LEMON_TERMINAL_SPEED : -LEMON_TERMINAL_SPEED, Velocity.Y);
        }

        private void OnStartHitting(EntityState state)
        {
            if (hitEntity != null)
            {
                Box otherHitbox = hitEntity.HitBox;
                Vector center = HitBox.Center;
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
            SetState(SemiChargedState.EXPLODING);
        }

        public void Hit(Entity entity)
        {
            hitEntity = entity;
            SetState(SemiChargedState.HITTING);
        }

        public override void Dispose()
        {
            Shooter.shots--;
            Shooter.shootingCharged = false;

            base.Dispose();
        }

        protected override void OnStartTouch(Entity entity)
        {
            if (entity is Enemy)
                Hit(entity);

            base.OnStartTouch(entity);
        }

        protected internal override void OnAnimationEnd(Animation animation)
        {
            base.OnAnimationEnd(animation);

            if (animation.FrameSequenceName != CurrentState.AnimationName)
                return;

            if (Firing)
                SetState(SemiChargedState.SHOOTING);
            else if (Hitting || Exploding)
                KillOnNextFrame();
        }
    }
}
