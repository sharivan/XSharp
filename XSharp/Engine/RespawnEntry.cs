using MMX.Geometry;

namespace MMX.Engine
{
    internal class RespawnEntry
    {
        public RespawnEntry(Entity entity, Box box)
        {
            Entity = entity;
            Box = box;
        }

        public Entity Entity { get; }

        public Box Box
        {
            get;
        }
    }
}
