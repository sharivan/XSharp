using SharpDX;

using XSharp.Engine.Collision;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

using MMXBox = XSharp.Math.Geometry.Box;

namespace XSharp.Engine.World;

public class Map
{
    public static Cell GetTileCellFromPos(Vector pos)
    {
        int col = (int) (pos.X / TILE_SIZE);
        int row = (int) (pos.Y / TILE_SIZE);

        return new Cell(row, col);
    }

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
            {
                for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
                {
                    Tile tile = tiles[row, col];
                    if (tile != null)
                        return false;
                }
            }

            return true;
        }
    }

    internal Map(World world, int id, CollisionData collisionData = CollisionData.NONE)
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

    public Tile GetTileFrom(Vector pos)
    {
        Cell tsp = GetTileCellFromPos(pos);
        int row = tsp.Row;
        int col = tsp.Col;

        return row < 0 || col < 0 || row >= SIDE_TILES_PER_MAP || col >= SIDE_TILES_PER_MAP ? null : tiles[row, col];
    }

    public void RemoveTile(Tile tile)
    {
        if (tile == null)
            return;

        for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
        {
            for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
            {
                if (tiles[row, col] == tile)
                {
                    tiles[row, col] = null;
                    palette[row, col] = -1;
                    flipped[row, col] = false;
                    mirrored[row, col] = false;
                    upLayer[row, col] = false;
                }
            }
        }
    }

    public Tile GetTile(Vector pos)
    {
        Cell cell = GetTileCellFromPos(pos);
        return tiles[cell.Row, cell.Col];
    }

    public void SetTile(Vector pos, Tile tile, int palette = -1, bool flipped = false, bool mirrored = false, bool upLayer = false)
    {
        Cell cell = GetTileCellFromPos(pos);
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
        {
            for (int r = 0; r < rows; r++)
                SetTile(new Vector((col + c) * TILE_SIZE, (row + r) * TILE_SIZE), tile, palette, flipped, mirrored, upLayer);
        }
    }

    internal void Tessellate(DataStream downLayerVBData, DataStream upLayerVBData, Vector pos)
    {
        for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
        {
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