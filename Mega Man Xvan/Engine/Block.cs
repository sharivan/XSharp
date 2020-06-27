using SharpDX;
using SharpDX.Direct2D1;
using System;

using static MMX.Engine.Consts;

namespace MMX.Engine
{
    public class Block
    {
        private World world;
        private int id;
        internal Map[,] maps;

        public World World
        {
            get
            {
                return world;
            }
        }

        public int ID
        {
            get
            {
                return id;
            }
        }

        public Map this[int row, int col]
        {
            get
            {
                return maps[row, col];
            }

            set
            {
                maps[row, col] = value;
            }
        }

        internal Block(World world, int id)
        {
            this.world = world;
            this.id = id;

            maps = new Map[SIDE_MAPS_PER_BLOCK, SIDE_MAPS_PER_BLOCK];
        }

        public Tile GetTileFrom(MMXVector pos)
        {
            Cell tsp = World.GetMapCellFromPos(pos);
            int row = tsp.Row;
            int col = tsp.Col;

            if (row < 0 || col < 0 || row >= SIDE_MAPS_PER_BLOCK || col >= SIDE_MAPS_PER_BLOCK)
                return null;

            Map map = maps[row, col];
            if (map == null)
                return null;

            return map.GetTileFrom(pos - new MMXVector(col * MAP_SIZE, row * MAP_SIZE));
        }

        public Map GetMapFrom(MMXVector pos)
        {
            Cell tsp = World.GetMapCellFromPos(pos);
            int row = tsp.Row;
            int col = tsp.Col;

            if (row < 0 || col < 0 || row >= SIDE_MAPS_PER_BLOCK || col >= SIDE_MAPS_PER_BLOCK)
                return null;

            return maps[row, col];
        }

        public void RemoveMap(Map map)
        {
            if (map == null)
                return;

            for (int col = 0; col < SIDE_MAPS_PER_BLOCK; col++)
                for (int row = 0; row < SIDE_MAPS_PER_BLOCK; row++)
                    if (maps[row, col] == map)
                        maps[row, col] = null;
        }

        public void SetTile(MMXVector pos, Tile tile)
        {
            Cell cell = World.GetMapCellFromPos(pos);
            Map map = maps[cell.Row, cell.Col];
            if (map == null)
            {
                map = world.AddMap();
                maps[cell.Row, cell.Col] = map;
            }

            map.SetTile(pos - new MMXVector(cell.Col * MAP_SIZE, cell.Row * MAP_SIZE), tile);
        }

        public void SetMap(MMXVector pos, Map map)
        {
            Cell cell = World.GetMapCellFromPos(pos);
            maps[cell.Row, cell.Col] = map;
        }

        public void Fill(Bitmap source, Point offset, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false)
        {
            for (int col = 0; col < SIDE_MAPS_PER_BLOCK; col++)
                for (int row = 0; row < SIDE_MAPS_PER_BLOCK; row++)
                {
                    Map map = world.AddMap(collisionData);
                    map.Fill(source, new Point(offset.X + col * MAP_SIZE, offset.Y + row * MAP_SIZE), flipped, mirrored, upLayer);
                    maps[row, col] = map;
                }
        }

        public void FillRectangle(MMXBox box, Tile tile)
        {
            MMXVector boxLT = box.LeftTop;
            MMXVector boxSize = box.DiagonalVector;

            int col = (int) (boxLT.X / TILE_SIZE);
            int row = (int) (boxLT.Y / TILE_SIZE);
            int cols = (int) (boxSize.X / TILE_SIZE);
            int rows = (int) (boxSize.Y / TILE_SIZE);

            for (int c = 0; c < cols; c++)
                for (int r = 0; r < rows; r++)
                    SetTile(new MMXVector((col + c) * TILE_SIZE, (row + r) * TILE_SIZE), tile);
        }

        public void FillRectangle(MMXBox box, Map map)
        {
            MMXVector boxLT = box.LeftTop;
            MMXVector boxSize = box.DiagonalVector;

            int col = (int) (boxLT.X / MAP_SIZE);
            int row = (int) (boxLT.Y / MAP_SIZE);
            int cols = (int) (boxSize.X / MAP_SIZE);
            int rows = (int) (boxSize.Y / MAP_SIZE);

            for (int c = 0; c < cols; c++)
                for (int r = 0; r < rows; r++)
                    SetMap(new MMXVector((col + c) * MAP_SIZE, (row + r) * MAP_SIZE), map);
        }

        internal void PaintDownLayer(RenderTarget target, MMXVector offset)
        {
            for (int col = 0; col < SIDE_MAPS_PER_BLOCK; col++)
                for (int row = 0; row < SIDE_MAPS_PER_BLOCK; row++)
                {
                    Map map = maps[row, col];
                    if (map != null)
                        map.PaintDownLayer(target, new MMXVector(offset.X + col * MAP_SIZE, offset.Y + row * MAP_SIZE));
                    else                      
                        using (Brush brush = new SolidColorBrush(target, Color.Transparent))
                        {
                            target.FillRectangle(new RectangleF((float) (offset.X + col * MAP_SIZE), (float) (offset.Y + row * MAP_SIZE), MAP_SIZE, MAP_SIZE), brush);
                        }
                }
        }

        internal void PaintUpLayer(RenderTarget target, MMXVector offset)
        {
            for (int col = 0; col < SIDE_MAPS_PER_BLOCK; col++)
                for (int row = 0; row < SIDE_MAPS_PER_BLOCK; row++)
                {
                    Map map = maps[row, col];
                    if (map != null)
                        map.PaintUpLayer(target, new MMXVector(offset.X + col * MAP_SIZE, offset.Y + row * MAP_SIZE));
                    else
                        using (Brush brush = new SolidColorBrush(target, Color.Transparent))
                        {
                            target.FillRectangle(new RectangleF((float) (offset.X + col * MAP_SIZE), (float) (offset.Y + row * MAP_SIZE), MAP_SIZE, MAP_SIZE), brush);
                        }
                }
        }
    }
}
