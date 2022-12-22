using MMX.Geometry;

namespace MMX.Engine
{
    internal class RespawnEntry
    {
        public RespawnEntry(Entity entity, Box box)
        {
            this.Entity = entity;
            this.Box = box;
        }

        public Entity Entity { get; }

        public Box Box
        {
            get;
        }
    }
}
