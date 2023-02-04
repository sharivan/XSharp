using XSharp.Engine.World;
using MMXWorld = XSharp.Engine.World.World;

namespace XSharp.Engine
{
    public readonly struct CollisionPlacement
    {
        public CollisionFlags Flag
        {
            get;
        }

        public MapPlacement Placement
        {
            get;
        }

        internal CollisionPlacement(CollisionFlags flag, MapPlacement placement)
        {
            Flag = flag;
            Placement = placement;
        }

        internal CollisionPlacement(MMXWorld world, CollisionFlags flag, Cell cell, Map map)
        {
            Flag = flag;
            Placement = new MapPlacement(world, cell, map);
        }

        internal CollisionPlacement(MMXWorld world, CollisionFlags flag, int row, int col, Map map)
        {
            Flag = flag;
            Placement = new MapPlacement(world, row, col, map);
        }
    }
}
