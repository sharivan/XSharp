using MMX.Geometry;
using SharpDX;
using static MMX.Engine.Consts;
using MMXBox = MMX.Geometry.Box;

namespace MMX.Engine.World
{
    public class Map
    {
        internal Tile[,] tiles;
        internal int[,] palette;
        internal bool[,] flipped;
        internal bool[,] mirrored;
        internal bool[,] upLayer;

        public World World
        {
            get;
        }

        public int ID
        {
            get;
        }

        public CollisionData CollisionData
        {
            get;
            set;
        }

        public Tile this[int row, int col]
        {
            get => tiles[row, col];
            set => tiles[row, col] = value;
        }

        public bool IsNull
        {
            get
            {
                for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
                    for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
                    {
                        Tile tile = tiles[row, col];
                        if (tile != null)
                            return false;
                    }

                return true;
            }
        }

        internal Map(World world, int id, CollisionData collisionData = CollisionData.BACKGROUND)
        {
            World = world;
            ID = id;
            CollisionData = collisionData;

            tiles = new Tile[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
            palette = new int[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
            flipped = new bool[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
            mirrored = new bool[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
            upLayer = new bool[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
        }

        /*internal void Render(MMXBox box, Vector offset, bool background)
        {
            MMXBox mapBox = new MMXBox(offset.X, offset.Y, MAP_SIZE, MAP_SIZE);
            MMXBox intersection = (box & mapBox) - offset;

            Cell start = World.GetBlockCellFromPos(intersection.LeftTop);
            Cell end = World.GetBlockCellFromPos(intersection.RightBottom);

            for (int col = start.Col; col <= end.Col + 1; col++)
            {
                if (col < 0 || col >= SIDE_TILES_PER_MAP)
                    continue;

                for (int row = start.Row; row <= end.Row + 1; row++)
                {
                    if (row < 0 || row >= SIDE_TILES_PER_MAP)
                        continue;

                    Vector tileLT = new Vector(col * TILE_SIZE, row * TILE_SIZE);
                    Tile tile = tiles[row, col];
                    if (tile != null)
                        PaintTile(tile, offset.X + tileLT.X, offset.Y + tileLT.Y, palette[row, col], flipped[row, col], mirrored[row, col], upLayer[row, col], background);
                }
            }

            if (collisionData != CollisionData.NONE)
            {
                if (DEBUG_DRAW_COLLISION_DATA)
                {
                    using (TextFormat format = new TextFormat(world.Engine.DWFactory, "Arial", FontWeight.Bold, FontStyle.Normal, 10))
                    {
                        using (Brush brush = new SolidColorBrush(target, Color.Blue))
                        {
                            target.DrawText(((int) collisionData).ToString("X2"), format, new RectangleF((float) offset.X, (float) offset.Y, MAP_SIZE, MAP_SIZE), brush);
                        }
                    }
                }

                if (DEBUG_DRAW_MAP_BOUNDS)
                {
                    const int TILE_BOUND_STRIKE_WIDTH = 1;

                    using (Brush brush = new SolidColorBrush(target, Color.Red))
                    {
                        if (World.IsSolidBlock(collisionData))
                            target.DrawRectangle(new RectangleF((float) offset.X, (float) offset.Y, MAP_SIZE, MAP_SIZE), brush, TILE_BOUND_STRIKE_WIDTH);
                        else if (World.IsSlope(collisionData))
                        {
                            RightTriangle slopeTriangle = World.MakeSlopeTriangle(collisionData);
                            Vector tv1 = slopeTriangle.Origin;
                            Vector tv2 = slopeTriangle.HCathetusVertex;
                            Vector tv3 = slopeTriangle.VCathetusVertex;

                            FixedSingle h = slopeTriangle.Origin.Y;
                            FixedSingle H = MAP_SIZE - h;
                            if (H > 0)
                            {
                                target.DrawLine(new Vector2((float) (offset.X + tv2.X), (float) (offset.Y + tv2.Y)), new Vector2((float) (offset.X + tv3.X), (float) (offset.Y + tv3.Y)), brush, TILE_BOUND_STRIKE_WIDTH);

                                if (slopeTriangle.HCathetusVector.X < 0)
                                {
                                    target.DrawLine(new Vector2((float) (offset.X + tv2.X), (float) (offset.Y + tv2.Y)), new Vector2((float) (offset.X), (float) (offset.Y + MAP_SIZE)), brush, TILE_BOUND_STRIKE_WIDTH);
                                    target.DrawLine(new Vector2((float) (offset.X + tv3.X), (float) (offset.Y + tv3.Y)), new Vector2((float) (offset.X + MAP_SIZE), (float) (offset.Y + MAP_SIZE)), brush, TILE_BOUND_STRIKE_WIDTH);
                                }
                                else
                                {
                                    target.DrawLine(new Vector2((float) (offset.X + tv3.X), (float) (offset.Y + tv3.Y)), new Vector2((float) (offset.X), (float) (offset.Y + MAP_SIZE)), brush, TILE_BOUND_STRIKE_WIDTH);
                                    target.DrawLine(new Vector2((float) (offset.X + tv2.X), (float) (offset.Y + tv2.Y)), new Vector2((float) (offset.X + MAP_SIZE), (float) (offset.Y + MAP_SIZE)), brush, TILE_BOUND_STRIKE_WIDTH);
                                }

                                target.DrawLine(new Vector2((float) (offset.X), (float) (offset.Y + MAP_SIZE)), new Vector2((float) (offset.X + MAP_SIZE), (float) (offset.Y + MAP_SIZE)), brush, TILE_BOUND_STRIKE_WIDTH);
                            }
                            else
                            {
                                target.DrawLine(new Vector2((float) (offset.X + tv1.X), (float) (offset.Y + tv1.Y)), new Vector2((float) (offset.X + tv2.X), (float) (offset.Y + tv2.Y)), brush, TILE_BOUND_STRIKE_WIDTH);
                                target.DrawLine(new Vector2((float) (offset.X + tv2.X), (float) (offset.Y + tv2.Y)), new Vector2((float) (offset.X + tv3.X), (float) (offset.Y + tv3.Y)), brush, TILE_BOUND_STRIKE_WIDTH);
                                target.DrawLine(new Vector2((float) (offset.X + tv3.X), (float) (offset.Y + tv3.Y)), new Vector2((float) (offset.X + tv1.X), (float) (offset.Y + tv1.Y)), brush, TILE_BOUND_STRIKE_WIDTH);
                            }
                        }
                    }
                }
            }
        }*/

        public Tile GetTileFrom(Vector pos)
        {
            Cell tsp = World.GetTileCellFromPos(pos);
            int row = tsp.Row;
            int col = tsp.Col;

            return row < 0 || col < 0 || row >= SIDE_TILES_PER_MAP || col >= SIDE_TILES_PER_MAP ? null : tiles[row, col];
        }

        public void RemoveTile(Tile tile)
        {
            if (tile == null)
                return;

            for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
                for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
                    if (tiles[row, col] == tile)
                    {
                        tiles[row, col] = null;
                        palette[row, col] = -1;
                        flipped[row, col] = false;
                        mirrored[row, col] = false;
                        upLayer[row, col] = false;
                    }
        }

        public Tile GetTile(Vector pos)
        {
            Cell cell = World.GetTileCellFromPos(pos);
            return tiles[cell.Row, cell.Col];
        }

        public void SetTile(Vector pos, Tile tile, int palette = -1, bool flipped = false, bool mirrored = false, bool upLayer = false)
        {
            Cell cell = World.GetTileCellFromPos(pos);
            SetTile(cell, tile, palette, flipped, mirrored, upLayer);
        }

        public void SetTile(int row, int col, Tile tile, int palette = -1, bool flipped = false, bool mirrored = false, bool upLayer = false)
        {
            SetTile(new Cell(row, col), tile, palette, flipped, mirrored, upLayer);
        }

        public void SetTile(Cell cell, Tile tile, int palette = -1, bool flipped = false, bool mirrored = false, bool upLayer = false)
        {
            tiles[cell.Row, cell.Col] = tile;
            this.palette[cell.Row, cell.Col] = palette;
            this.flipped[cell.Row, cell.Col] = flipped;
            this.mirrored[cell.Row, cell.Col] = mirrored;
            this.upLayer[cell.Row, cell.Col] = upLayer;
        }

        public void FillRectangle(MMXBox box, Tile tile, int palette = -1, bool flipped = false, bool mirrored = false, bool upLayer = false)
        {
            Vector boxLT = box.LeftTop;
            Vector boxSize = box.DiagonalVector;

            int col = (int) (boxLT.X / TILE_SIZE);
            int row = (int) (boxLT.Y / TILE_SIZE);
            int cols = (int) (boxSize.X / TILE_SIZE);
            int rows = (int) (boxSize.Y / TILE_SIZE);

            for (int c = 0; c < cols; c++)
                for (int r = 0; r < rows; r++)
                    SetTile(new Vector((col + c) * TILE_SIZE, (row + r) * TILE_SIZE), tile, palette, flipped, mirrored, upLayer);
        }

        internal void Tessellate(DataStream downLayerVBData, DataStream upLayerVBData, Vector pos)
        {
            for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
                for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
                {
                    var tilePos = new Vector(pos.X + col * TILE_SIZE, pos.Y - row * TILE_SIZE);
                    Tile tile = tiles[row, col];

                    if (tile != null)
                    {
                        var tilemapPos = new Vector((ID % 32 * SIDE_TILES_PER_MAP + col) * World.TILE_FRAC_SIZE, (ID / 32 * SIDE_TILES_PER_MAP + row) * World.TILE_FRAC_SIZE);
                        if (upLayer[row, col])
                        {
                            GameEngine.WriteSquare(downLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                            GameEngine.WriteSquare(upLayerVBData, tilemapPos, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                        }
                        else
                        {
                            GameEngine.WriteSquare(downLayerVBData, tilemapPos, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                            GameEngine.WriteSquare(upLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                        }
                    }
                    else
                    {
                        GameEngine.WriteSquare(downLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                        GameEngine.WriteSquare(upLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                    }
                }
        }
    }
}
