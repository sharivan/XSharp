using MMX.Geometry;
using MMX.Math;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using System;

using static MMX.Engine.Consts;

namespace MMX.Engine
{
    public class Map
    {
        private World world;
        private int id;
        private CollisionData collisionData;

        internal Tile[,] tiles;
        internal bool[,] flipped;
        internal bool[,] mirrored;
        internal bool[,] upLayer;

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

        public CollisionData CollisionData
        {
            get
            {
                return collisionData;
            }

            set
            {
                collisionData = value;
            }
        }

        public Tile this[int row, int col]
        {
            get
            {
                return tiles[row, col];
            }

            set
            {
                tiles[row, col] = value;
            }
        }

        internal Map(World world, int id, CollisionData collisionData = CollisionData.NONE)
        {
            this.world = world;
            this.id = id;
            this.collisionData = collisionData;

            tiles = new Tile[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
            flipped = new bool[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
            mirrored = new bool[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
            upLayer = new bool[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
        }

        internal Map(World world, int id, Color color, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false) :
            this(world, id, collisionData)
        {
            for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
                for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
                {
                    Tile tile = world.AddTile(color);
                    tiles[row, col] = tile;
                    this.flipped[row, col] = flipped;
                    this.mirrored[row, col] = mirrored;
                    this.upLayer[row, col] = upLayer;
                }
        }

        /*internal Map(World world, int id, Color[,] pixels, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false) :
            this(world, id, pixels, Point.Zero, collisionData, flipped, mirrored, upLayer)
        {
        }*/

        /*internal Map(World world, int id, Color[,] pixels, Point offset, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false) :
            this(world, id, collisionData)
        {
            for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
                for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
                {
                    Tile tile = world.AddTile(pixels, new Point(offset.X + col * TILE_SIZE, offset.Y + row * TILE_SIZE));
                    tiles[row, col] = tile;
                    this.flipped[row, col] = flipped;
                    this.mirrored[row, col] = mirrored;
                    this.upLayer[row, col] = upLayer;
                }
        }*/

        internal Map(World world, int id, Bitmap source, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false) :
            this(world, id, source, Point.Zero, collisionData, flipped, mirrored, upLayer)
        {
        }

        internal Map(World world, int id, Bitmap source, Point offset, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false) :
            this(world, id, collisionData)
        {
            for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
                for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
                {
                    Tile tile = world.AddTile(source, new Point(offset.X + col * TILE_SIZE, offset.Y + row * TILE_SIZE));
                    tiles[row, col] = tile;
                    this.flipped[row, col] = flipped;
                    this.mirrored[row, col] = mirrored;
                    this.upLayer[row, col] = upLayer;
                }
        }

        private void PaintTile(RenderTarget target, Tile tile, FixedSingle x, FixedSingle y, bool flipped, bool mirrored)
        {
            Vector2 center = new Vector2((float) x + TILE_SIZE / 2, (float) y + TILE_SIZE / 2);
            RectangleF dst = new RectangleF((float) x, (float) y, TILE_SIZE, TILE_SIZE);
            RectangleF src = new RectangleF(0, 0, TILE_SIZE, TILE_SIZE);

            var lastTransform = target.Transform;

            if (flipped)
            {
                if (mirrored)
                    target.Transform *= Matrix3x2.Scaling(-1, -1, center);
                else
                    target.Transform *= Matrix3x2.Scaling(1, -1, center);
            
            }
            else if (mirrored)
                target.Transform *= Matrix3x2.Scaling(-1, 1, center);

            target.DrawBitmap(tile.target.Bitmap, dst, 1, INTERPOLATION_MODE, src);

            //if (flipped || mirrored)
            //{
                target.Flush();
                target.Transform = lastTransform;
            //}
        }

        internal void PaintDownLayer(RenderTarget target, Vector offset)
        {
            for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
                for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
                {
                    Tile tile = tiles[row, col];
                    if (tile != null && !upLayer[row, col])
                        PaintTile(target, tile, offset.X + col * TILE_SIZE, offset.Y + row * TILE_SIZE, flipped[row, col], mirrored[row, col]);
                }
        }

        internal void PaintUpLayer(RenderTarget target, Vector offset)
        {
            for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
                for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
                {
                    Tile tile = tiles[row, col];
                    if (tile != null && upLayer[row, col])
                        PaintTile(target, tile, offset.X + col * TILE_SIZE, offset.Y + row * TILE_SIZE, flipped[row, col], mirrored[row, col]);
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
        }

        public Tile GetTileFrom(Vector pos)
        {
            Cell tsp = World.GetTileCellFromPos(pos);
            int row = tsp.Row;
            int col = tsp.Col;

            if (row < 0 || col < 0 || row >= SIDE_TILES_PER_MAP || col >= SIDE_TILES_PER_MAP)
                return null;

            return tiles[row, col];
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

        public void SetTile(Vector pos, Tile tile, bool flipped = false, bool mirrored = false, bool upLayer = false)
        {
            Cell cell = World.GetTileCellFromPos(pos);
            SetTile(cell, tile, flipped, mirrored, upLayer);
        }

        public void SetTile(int row, int col, Tile tile, bool flipped = false, bool mirrored = false, bool upLayer = false)
        {
            SetTile(new Cell(row, col), tile, flipped, mirrored, upLayer);
        }

        public void SetTile(Cell cell, Tile tile, bool flipped = false, bool mirrored = false, bool upLayer = false)
        {
            tiles[cell.Row, cell.Col] = tile;
            this.flipped[cell.Row, cell.Col] = flipped;
            this.mirrored[cell.Row, cell.Col] = mirrored;
            this.upLayer[cell.Row, cell.Col] = upLayer;
        }

        public void Fill(Bitmap source, Point offset, bool flipped = false, bool mirrored = false, bool upLayer = false)
        {
            for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
                for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
                {
                    Tile tile = world.AddTile(source, new Point(offset.X + col * TILE_SIZE, offset.Y + row * TILE_SIZE));
                    tiles[row, col] = tile;
                    this.flipped[row, col] = flipped;
                    this.mirrored[row, col] = mirrored;
                    this.upLayer[row, col] = upLayer;
                }
        }

        public void FillRectangle(Box box, Tile tile, bool flipped = false, bool mirrored = false, bool upLayer = false)
        {
            Vector boxLT = box.LeftTop;
            Vector boxSize = box.DiagonalVector;

            int col = (int) (boxLT.X / TILE_SIZE);
            int row = (int) (boxLT.Y / TILE_SIZE);
            int cols = (int) (boxSize.X / TILE_SIZE);
            int rows = (int) (boxSize.Y / TILE_SIZE);

            for (int c = 0; c < cols; c++)
                for (int r = 0; r < rows; r++)
                    SetTile(new Vector((col + c) * TILE_SIZE, (row + r) * TILE_SIZE), tile, flipped, mirrored, upLayer);
        }
    }
}
