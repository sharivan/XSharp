﻿using MMX.Geometry;
using SharpDX;
using SharpDX.Direct3D9;
using System;
using static MMX.Engine.Consts;
using MMXBox = MMX.Geometry.Box;

namespace MMX.Engine.World
{
    public class Scene : IDisposable
    {
        public const int PRIMITIVE_COUNT = 2 * SIDE_BLOCKS_PER_SCENE * SIDE_MAPS_PER_BLOCK * SIDE_TILES_PER_MAP * SIDE_BLOCKS_PER_SCENE * SIDE_MAPS_PER_BLOCK * SIDE_TILES_PER_MAP;
        internal Block[,] blocks;

        internal VertexBuffer[] layers;

        public World World
        {
            get;
        }

        public int ID
        {
            get;
        }

        public Block this[int row, int col]
        {
            get => blocks[row, col];
            set => blocks[row, col] = value;
        }

        internal Scene(World world, int id)
        {
            World = world;
            ID = id;

            blocks = new Block[SIDE_BLOCKS_PER_SCENE, SIDE_BLOCKS_PER_SCENE];
            layers = new VertexBuffer[3];
        }

        public Tile GetTileFrom(Vector pos)
        {
            Cell tsp = World.GetBlockCellFromPos(pos);
            int row = tsp.Row;
            int col = tsp.Col;

            if (row < 0 || col < 0 || row >= SIDE_BLOCKS_PER_SCENE || col >= SIDE_BLOCKS_PER_SCENE)
                return null;

            Block block = blocks[row, col];
            return block?.GetTileFrom(pos - new Vector(col * BLOCK_SIZE, row * BLOCK_SIZE));
        }

        public Map GetMapFrom(Vector pos)
        {
            Cell tsp = World.GetBlockCellFromPos(pos);
            int row = tsp.Row;
            int col = tsp.Col;

            if (row < 0 || col < 0 || row >= SIDE_BLOCKS_PER_SCENE || col >= SIDE_BLOCKS_PER_SCENE)
                return null;

            Block block = blocks[row, col];
            return block?.GetMapFrom(pos - new Vector(col * BLOCK_SIZE, row * BLOCK_SIZE));
        }

        public Block GetBlockFrom(Vector pos)
        {
            Cell tsp = World.GetBlockCellFromPos(pos);
            int row = tsp.Row;
            int col = tsp.Col;

            return row < 0 || col < 0 || row >= SIDE_BLOCKS_PER_SCENE || col >= SIDE_BLOCKS_PER_SCENE ? null : blocks[row, col];
        }

        public void RemoveBlock(Block block)
        {
            if (block == null)
                return;

            for (int col = 0; col < SIDE_BLOCKS_PER_SCENE; col++)
                for (int row = 0; row < SIDE_BLOCKS_PER_SCENE; row++)
                    if (blocks[row, col] == block)
                        blocks[row, col] = null;
        }

        public void SetMap(Vector pos, Map map)
        {
            Cell cell = World.GetBlockCellFromPos(pos);
            Block block = blocks[cell.Row, cell.Col];
            if (block == null)
            {
                block = World.AddBlock();
                blocks[cell.Row, cell.Col] = block;
            }

            block.SetMap(pos - new Vector(cell.Col * BLOCK_SIZE, cell.Row * BLOCK_SIZE), map);
        }

        public void SetBlock(Vector pos, Block block)
        {
            Cell cell = World.GetBlockCellFromPos(pos);
            blocks[cell.Row, cell.Col] = block;
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
                for (int r = 0; r < rows; r++)
                    SetMap(new Vector((col + c) * MAP_SIZE, (row + r) * MAP_SIZE), map);
        }

        public void FillRectangle(MMXBox box, Block block)
        {
            Vector boxLT = box.LeftTop;
            Vector boxSize = box.DiagonalVector;

            int col = (int) (boxLT.X / BLOCK_SIZE);
            int row = (int) (boxLT.Y / BLOCK_SIZE);
            int cols = (int) (boxSize.X / BLOCK_SIZE);
            int rows = (int) (boxSize.Y / BLOCK_SIZE);

            for (int c = 0; c < cols; c++)
                for (int r = 0; r < rows; r++)
                    SetBlock(new Vector((col + c) * BLOCK_SIZE, (row + r) * BLOCK_SIZE), block);
        }

        internal void Tessellate()
        {
            Dispose();

            layers[0] = new VertexBuffer(World.Device, GameEngine.VERTEX_SIZE * 3 * PRIMITIVE_COUNT, Usage.WriteOnly, GameEngine.D3DFVF_TLVERTEX, Pool.Managed);
            layers[1] = new VertexBuffer(World.Device, GameEngine.VERTEX_SIZE * 3 * PRIMITIVE_COUNT, Usage.WriteOnly, GameEngine.D3DFVF_TLVERTEX, Pool.Managed);

            DataStream downLayerVBData = layers[0].Lock(0, 4 * GameEngine.VERTEX_SIZE, LockFlags.None);
            DataStream upLayerVBData = layers[1].Lock(0, 4 * GameEngine.VERTEX_SIZE, LockFlags.None);

            for (int col = 0; col < SIDE_BLOCKS_PER_SCENE; col++)
                for (int row = 0; row < SIDE_BLOCKS_PER_SCENE; row++)
                {
                    var blockPos = new Vector(col * BLOCK_SIZE, -row * BLOCK_SIZE);
                    Block block = blocks[row, col];

                    if (block != null)
                        block.Tessellate(downLayerVBData, upLayerVBData, blockPos);
                    else
                    {
                        for (int tileRow = 0; tileRow < SIDE_TILES_PER_MAP * SIDE_MAPS_PER_BLOCK; tileRow++)
                            for (int tileCol = 0; tileCol < SIDE_TILES_PER_MAP * SIDE_MAPS_PER_BLOCK; tileCol++)
                            {
                                var tilePos = new Vector(blockPos.X + tileCol * TILE_SIZE, blockPos.Y - tileRow * TILE_SIZE);
                                GameEngine.WriteSquare(downLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                                GameEngine.WriteSquare(upLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                            }
                    }
                }

            layers[0].Unlock();
            layers[1].Unlock();
        }

        public void Dispose()
        {
            for (int i = 0; i < layers.Length; i++)
                if (layers[i] != null)
                {
                    layers[i].Dispose();
                    layers[i] = null;
                }
        }

        internal void OnDisposeDevice()
        {
            Dispose();
        }
    }
}
