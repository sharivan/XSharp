using SharpDX;
using XSharp.Geometry;
using XSharp.Math;

using static XSharp.Engine.Consts;

namespace XSharp.Engine.Entities.Enemies
{
    public abstract class Enemy : Sprite
    {
        public FixedSingle ContactDamage
        {
            get;
            set;
        }

        public float SmallHealthDropOdd
        {
            get;
            set;
        }

        public float BigHealthDropOdd
        {
            get;
            set;
        }

        public float SmallAmmoDropOdd
        {
            get;
            set;
        }

        public float BigAmmoDropOdd
        {
            get;
            set;
        }

        public float LifeUpDropOdd
        {
            get;
            set;
        }

        public float NothingDropOdd
        {
            get;
            set;
        }

        public float TotalDropOdd => SmallHealthDropOdd + BigHealthDropOdd + SmallAmmoDropOdd + BigAmmoDropOdd + LifeUpDropOdd + NothingDropOdd;

        protected Enemy()
        {
            Directional = true;
            CanGoOutOfMapBounds = true;
        }

        protected override Box GetCollisionBox()
        {
            Animation animation = CurrentAnimation;
            return animation != null ? animation.CurrentFrameCollisionBox : Box.EMPTY_BOX;
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            KillOnOffscreen = true;
        }

        protected virtual void OnContactDamage(Player player)
        {
            Hurt(player, ContactDamage);
        }

        protected override void OnTouching(Entity entity)
        {
            if (ContactDamage > 0 && entity is Player player)
                OnContactDamage(player);

            base.OnTouching(entity);
        }

        protected virtual void OnDamaged()
        {            
        }

        protected override void OnTakeDamagePost(Sprite attacker, FixedSingle damage)
        {
            OnDamaged();
            base.OnTakeDamagePost(attacker, damage);
        }

        protected override void OnBroke()
        {
            Engine.CreateExplosionEffect(Hitbox.Center);

            double random = Engine.RNG.NextDouble() * TotalDropOdd;
            if (random < SmallHealthDropOdd)
            {
                Engine.DropSmallHealthRecover(Hitbox.Center, ITEM_DURATION_FRAMES);
                return;
            }

            random -= SmallHealthDropOdd;
            if (random < BigHealthDropOdd)
            {
                Engine.DropBigHealthRecover(Hitbox.Center, ITEM_DURATION_FRAMES);
                return;
            }

            random -= BigHealthDropOdd;
            if (random < SmallAmmoDropOdd)
            {
                Engine.DropSmallAmmoRecover(Hitbox.Center, ITEM_DURATION_FRAMES);
                return;
            }

            random -= SmallAmmoDropOdd;
            if (random < BigAmmoDropOdd)
            {
                Engine.DropBigAmmoRecover(Hitbox.Center, ITEM_DURATION_FRAMES);
                return;
            }

            random -= BigAmmoDropOdd;
            if (random < LifeUpDropOdd)
                Engine.DropLifeUp(Hitbox.Center, ITEM_DURATION_FRAMES);
        }
    }
}