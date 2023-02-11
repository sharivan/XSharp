using SharpDX;
using XSharp.Geometry;
using XSharp.Math;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin
{
    public class PenguinFrozenBlock : Sprite
    {
        public Penguin Attacker
        {
            get;
            internal set;
        }

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
            Layer = 1;
            Directional = true;
            SpriteSheetIndex = 10;
            CollisionData = CollisionData.SOLID;

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

            DetatchFromPlayer();

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
            player.ForcedStateException = PlayerState.TAKING_DAMAGE;
            player.ForcedState = PlayerState.STAND;
            player.Velocity = Vector.NULL_VECTOR;
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

        private void DetatchFromPlayer()
        {
            var player = Engine.Player;
            if (Parent == player)
            {
                player.ForcedStateException = PlayerState.NONE;
                player.ForcedState = PlayerState.NONE;
                player.InputLocked = Attacker.Exploding;
                player.TakeDamageEvent -= OnPlayerTakedamage;

                Parent = null;
            }
        }

        protected override void OnDeath()
        {
            DetatchFromPlayer();

            base.OnDeath();
        }

        protected override void Think()
        {
            base.Think();

            if ((Engine.Player.Keys.HasActionOrMovement()
                || Engine.Player.LastKeys.HasActionOrMovement())
                && Engine.Player.Keys != Engine.Player.LastKeys)
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