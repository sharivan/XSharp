using System;
using System.Collections.Generic;

using SharpDX;
using SharpDX.Direct3D9;

using XSharp.Engine.Graphics;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

using Box = XSharp.Math.Geometry.Box;

namespace XSharp.Engine.World;

public class Scene : IDisposable
{
    public static Cell GetBlockCellFromPos(Vector pos)
    {
        int col = (int) (pos.X / BLOCK_SIZE);
        int row = (int) (pos.Y / BLOCK_SIZE);

        return new Cell(row, col);
    }

    public const int TILE_PRIMITIVE_SIZE = 2 * BaseEngine.VERTEX_SIZE * 3;
    public const int MAP_PRIMITIVE_SIZE = SIDE_TILES_PER_MAP * SIDE_TILES_PER_MAP * TILE_PRIMITIVE_SIZE;
    public const int BLOCK_PRIMITIVE_SIZE = SIDE_MAPS_PER_BLOCK * SIDE_MAPS_PER_BLOCK * MAP_PRIMITIVE_SIZE;
    public const int SCENE_PRIMITIVE_SIZE = SIDE_BLOCKS_PER_SCENE * SIDE_BLOCKS_PER_SCENE * BLOCK_PRIMITIVE_SIZE;
    public const int PRIMITIVE_COUNT = SCENE_PRIMITIVE_SIZE / (BaseEngine.VERTEX_SIZE * 3);

    internal Block[,] blocks;
    internal VertexBuffer[] layers;
    internal List<Tilemap> tilemaps;

    public int ID
    {
        get;
    }

    public Block this[int row, int col]
    {
        get => blocks[row, col];
        set => blocks[row, col] = value;
    }

    public bool Tessellated
    {
        get;
        private set;
    }

    internal Scene(int id)
    {
        ID = id;

        blocks = new Block[SIDE_BLOCKS_PER_SCENE, SIDE_BLOCKS_PER_SCENE];
        layers = new VertexBuffer[3];

        tilemaps = new List<Tilemap>();
    }

    public Tile GetTileFrom(Vector pos)
    {
        Cell tsp = GetBlockCellFromPos(pos);
        int row = tsp.Row;
        int col = tsp.Col;

        if (row < 0 || col < 0 || row >= SIDE_BLOCKS_PER_SCENE || col >= SIDE_BLOCKS_PER_SCENE)
            return null;

        Block block = blocks[row, col];
        return block?.GetTileFrom(pos - new Vector(col * BLOCK_SIZE, row * BLOCK_SIZE));
    }

    public Map GetMapFrom(Vector pos)
    {
        Cell tsp = GetBlockCellFromPos(pos);
        int row = tsp.Row;
        int col = tsp.Col;

        if (row < 0 || col < 0 || row >= SIDE_BLOCKS_PER_SCENE || col >= SIDE_BLOCKS_PER_SCENE)
            return null;

        Block block = blocks[row, col];
        return block?.GetMapFrom(pos - new Vector(col * BLOCK_SIZE, row * BLOCK_SIZE));
    }

    public Block GetBlockFrom(Vector pos)
    {
        Cell tsp = GetBlockCellFromPos(pos);
        int row = tsp.Row;
        int col = tsp.Col;

        return row < 0 || col < 0 || row >= SIDE_BLOCKS_PER_SCENE || col >= SIDE_BLOCKS_PER_SCENE ? null : blocks[row, col];
    }

    public void RemoveBlock(Block block)
    {
        if (block == null)
            return;

        List<Cell> positions = new();

        for (int col = 0; col < SIDE_BLOCKS_PER_SCENE; col++)
        {
            for (int row = 0; row < SIDE_BLOCKS_PER_SCENE; row++)
            {
                if (blocks[row, col] == block)
                {
                    positions.Add((row, col));
                    blocks[row, col] = null;
                }
            }
        }

        if (Tessellated)
        {
            foreach (var cell in positions)
                RefreshLayers((cell.Col * BLOCK_SIZE, cell.Row * BLOCK_SIZE, BLOCK_SIZE, BLOCK_SIZE));
        }
    }

    public void SetMap(Vector pos, Map map)
    {
        Cell cell = GetBlockCellFromPos(pos);
        Block block = blocks[cell.Row, cell.Col];
        if (block == null)
        {
            block = BaseEngine.Engine.World.ForegroundLayout.AddBlock();
            blocks[cell.Row, cell.Col] = block;
        }

        block.SetMap(pos - new Vector(cell.Col * BLOCK_SIZE, cell.Row * BLOCK_SIZE), map);

        if (Tessellated)
            RefreshLayers((pos.X, pos.Y, MAP_SIZE, MAP_SIZE));
    }

    public void SetMap(Cell cell, Map map)
    {
        SetMap(new Vector(cell.Col * MAP_SIZE, cell.Row * MAP_SIZE), map);
    }

    public void SetBlock(Vector pos, Block block)
    {
        Cell cell = GetBlockCellFromPos(pos);
        blocks[cell.Row, cell.Col] = block;

        if (Tessellated)
            RefreshLayers((pos.X, pos.Y, BLOCK_SIZE, BLOCK_SIZE));
    }

    public void SetBlock(Cell cell, Block block)
    {
        SetBlock(new Vector(cell.Col * BLOCK_SIZE, cell.Row * BLOCK_SIZE), block);
    }

    public void FillRectangle(Box box, Map map)
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

