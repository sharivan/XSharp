using MMX.Geometry;
using MMX.Math;

using static MMX.Engine.Consts;

namespace MMX.Engine.Entities.Enemies
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

        public float LifeDropOdd
        {
            get;
            set;
        }

        public float NothingDropOdd
        {
            get;
            set;
        }

        public float TotalDropOdd => SmallHealthDropOdd + BigHealthDropOdd + SmallAmmoDropOdd + BigAmmoDropOdd + LifeDropOdd + NothingDropOdd;

        protected Enemy(GameEngine engine, string name, Vector origin, int spriteSheetIndex, bool directional = false) : base(engine, name, origin, spriteSheetIndex, directional)
        {
        }

        protected override Box GetCollisionBox()
        {
            Animation animation = CurrentAnimation;
            return animation != null ? animation.CurrentFrameCollisionBox : Box.EMPTY_BOX;
        }

        protected override void OnTouching(Entity entity)
        {
            if (entity is Player player)
                Hurt(player, ContactDamage);

            base.OnTouching(entity);
        }

        protected override void OnTakeDamagePost(Sprite attacker, FixedSingle damage)
        {
            Engine.PlaySound(2, 8);
            base.OnTakeDamagePost(attacker, damage);
        }

        protected override void OnBroke()
        {
            Engine.CreateExplosionEffect(HitBox.Center);

            int random = Engine.RNG.Next(0, (int) System.Math.Ceiling(TotalDropOdd));
            if (random <= SmallHealthDropOdd)
                Engine.DropSmallHealthRecover(Origin, ITEM_DURATION_FRAMES);
        }

        protected override void Think()
        {
            base.Think();

            if (Offscreen)
                KillOnNextFrame();
        }
    }
}
