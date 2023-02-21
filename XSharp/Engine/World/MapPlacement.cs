using XSharp.Math.Geometry;
using static XSharp.Engine.Consts;

namespace XSharp.Engine.World
{
    public readonly struct MapPlacement
    {
        private readonly Map map;

        public World World
        {
            get;
        }

        public Cell Cell
        {
            get;
        }

        public int Row => Cell.Row;

        public int Col => Cell.Col;

        public Vector LeftTop => new(Cell.Col * MAP_SIZE, Cell.Row * MAP_SIZE);

        public Box BoudingBox => World.GetMapBoundingBox(Cell);

        public RightTriangle SlopeTriangle => CollisionData.MakeSlopeTriangle() + LeftTop;

        public CollisionData CollisionData => map != null ? map.CollisionData : CollisionData.NONE;

        internal MapPlacement(World world, int row, int col, Map map)
            : this(world, new Cell(row, col), map)
        {
        }

        internal MapPlacement(World world, Cell cell, Map map)
        {
            World = world;
            Cell = cell;
            this.map = map;
        }
    }
}