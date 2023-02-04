using MMX.Geometry;
using SharpDX;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using static MMX.Engine.Consts;
using MMXBox = MMX.Geometry.Box;

namespace MMX.Engine.World
{
    public class Scene : IDisposable
    {
        public const int TILE_PRIMITIVE_SIZE = 2 * GameEngine.VERTEX_SIZE * 3;
        public const int MAP_PRIMITIVE_SIZE = SIDE_TILES_PER_MAP * SIDE_TILES_PER_MAP * TILE_PRIMITIVE_SIZE;
        public const int BLOCK_PRIMITIVE_SIZE = SIDE_MAPS_PER_BLOCK * SIDE_MAPS_PER_BLOCK * MAP_PRIMITIVE_SIZE;
        public const int SCENE_PRIMITIVE_SIZE = SIDE_BLOCKS_PER_SCENE * SIDE_BLOCKS_PER_SCENE * BLOCK_PRIMITIVE_SIZE;
        public const int PRIMITIVE_COUNT = SCENE_PRIMITIVE_SIZE / (GameEngine.VERTEX_SIZE * 3);

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

        public bool Tessellated
        {
            get;
            private set;
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

            List<Cell> positions = new();

            for (int col = 0; col < SIDE_BLOCKS_PER_SCENE; col++)
                for (int row = 0; row < SIDE_BLOCKS_PER_SCENE; row++)
                    if (blocks[row, col] == block)
                    {
                        positions.Add((row, col));
                        blocks[row, col] = null;
                    }

            if (Tessellated)
                foreach (var cell in positions)
                    RefreshLayers((cell.Col * BLOCK_SIZE, cell.Row * BLOCK_SIZE, BLOCK_SIZE, BLOCK_SIZE));
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

            if (Tessellated)
                RefreshLayers((pos, MAP_SIZE, MAP_SIZE));
        }

        public void SetMap(Cell cell, Map map)
        {
            SetMap(new Vector(cell.Col * MAP_SIZE, cell.Row * MAP_SIZE), map);
        }

        public void SetBlock(Vector pos, Block block)
        {
            Cell cell = World.GetBlockCellFromPos(pos);
            blocks[cell.Row, cell.Col] = block;

            if (Tessellated)
                RefreshLayers((pos, BLOCK_SIZE, BLOCK_SIZE));
        }

        public void SetBlock(Cell cell, Block block)
        {
            SetBlock(new Vector(cell.Col * BLOCK_SIZE, cell.Row * BLOCK_SIZE), block);
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

            if (Tessellated)
                RefreshLayers(box);
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

            if (Tessellated)
                RefreshLayers(box);
        }

        private void RefreshLayers(MMXBox box)
        {
            Cell start = World.GetBlockCellFromPos(box.LeftTop);
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

            Cell end = World.GetBlockCellFromPos(box.RightBottom);
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

                startPos += SIDE_BLOCKS_PER_SCENE * BLOCK_PRIMITIVE_SIZE;
            }
        }

        internal void Tessellate()
        {
            Dispose();

            layers[0] = new VertexBuffer(World.Device, SCENE_PRIMITIVE_SIZE, Usage.WriteOnly, GameEngine.D3DFVF_TLVERTEX, Pool.Managed);
            layers[1] = new VertexBuffer(World.Device, SCENE_PRIMITIVE_SIZE, Usage.WriteOnly, GameEngine.D3DFVF_TLVERTEX, Pool.Managed);

            DataStream downLayerVBData = layers[0].Lock(0, 0, LockFlags.None);
            DataStream upLayerVBData = layers[1].Lock(0, 0, LockFlags.None);

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

            Tessellated = true;
        }

        public void Dispose()
        {
            for (int i = 0; i < layers.Length; i++)
                if (layers[i] != null)
                {
                    layers[i].Dispose();
                    layers[i] = null;
                }

            Tessellated = false;
        }

        internal void OnDisposeDevice()
        {
            Dispose();
        }
    }
}