using MMX.Geometry;

namespace MMX.Engine.Entities.Effects
{
    internal class ExplosionEffect : SpriteEffect
    {
        public ExplosionEffect(string name, Vector origin) : base(name, origin, 5, false, "Explosion")
        {
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            Engine.PlaySound(2, 12);
        }

        protected internal override void OnAnimationEnd(Animation animation)
        {
            base.OnAnimationEnd(animation);
            KillOnNextFrame();
        }
    }
}