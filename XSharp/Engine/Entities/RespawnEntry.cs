using XSharp.Geometry;

namespace XSharp.Engine.Entities
{
    public class RespawnEntry
    {
        public Entity Entity
        {
            get;
            internal set;
        }

        public Vector Origin
        {
            get;
            internal set;
        }

        public RespawnEntry(Entity entity, Vector origin)
        {
            Entity = entity;
            Origin = origin;
        }
    }
}