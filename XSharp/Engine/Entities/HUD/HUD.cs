using XSharp.Math.Geometry;

namespace XSharp.Engine.Entities.HUD
{
    public abstract class HUD : Sprite
    {
        public Vector Offset
        {
            get;
            set;
        }

        protected HUD()
        {
            Directional = false;
        }

        protected internal override void OnSpawn()
        {
            base.OnSpawn();

            CheckTouchingEntities = false;
            CheckCollisionWithWorld = false;
            Static = true;
        }

        protected internal virtual void UpdateOrigin()
        {
            Origin = Engine.World.Camera.LeftTop + Offset;
        }

        protected internal override void UpdatePartition(bool force = false)
        {
        }
    }
}