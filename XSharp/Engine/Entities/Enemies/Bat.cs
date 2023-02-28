﻿using XSharp.Math;
using XSharp.Math.Geometry;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies
{
    public enum BatState
    {
        IDLE = 0,
        ATTACKING = 1,
        ESCAPING = 2
    }

    public class Bat : Enemy
    {
        private bool flashing;

        public BatState State
        {
            get => GetState<BatState>();
            set
            {
                CheckCollisionWithWorld = value == BatState.ESCAPING;
                SetState(value);
            }
        }

        public Bat()
        {
            SpriteSheetIndex = 8;

            SetAnimationNames("Idle", "Attacking");

            SetupStateArray(typeof(BatState));
            RegisterState(BatState.IDLE, OnIdle, "Idle");
            RegisterState(BatState.ATTACKING, OnAttacking, "Attacking");
            RegisterState(BatState.ESCAPING, OnEscaping, "Attacking");
        }

        public override FixedSingle GetGravity()
        {
            return 0;
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            CheckCollisionWithWorld = false;

            flashing = false;

            PaletteIndex = 6;
            Health = BAT_HEALTH;
            ContactDamage = BAT_CONTACT_DAMAGE;

            NothingDropOdd = 79;
            SmallHealthDropOdd = 5;
            BigHealthDropOdd = 5;
            SmallAmmoDropOdd = 5;
            BigAmmoDropOdd = 5;
            LifeUpDropOdd = 1;

            State = BatState.IDLE;
        }

        protected override bool OnTakeDamage(Sprite attacker, ref FixedSingle damage)
        {
            flashing = true;
            PaletteIndex = 4;

            return base.OnTakeDamage(attacker, ref damage);
        }

        protected override void OnHurt(Sprite victim, FixedSingle damage)
        {
            Velocity = BAT_ESCAPE_SPEED * Vector.UP_VECTOR;
            State = BatState.ESCAPING;
        }

        protected override void OnStopMoving()
        {
            base.OnStopMoving();

            if (State == BatState.ESCAPING)
            {
                Velocity = Vector.NULL_VECTOR;
                State = BatState.IDLE;
            }
        }

        private void OnIdle(EntityState state, long frameCounter)
        {
            if (frameCounter >= 60 && Origin.DistanceTo(Engine.Player.Origin) <= SCENE_SIZE * 0.5)
                State = BatState.ATTACKING;
            else
                Velocity = Vector.NULL_VECTOR;
        }

        private void OnAttacking(EntityState state, long frameCounter)
        {
            Vector delta = Engine.Player.Origin - Origin;
            Velocity = BAT_ATTACK_SPEED * delta.Versor();
        }

        private void OnEscaping(EntityState state, long frameCounter)
        {
            if (BlockedUp)
            {
                Velocity = Vector.NULL_VECTOR;
                State = BatState.IDLE;
            }
            else
                Velocity = BAT_ESCAPE_SPEED * Vector.UP_VECTOR;
        }

        protected override bool PreThink()
        {
            if (flashing)
            {
                flashing = false;
                PaletteIndex = 6;
            }

            return base.PreThink();
        }
    }
}