using MMX.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static MMX.Engine.Consts;

namespace MMX.Engine.World
{
    public struct MapPlacement
    {
        private World world;
        private Cell cell;
        private Map map;

        public World World
        {
            get
            {
                return world;
            }
        }

        public Cell Cell
        {
            get
            {
                return cell;
            }
        }

        public int Row
        {
            get
            {
                return cell.Row;
            }
        }

        public int Col
        {
            get
            {
                return cell.Col;
            }
        }

        public Vector LeftTop
        {
            get
            {
                return new Vector(cell.Col * MAP_SIZE, cell.Row * MAP_SIZE);
            }
        }

        public Box BoudingBox
        {
            get
            {
                return World.GetMapBoundingBox(cell);
            }
        }

        public RightTriangle SlopeTriangle
        {
            get
            {
                return World.MakeSlopeTriangle(CollisionData) + LeftTop;
            }
        }

        public CollisionData CollisionData
        {
            get
            {
                return map != null ? map.CollisionData : CollisionData.NONE;
            }
        }

        internal MapPlacement(World world, int row, int col, Map map) :
            this(world, new Cell(row, col), map)
        {
        }

        internal MapPlacement(World world, Cell cell, Map map)
        {
            this.world = world;
            this.cell = cell;
            this.map = map;
        }
    }
}
