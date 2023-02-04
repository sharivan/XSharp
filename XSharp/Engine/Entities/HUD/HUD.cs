using MMX.Geometry;

namespace MMX.Engine.Entities.HUD
{
    public class HUD : Sprite
    {
        public Vector Offset
        {
            get; set;
        }

        public HUD(string name, Vector offset, int spriteSheetIndex, string[] animationNames, string initialAnimationName)
            : base(name, GameEngine.Engine.World.Camera.LeftTop + offset, spriteSheetIndex, animationNames, initialAnimationName, false)
        {
            Offset = offset;
        }

        public HUD(string name, Vector offset, int spriteSheetIndex, params string[] animationNames)
            : base(name, GameEngine.Engine.World.Camera.LeftTop + offset, spriteSheetIndex, false, animationNames)
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

        protected override void UpdatePartition(bool force = false)
        {
        }
    }
}