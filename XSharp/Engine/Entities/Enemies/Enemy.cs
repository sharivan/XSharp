using MMX.Geometry;
using MMX.Math;

namespace MMX.Engine.Entities.Enemies
{
    public abstract class Enemy : Sprite
    {
        public FixedSingle ContactDamage
        {
            get;
            set;
        }

        protected Enemy(GameEngine engine, string name, Vector origin, int spriteSheetIndex) : base(engine, name, origin, spriteSheetIndex, true)
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
        }

        protected override void Think()
        {
            base.Think();

            if (Offscreen)
                KillOnNextFrame();
        }
    }
}
