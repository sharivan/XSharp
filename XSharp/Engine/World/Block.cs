using SharpDX;

using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

using MMXBox = XSharp.Math.Geometry.Box;

namespace XSharp.Engine.World;

public class Block
{
    public static Cell GetMapCellFromPos(Vector pos)
    {
        int col = (int) (pos.X / MAP_SIZE);
        int row = (int) (pos.Y / MAP_SIZE);

        return new Cell(row, col);
    }

    internal Map[,] maps;

    public int ID
    {
        get;
    }

    public Map this[int row, int col]
    {
        get => maps[row, col];

        set => maps[row, col] = value;
    }

    internal Block(int id)
    {
        ID = id;

        maps = new Map[SIDE_MAPS_PER_BLOCK, SIDE_MAPS_PER_BLOCK];
    }

    public Tile GetTileFrom(Vector pos)
    {
        Cell tsp = GetMapCellFromPos(pos);
        int row = tsp.Row;
        int col = tsp.Col;

        if (row < 0 || col < 0 || row >= SIDE_MAPS_PER_BLOCK || col >= SIDE_MAPS_PER_BLOCK)
            return null;

        Map map = maps[row, col];
        return map?.GetTileFrom(pos - new Vector(col * MAP_SIZE, row * MAP_SIZE));
    }

    public Map GetMapFrom(Vector pos)
    {
        Cell tsp = GetMapCellFromPos(pos);
        int row = tsp.Row;
        int col = tsp.Col;

        return row < 0 || col < 0 || row >= SIDE_MAPS_PER_BLOCK || col >= SIDE_MAPS_PER_BLOCK ? null : maps[row, col];
    }

    public void RemoveMap(Map map)
    {
        if (map == null)
            return;

        for (int col = 0; col < SIDE_MAPS_PER_BLOCK; col++)
        {
            for (int row = 0; row < SIDE_MAPS_PER_BLOCK; row++)
            {
                if (maps[row, col] == map)
                    maps[row, col] = null;
            }
        }
    }

    public void SetTile(Vector pos, Tile tile)
    {
        Cell cell = GetMapCellFromPos(pos);
        Map map = maps[cell.Row, cell.Col];
        if (map == null)
        {
            map = GameEngine.Engine.World.AddMap();
            maps[cell.Row, cell.Col] = map;
        }

        map.SetTile(pos - new Vector(cell.Col * MAP_SIZE, cell.Row * MAP_SIZE), tile);
    }

    public void SetMap(Vector pos, Map map)
    {
        Cell cell = GetMapCellFromPos(pos);
        maps[cell.Row, cell.Col] = map;
    }

    public void FillRectangle(MMXBox box, Tile tile)
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
                SetTile(new Vector((col + c) * TILE_SIZE, (row + r) * TILE_SIZE), tile);
        }
    }

    public void FillRectangle(MMXBox box, Map map)
    {
        Vector boxLT = box.LeftTop;
        Vector boxSize = box.DiagonalVector;

        int col = (int) (boxLT.X / MAP_SIZE);
        int row = (int) (boxLT.Y / MAP_SIZE);
        int cols = (int) (boxSize.X / MAP_SIZE);
        int rows = (int) (boxSize.Y / MAP_SIZE);

        for (int c = 0; c < cols; c++)
        {
            for (int r = 0; r < rows; r++)
                SetMap(new Vector((col + c) * MAP_SIZE, (row + r) * MAP_SIZE), map);
        }
    }

    internal void Tessellate(DataStream downLayerVBData, DataStream upLayerVBData, Vector pos)
    {
        for (int col = 0; col < SIDE_MAPS_PER_BLOCK; col++)
        {
            for (int row = 0; row < SIDE_MAPS_PER_BLOCK; row++)
            {
                var mapPos = new Vector(pos.X + col * MAP_SIZE, pos.Y - row * MAP_SIZE);
                Map map = maps[row, col];

                if (map != null)
                {
                    map.Tessellate(downLayerVBData, upLayerVBData, mapPos);
                }
                else
                {
                    for (int tileRow = 0; tileRow < SIDE_TILES_PER_MAP; tileRow++)
                    {
                        for (int tileCol = 0; tileCol < SIDE_TILES_PER_MAP; tileCol++)
                        {
                            var tilePos = new Vector(mapPos.X + tileCol * TILE_SIZE, mapPos.Y - tileRow * TILE_SIZE);
                            GameEngine.WriteSquare(downLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                            GameEngine.WriteSquare(upLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                        }
                    }
                }
            }
        }
    }
}