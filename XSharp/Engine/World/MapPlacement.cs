using MMX.Geometry;

using static MMX.Engine.Consts;

namespace MMX.Engine.World
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

        public RightTriangle SlopeTriangle => World.MakeSlopeTriangle(CollisionData) + LeftTop;

        public CollisionData CollisionData => map != null ? map.CollisionData : CollisionData.BACKGROUND;

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