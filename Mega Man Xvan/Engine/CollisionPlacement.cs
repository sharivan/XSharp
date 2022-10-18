using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MMX.Engine.World;
using MMXWorld = MMX.Engine.World.World;

namespace MMX.Engine
{
    public struct CollisionPlacement
    {
        private CollisionFlags flag;
        private MapPlacement placement;

        public CollisionFlags Flag
        {
            get
            {
                return flag;
            }
        }

        public MapPlacement Placement
        {
            get
            {
                return placement;
            }
        }

        internal CollisionPlacement(CollisionFlags flag, MapPlacement placement)
        {
            this.flag = flag;
            this.placement = placement;
        }

        internal CollisionPlacement(MMXWorld world, CollisionFlags flag, Cell cell, Map map)
        {
            this.flag = flag;
            placement = new MapPlacement(world, cell, map);
        }

        internal CollisionPlacement(MMXWorld world, CollisionFlags flag, int row, int col, Map map)
        {
            this.flag = flag;
            placement = new MapPlacement(world, row, col, map);
        }
    }
}
