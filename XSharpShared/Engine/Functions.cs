using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSharp.Engine.World;
using XSharp.Math.Fixed.Geometry;
using static XSharp.Engine.Consts;

namespace XSharp.Engine;

public class Functions
{
    public static Cell GetTileCellFromPos(Vector pos)
    {
        int col = (int) ((pos.X - WORLD_OFFSET.X) / TILE_SIZE);
        int row = (int) ((pos.Y - WORLD_OFFSET.Y) / TILE_SIZE);

        return new Cell(row, col);
    }

    public static Cell GetMapCellFromPos(Vector pos)
    {
        int col = (int) ((pos.X - WORLD_OFFSET.X) / MAP_SIZE);
        int row = (int) ((pos.Y - WORLD_OFFSET.Y) / MAP_SIZE);

        return new Cell(row, col);
    }

    public static Cell GetBlockCellFromPos(Vector pos)
    {
        int col = (int) ((pos.X - WORLD_OFFSET.X) / BLOCK_SIZE);
        int row = (int) ((pos.Y - WORLD_OFFSET.Y) / BLOCK_SIZE);

        return new Cell(row, col);
    }

    public static Cell GetSceneCellFromPos(Vector pos)
    {
        int col = (int) ((pos.X - WORLD_OFFSET.X) / SCENE_SIZE);
        int row = (int) ((pos.Y - WORLD_OFFSET.Y) / SCENE_SIZE);

        return new Cell(row, col);
    }

    public static Box GetTileBoundingBox(int row, int col)
    {
        return GetTileBoundingBox(new Cell(row, col));
    }

    public static Box GetMapBoundingBox(int row, int col)
    {
        return GetMapBoundingBox(new Cell(row, col));
    }

    public static Box GetBlockBoundingBox(int row, int col)
    {
        return GetBlockBoundingBox(new Cell(row, col));
    }

    public static Box GetSceneBoundingBox(int row, int col)
    {
        return GetSceneBoundingBox(new Cell(row, col));
    }

    public static Box GetTileBoundingBox(Cell pos)
    {
        return (pos.Col * TILE_SIZE + WORLD_OFFSET.X, pos.Row * TILE_SIZE + WORLD_OFFSET.Y, TILE_SIZE, TILE_SIZE);
    }

    public static Box GetMapBoundingBox(Cell pos)
    {
        return (pos.Col * MAP_SIZE + WORLD_OFFSET.X, pos.Row * MAP_SIZE + WORLD_OFFSET.Y, MAP_SIZE, MAP_SIZE);
    }

    public static Box GetBlockBoundingBox(Cell pos)
    {
        return (pos.Col * BLOCK_SIZE + WORLD_OFFSET.X, pos.Row * BLOCK_SIZE + WORLD_OFFSET.Y, BLOCK_SIZE, BLOCK_SIZE);
    }

    public static Box GetSceneBoundingBox(Cell pos)
    {
        return (pos.Col * SCENE_SIZE + WORLD_OFFSET.X, pos.Row * SCENE_SIZE + WORLD_OFFSET.Y, SCENE_SIZE, SCENE_SIZE);
    }

    public static Vector GetTileLeftTop(int row, int col)
    {
        return GetTileLeftTop(new Cell(row, col));
    }

    public static Vector GetMapLeftTop(int row, int col)
    {
        return GetMapLeftTop(new Cell(row, col));
    }

    public static Vector GetBlockLeftTop(int row, int col)
    {
        return GetBlockLeftTop(new Cell(row, col));
    }

    public static Vector GetSceneLeftTop(int row, int col)
    {
        return GetSceneLeftTop(new Cell(row, col));
    }

    public static Vector GetTileLeftTop(Cell pos)
    {
        return (pos.Col * TILE_SIZE + WORLD_OFFSET.X, pos.Row * TILE_SIZE + WORLD_OFFSET.Y);
    }

    public static Vector GetMapLeftTop(Cell pos)
    {
        return (pos.Col * MAP_SIZE + WORLD_OFFSET.X, pos.Row * MAP_SIZE + WORLD_OFFSET.Y);
    }

    public static Vector GetBlockLeftTop(Cell pos)
    {
        return (pos.Col * BLOCK_SIZE + WORLD_OFFSET.X, pos.Row * BLOCK_SIZE + WORLD_OFFSET.Y);
    }

    public static Vector GetSceneLeftTop(Cell pos)
    {
        return (pos.Col * SCENE_SIZE + WORLD_OFFSET.X, pos.Row * SCENE_SIZE + WORLD_OFFSET.Y);
    }

    public static Box GetTileBoundingBoxFromPos(Vector pos)
    {
        return GetTileBoundingBox(GetTileCellFromPos(pos));
    }

    public static Box GetMapBoundingBoxFromPos(Vector pos)
    {
        return GetMapBoundingBox(GetMapCellFromPos(pos));
    }

    public static Box GetBlockBoundingBoxFromPos(Vector pos)
    {
        return GetBlockBoundingBox(GetBlockCellFromPos(pos));
    }

    public static Box GetSceneBoundingBoxFromPos(Vector pos)
    {
        return GetSceneBoundingBox(GetSceneCellFromPos(pos));
    }
}