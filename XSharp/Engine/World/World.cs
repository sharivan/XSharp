using System;
using System.Collections.Generic;

using SharpDX.Direct3D9;

using XSharp.Engine.Collision;
using XSharp.Engine.Entities;
using XSharp.Math.Geometry;
using XSharp.Util;

using static XSharp.Engine.Consts;

using Box = XSharp.Math.Geometry.Box;

namespace XSharp.Engine.World;

public class World : IDisposable
{
    public const int TILEMAP_WIDTH = 32 * MAP_SIZE;
    public const int TILEMAP_HEIGHT = 32 * MAP_SIZE;

    public const float TILE_FRAC_SIZE = 1f / 64;

    public static readonly Vector TILE_SIZE_VECTOR = new(TILE_SIZE, TILE_SIZE);
    public static readonly Vector TILE_FRAC_SIZE_VECTOR = new(TILE_FRAC_SIZE, TILE_FRAC_SIZE);

    public static BaseEngine Engine => BaseEngine.Engine;

    public Device Device => BaseEngine.Engine.Device;

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

    private PixelCollisionChecker collisionChecker;

    public ForegroundLayout ForegroundLayout
    {
        get;
    }

    public BackgroundLayout BackgroundLayout
    {
        get;
    }

    internal World(int sceneRowCount, int sceneColCount)
        : this(sceneRowCount, sceneColCount, sceneRowCount, sceneColCount)
    {
    }

    internal World(int foregroundSceneRowCount, int foregroundSceneColCount, int backgroundSceneRowCount, int backgroundSceneColCount)
    {
        ForegroundLayout = new ForegroundLayout(foregroundSceneRowCount, foregroundSceneColCount);
        BackgroundLayout = new BackgroundLayout(backgroundSceneRowCount, backgroundSceneColCount);

        collisionChecker = new PixelCollisionChecker();
    }

    public void Clear()
    {
        ForegroundLayout.Clear();
        BackgroundLayout.Clear();
    }

    public void Dispose()
    {
        try
        {
            Clear();
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }

    public void RenderBackground(int layer)
    {
        BackgroundLayout.Render(layer);
    }

    public void RenderForeground(int layer)
    {
        ForegroundLayout.Render(layer);
    }

    internal void OnFrame()
    {
        ForegroundLayout.OnFrame();
        BackgroundLayout.OnFrame();
    }

    internal void Tessellate()
    {
        ForegroundLayout.Tesselate();
        BackgroundLayout.Tesselate();
    }

    internal void OnDisposeDevice()
    {
        ForegroundLayout.OnDisposeDevice();
        BackgroundLayout.OnDisposeDevice();
    }

    public CollisionFlags GetCollisionFlags(Box collisionBox, CollisionFlags ignore = CollisionFlags.NONE, bool checkWithWorld = true, bool checknWithSolidSprites = false, params Entities.Sprite[] ignoreSprites)
    {
        collisionChecker.Setup(collisionBox, ignore, checkWithWorld, checknWithSolidSprites, false, ignoreSprites);
        return collisionChecker.GetCollisionFlags();
    }

    public CollisionFlags GetCollisionFlags(Box collisionBox, EntitySet<Entities.Sprite> ignoreSprites, CollisionFlags ignore = CollisionFlags.NONE, bool checkWithWorld = true, bool checknWithSolidSprites = false)
    {
        collisionChecker.Setup(collisionBox, ignore, ignoreSprites, checkWithWorld, checknWithSolidSprites, false);
        return collisionChecker.GetCollisionFlags();
    }

    public CollisionFlags GetCollisionFlags(Box collisionBox, BitSet ignoreSprites, CollisionFlags ignore = CollisionFlags.NONE, bool checkWithWorld = true, bool checknWithSolidSprites = false)
    {
        collisionChecker.Setup(collisionBox, ignore, ignoreSprites, checkWithWorld, checknWithSolidSprites, false);
        return collisionChecker.GetCollisionFlags();
    }

    public IEnumerable<CollisionPlacement> GetCollisionPlacements(Box collisionBox, CollisionFlags ignore = CollisionFlags.NONE, bool checkWithWorld = true, bool checknWithSolidSprites = false, params Entities.Sprite[] ignoreSprites)
    {
        collisionChecker.Setup(collisionBox, ignore, checkWithWorld, checknWithSolidSprites, true, ignoreSprites);
        collisionChecker.GetCollisionFlags();
        return collisionChecker.Placements;
    }

    public IEnumerable<CollisionPlacement> GetCollisionPlacements(Box collisionBox, EntitySet<Entities.Sprite> ignoreSprites, CollisionFlags ignore = CollisionFlags.NONE, bool checkWithWorld = true, bool checknWithSolidSprites = false)
    {
        collisionChecker.Setup(collisionBox, ignore, ignoreSprites, checkWithWorld, checknWithSolidSprites, true);
        collisionChecker.GetCollisionFlags();
        return collisionChecker.Placements;
    }

    public IEnumerable<CollisionPlacement> GetCollisionPlacements(Box collisionBox, BitSet ignoreSprites, CollisionFlags ignore = CollisionFlags.NONE, bool checkWithWorld = true, bool checknWithSolidSprites = false)
    {
        collisionChecker.Setup(collisionBox, ignore, ignoreSprites, checkWithWorld, checknWithSolidSprites, true);
        collisionChecker.GetCollisionFlags();
        return collisionChecker.Placements;
    }
}