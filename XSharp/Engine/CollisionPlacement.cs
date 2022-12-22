using MMX.Engine.World;
using MMXWorld = MMX.Engine.World.World;

namespace MMX.Engine
{
    public readonly struct CollisionPlacement
    {
        public CollisionFlags Flag { get; }

        public MapPlacement Placement { get; }

        internal CollisionPlacement(CollisionFlags flag, MapPlacement placement)
        {
            this.Flag = flag;
            this.Placement = placement;
        }

        internal CollisionPlacement(MMXWorld world, CollisionFlags flag, Cell cell, Map map)
        {
            this.Flag = flag;
            Placement = new MapPlacement(world, cell, map);
        }

        internal CollisionPlacement(MMXWorld world, CollisionFlags flag, int row, int col, Map map)
        {
            this.Flag = flag;
            Placement = new MapPlacement(world, row, col, map);
        }
    }
}