        if (Tessellated)
            RefreshLayers(box);
    }

    public void FillRectangle(Box box, Block block)
    {
        Vector boxLT = box.LeftTop;
        Vector boxSize = box.DiagonalVector;

        int col = (int) (boxLT.X / BLOCK_SIZE);
        int row = (int) (boxLT.Y / BLOCK_SIZE);
        int cols = (int) (boxSize.X / BLOCK_SIZE);
        int rows = (int) (boxSize.Y / BLOCK_SIZE);

        for (int c = 0; c < cols; c++)
        {
            for (int r = 0; r < rows; r++)
                SetBlock(new Vector((col + c) * BLOCK_SIZE, (row + r) * BLOCK_SIZE), block);
        }

        if (Tessellated)
            RefreshLayers(box);
    }

    private void RefreshLayers(Box box)
    {
        Cell start = GetBlockCellFromPos(box.LeftTop);
        int startCol = start.Col;
        int startRow = start.Row;

        if (startCol < 0)
            startCol = 0;

        if (startRow < 0)
            startRow = 0;

        if (startCol > SIDE_BLOCKS_PER_SCENE)
            startCol = SIDE_BLOCKS_PER_SCENE - 1;

        if (startRow > SIDE_BLOCKS_PER_SCENE)
            startRow = SIDE_BLOCKS_PER_SCENE - 1;

        Cell end = GetBlockCellFromPos(box.RightBottom);
        int endCol = end.Col;
        int endRow = end.Row;

        if (endCol < 0)
            endCol = 0;

        if (endRow < 0)
            endRow = 0;

        if (endCol > SIDE_BLOCKS_PER_SCENE)
            endCol = SIDE_BLOCKS_PER_SCENE;

        if (endRow > SIDE_BLOCKS_PER_SCENE)
            endRow = SIDE_BLOCKS_PER_SCENE;

        if (startCol == endCol)
            endCol++;

        if (startRow == endRow)
            endRow++;

        int startPos = BLOCK_PRIMITIVE_SIZE * (SIDE_BLOCKS_PER_SCENE * startCol + startRow);
        int sizeToLock = BLOCK_PRIMITIVE_SIZE * (endRow - startRow);

        for (int col = startCol; col < endCol; col++)
        {
            DataStream downLayerVBData = layers[0].Lock(startPos, sizeToLock, LockFlags.None);
            DataStream upLayerVBData = layers[1].Lock(startPos, sizeToLock, LockFlags.None);

            for (int row = startRow; row < endRow; row++)
            {
                var blockPos = new Vector(col * BLOCK_SIZE, -row * BLOCK_SIZE);
                Block block = blocks[row, col];

                if (block != null)
                {
                    block.Tessellate(downLayerVBData, upLayerVBData, blockPos);
                }
                else
                {
                    for (int tileRow = 0; tileRow < SIDE_TILES_PER_MAP * SIDE_MAPS_PER_BLOCK; tileRow++)
                    {
                        for (int tileCol = 0; tileCol < SIDE_TILES_PER_MAP * SIDE_MAPS_PER_BLOCK; tileCol++)
                        {
                            var tilePos = new Vector(blockPos.X + tileCol * TILE_SIZE, blockPos.Y - tileRow * TILE_SIZE);
                            BaseEngine.WriteSquare(downLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                            BaseEngine.WriteSquare(upLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                        }
                    }
                }
            }

            layers[0].Unlock();
            layers[1].Unlock();

            startPos += SIDE_BLOCKS_PER_SCENE * BLOCK_PRIMITIVE_SIZE;
        }
    }

    internal void Tessellate()
    {
        Dispose();

        layers[0] = new VertexBuffer(BaseEngine.Engine.Device, SCENE_PRIMITIVE_SIZE, Usage.WriteOnly, BaseEngine.D3DFVF_TLVERTEX, Pool.Managed);
        layers[1] = new VertexBuffer(BaseEngine.Engine.Device, SCENE_PRIMITIVE_SIZE, Usage.WriteOnly, BaseEngine.D3DFVF_TLVERTEX, Pool.Managed);

        DataStream downLayerVBData = layers[0].Lock(0, 0, LockFlags.None);
        DataStream upLayerVBData = layers[1].Lock(0, 0, LockFlags.None);

        for (int col = 0; col < SIDE_BLOCKS_PER_SCENE; col++)
        {
            for (int row = 0; row < SIDE_BLOCKS_PER_SCENE; row++)
            {
                var blockPos = new Vector(col * BLOCK_SIZE, -row * BLOCK_SIZE);
                Block block = blocks[row, col];

                if (block != null)
                {
                    block.Tessellate(downLayerVBData, upLayerVBData, blockPos);
                }
                else
                {
                    for (int tileRow = 0; tileRow < SIDE_TILES_PER_MAP * SIDE_MAPS_PER_BLOCK; tileRow++)
                    {
                        for (int tileCol = 0; tileCol < SIDE_TILES_PER_MAP * SIDE_MAPS_PER_BLOCK; tileCol++)
                        {
                            var tilePos = new Vector(blockPos.X + tileCol * TILE_SIZE, blockPos.Y - tileRow * TILE_SIZE);
                            BaseEngine.WriteSquare(downLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                            BaseEngine.WriteSquare(upLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                        }
                    }
                }
            }
        }

        layers[0].Unlock();
        layers[1].Unlock();

        Tessellated = true;
    }

    public void Dispose()
    {
        try
        {
            for (int i = 0; i < layers.Length; i++)
            {
                if (layers[i] != null)
                {
                    layers[i].Dispose();
                    layers[i] = null;
                }
            }
        }
        finally
        {
            Tessellated = false;
            GC.SuppressFinalize(this);
        }
    }

    internal void OnDisposeDevice()
    {
        Dispose();
    }
}