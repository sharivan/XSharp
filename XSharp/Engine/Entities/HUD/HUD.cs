using MMX.Engine;
using MMX.Engine.Entities;
using MMX.Geometry;

namespace MMX.Engine.Entities.HUD
{
    public class HUD : Sprite
    {
        public Vector Offset
        {
            get; set;
        }

        public HUD(GameEngine engine, string name, Vector offset, int spriteSheetIndex, string[] animationNames, string initialAnimationName)
            : base(engine, name, engine.World.Camera.LeftTop + offset, spriteSheetIndex, animationNames, initialAnimationName, false)
        {
            Offset = offset;
        }

        public HUD(GameEngine engine, string name, Vector offset, int spriteSheetIndex, params string[] animationNames)
            : base(engine, name, engine.World.Camera.LeftTop + offset, spriteSheetIndex, false, animationNames)
        {
            Offset = offset;
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            CheckCollisionWithEntities = false;
            CheckCollisionWithWorld = false;
            Static = true;
        }

        protected internal virtual void UpdateOrigin()
        {
            Origin = Engine.World.Camera.LeftTop + Offset;
        }
    }
}
