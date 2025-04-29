using XSharp.Engine.Collision;
using XSharp.Graphics;
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

    internal MapCell[,] cells;

    public int ID
    {
        get;
    }

    public CollisionData CollisionData
    {
        get;
        set;
    }

    public MapCell this[int row, int col] => cells[row, col];

    public bool IsNull
    {
        get
        {
            for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
            {
                for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
                {
                    Tile tile = cells[row, col].Tile;
                    if (tile != null)
                        return false;
                }
            }

            return true;
        }
    }

    internal Map(int id, CollisionData collisionData = CollisionData.NONE, bool fill = false)
    {
        ID = id;
        CollisionData = collisionData;

        cells = new MapCell[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];

        if (fill)
        {
            for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
            {
                for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
                {
                    var cell = new MapCell();
                    cells[row, col] = cell;
                }
            }
        }
    }

    public Tile GetTileFrom(Vector pos)
    {
        Cell tsp = GetTileCellFromPos(pos);
        int row = tsp.Row;
        int col = tsp.Col;

        return row < 0 || col < 0 || row >= SIDE_TILES_PER_MAP || col >= SIDE_TILES_PER_MAP ? null : cells[row, col]?.Tile;
    }

    public void RemoveTile(Tile tile)
    {
        if (tile == null)
            return;

        for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
        {
            for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
            {
                var cell = cells[row, col];
                if (cell != null && cell.Tile == tile)
                    cells[row, col] = null;
            }
        }
    }

    public Tile GetTile(Vector pos)
    {
        Cell cell = GetTileCellFromPos(pos);
        return cells[cell.Row, cell.Col]?.Tile;
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
        var mapCell = cells[cell.Row, cell.Col];
        if (mapCell == null)
        {
            mapCell = new MapCell();
            cells[cell.Row, cell.Col] = mapCell;
        }

        mapCell.Tile = tile;
        mapCell.Palette = palette;
        mapCell.Flipped = flipped;
        mapCell.Mirrored = mirrored;
        mapCell.UpLayer = upLayer;
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
                var cell = cells[row, col];

                if (cell != null)
                {
                    var tilemapPos = new Vector((ID % 32 * SIDE_TILES_PER_MAP + col) * World.TILE_FRAC_SIZE, (ID / 32 * SIDE_TILES_PER_MAP + row) * World.TILE_FRAC_SIZE);
                    if (cell.UpLayer)
                    {
                        BaseEngine.Engine.WriteSquare(downLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                        BaseEngine.Engine.WriteSquare(upLayerVBData, tilemapPos, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                    }
                    else
                    {
                        BaseEngine.Engine.WriteSquare(downLayerVBData, tilemapPos, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                        BaseEngine.Engine.WriteSquare(upLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                    }
                }
                else
                {
                    BaseEngine.Engine.WriteSquare(downLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                    BaseEngine.Engine.WriteSquare(upLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                }
            }
        }
    }
}