using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        internal CollisionPlacement(World world, CollisionFlags flag, Cell cell, Map map)
        {
            this.flag = flag;
            placement = new MapPlacement(world, cell, map);
        }

        internal CollisionPlacement(World world, CollisionFlags flag, int row, int col, Map map)
        {
            this.flag = flag;
            placement = new MapPlacement(world, row, col, map);
        }
    }
}
