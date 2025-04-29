using System;
using System.Collections.Generic;

using XSharp.Engine.Graphics;
using XSharp.Graphics;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

using Box = XSharp.Math.Geometry.Box;

namespace XSharp.Engine.World;

public abstract class Scene(int id) : IDisposable
{
    public static BaseEngine Engine => BaseEngine.Engine;

    public static Cell GetBlockCellFromPos(Vector pos)
    {
        int col = (int) (pos.X / BLOCK_SIZE);
        int row = (int) (pos.Y / BLOCK_SIZE);

        return new Cell(row, col);
    }

    protected internal Block[,] blocks = new Block[SIDE_BLOCKS_PER_SCENE, SIDE_BLOCKS_PER_SCENE];
    protected internal List<Tilemap> tilemaps = [];

    public int ID
    {
        get;
    } = id;

    public Block this[int row, int col]
    {
        get => blocks[row, col];
        set => blocks[row, col] = value;
    }

    public bool Tessellated
    {
        get;
        protected set;
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

        List<Cell> positions = [];

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
            block = Engine.World.ForegroundLayout.AddBlock();
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

    protected abstract void RefreshLayersImpl(int startRow, int startCol, int endRow, int endCol);

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

        RefreshLayersImpl(startRow, startCol, endRow, endCol);
    }

    protected void TesselateBlock(Block block, DataStream downLayerVBData, DataStream upLayerVBData, Vector blockPos)
    {
        block.Tessellate(downLayerVBData, upLayerVBData, blockPos);
    }

    protected abstract void TesselateImpl();

    internal void Tessellate()
    {
        Dispose();
        TesselateImpl();
        Tessellated = true;
    }

    public virtual void Dispose()
    {
    }

    internal void OnDisposeDevice()
    {
        Dispose();
    }

    protected internal abstract void RenderLayer(int layer, ITexture tilemap, Palette palette, FadingControl fadingControl, Box sceneBox);
}