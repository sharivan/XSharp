using XSharp.Engine.Entities.Effects;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.Enemies.Bosses.Penguin
{
    public class PenguinIceExplosionEffect : SpriteEffect
    {
        public Vector InitialVelocity
        {
            get;
            internal set;
        }

        public PenguinIceExplosionEffect()
        {
            Directional = false;
            SpriteSheetIndex = 10;

            SetAnimationNames("IceFragment");
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            CheckCollisionWithSolidSprites = false;
            CheckCollisionWithWorld = false;
            HasGravity = true;
            Velocity = InitialVelocity;
            KillOnOffscreen = true;
        }
    }
}