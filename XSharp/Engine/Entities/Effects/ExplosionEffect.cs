using XSharp.Geometry;

namespace XSharp.Engine.Entities.Effects
{
    internal class ExplosionEffect : SpriteEffect
    {
        public ExplosionEffect()
        {
            SpriteSheetIndex = 5;
            Directional = false;

            SetAnimationNames("Explosion");
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