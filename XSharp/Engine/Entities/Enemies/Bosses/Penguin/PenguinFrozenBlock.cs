using XSharp.Geometry;
using XSharp.Math;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin
{
    public class PenguinFrozenBlock : Sprite
    {
        public bool Exploding
        {
            get;
            private set;
        }

        public int Hits
        {
            get;
            private set;
        }

        public PenguinFrozenBlock()
        {
            Directional = true;
            SpriteSheetIndex = 10;

            SetAnimationNames("FrozenBlock");
        }

        protected override Box GetHitbox()
        {
            return PENGUIN_FROZEN_BLOCK_HITBOX;
        }

        public override FixedSingle GetGravity()
        {
            return 0;
        }

        public void Explode()
        {
            if (Exploding)
                return;

            Parent = null;
            Engine.Player.InputLocked = false;

            Exploding = true;
            Engine.PlaySound(4, 31);

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

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            var player = Engine.Player;
            player.InputLocked = true;
            player.TakeDamageEvent += OnPlayerTakedamage;
            Origin = player.Origin;
            Parent = player;
            Direction = player.Direction;
            Health = 8;
            Invincible = true;

            Exploding = false;
            Hits = 0;

            SetCurrentAnimationByName("FrozenBlock");
        }

        protected override void OnBroke()
        {
            Explode();
        }

        protected override void OnDeath()
        {
            base.OnDeath();

            Engine.Player.InputLocked = false;
            Engine.Player.TakeDamageEvent -= OnPlayerTakedamage;
        }

        protected override void Think()
        {
            base.Think();

            if (Engine.Player.Keys != Engine.Player.LastKeys)
            {
                Hits++;

                if (Hits >= HITS_TO_BREAK_FROZEN_BLOCK)
                    Break();
            }
        }

        private void OnPlayerTakedamage(Sprite source, Sprite attacker, FixedSingle damage)
        {
            Explode();
        }
    }
}