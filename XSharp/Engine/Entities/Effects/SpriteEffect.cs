using MMX.Geometry;
using MMX.Math;

namespace MMX.Engine.Entities.Effects
{
    public class SpriteEffect : Sprite
    {
        public SpriteEffect(GameEngine engine, string name, Vector origin, int spriteSheetIndex, string[] animationNames, string initialAnimationName, bool directional = false)
            : base(engine, name, origin, spriteSheetIndex, animationNames, initialAnimationName, directional)
        {
        }

        public SpriteEffect(GameEngine engine, string name, Vector origin, int spriteSheetIndex, bool directional = false, params string[] animationNames) : this(engine, name, origin, spriteSheetIndex, animationNames, animationNames[0], directional) { }

        public override FixedSingle GetGravity()
        {
            return FixedSingle.ZERO;
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            CheckCollisionWithWorld = false;
        }
    }
}
