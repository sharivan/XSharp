using XSharp.Geometry;
using XSharp.Math;

namespace XSharp.Engine.Entities.Effects
{
    public abstract class SpriteEffect : Sprite
    {
        protected SpriteEffect()
        {
        }

        public override FixedSingle GetGravity()
        {
            return FixedSingle.ZERO;
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            CheckCollisionWithWorld = false;
            CheckTouchingEntities = false;
        }
    }
}