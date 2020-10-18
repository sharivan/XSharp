using System;

using SharpDX;
using SharpDX.Direct3D9;

using MMX.Math;
using MMX.Geometry;

using MMXBox = MMX.Geometry.Box;

using static MMX.Engine.Consts;

namespace MMX.Engine
{
    public class Scene : IDisposable
    {
        public const int PRIMITIVE_COUNT = 2 * SIDE_BLOCKS_PER_SCENE * SIDE_MAPS_PER_BLOCK * SIDE_TILES_PER_MAP * SIDE_BLOCKS_PER_SCENE * SIDE_MAPS_PER_BLOCK * SIDE_TILES_PER_MAP;

        private World world;
        private int id;
        internal Block[,] blocks;

        internal VertexBuffer downLayerVB;
        internal VertexBuffer upLayerVB;        

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

        public Block this[int row, int col]
        {
            get
            {
                return blocks[row, col];
            }

            set
            {
                blocks[row, col] = value;
            }
        }

        internal Scene(World world, int id)
        {
            this.world = world;
            this.id = id;

            blocks = new Block[SIDE_BLOCKS_PER_SCENE, SIDE_BLOCKS_PER_SCENE];
            downLayerVB = new VertexBuffer(world.Device, GameEngine.VERTEX_SIZE * 3 * PRIMITIVE_COUNT, Usage.WriteOnly, GameEngine.D3DFVF_TLVERTEX, Pool.Managed);
            upLayerVB = new VertexBuffer(world.Device, GameEngine.VERTEX_SIZE * 3 * PRIMITIVE_COUNT, Usage.WriteOnly, GameEngine.D3DFVF_TLVERTEX, Pool.Managed);
        }

        public Tile GetTileFrom(Vector pos)
        {
            Cell tsp = World.GetBlockCellFromPos(pos);
            int row = tsp.Row;
            int col = tsp.Col;

            if (row < 0 || col < 0 || row >= SIDE_BLOCKS_PER_SCENE || col >= SIDE_BLOCKS_PER_SCENE)
                return null;

            Block block = blocks[row, col];
            if (block == null)
                return null;

            return block.GetTileFrom(pos - new Vector(col * BLOCK_SIZE, row * BLOCK_SIZE));
        }

        public Map GetMapFrom(Vector pos)
        {
            Cell tsp = World.GetBlockCellFromPos(pos);
            int row = tsp.Row;
            int col = tsp.Col;

            if (row < 0 || col < 0 || row >= SIDE_BLOCKS_PER_SCENE || col >= SIDE_BLOCKS_PER_SCENE)
                return null;

            Block block = blocks[row, col];
            if (block == null)
                return null;

            return block.GetMapFrom(pos - new Vector(col * BLOCK_SIZE, row * BLOCK_SIZE));
        }

        public Block GetBlockFrom(Vector pos)
        {
            Cell tsp = World.GetBlockCellFromPos(pos);
            int row = tsp.Row;
            int col = tsp.Col;

            if (row < 0 || col < 0 || row >= SIDE_BLOCKS_PER_SCENE || col >= SIDE_BLOCKS_PER_SCENE)
                return null;

            return blocks[row, col];
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
                block = world.AddBlock();
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
            DataStream downLayerVBData = downLayerVB.Lock(0, 4 * GameEngine.VERTEX_SIZE, LockFlags.None);
            DataStream upLayerVBData = upLayerVB.Lock(0, 4 * GameEngine.VERTEX_SIZE, LockFlags.None);

            for (int col = 0; col < SIDE_BLOCKS_PER_SCENE; col++)
                for (int row = 0; row < SIDE_BLOCKS_PER_SCENE; row++)
                {
                    Vector blockPos = new Vector(col * BLOCK_SIZE, -row * BLOCK_SIZE);
                    Block block = blocks[row, col];

                    if (block != null)
                        block.Tessellate(downLayerVBData, upLayerVBData, blockPos);
                    else
                    {
                        for (int tileRow = 0; tileRow < SIDE_TILES_PER_MAP * SIDE_MAPS_PER_BLOCK; tileRow++)
                            for (int tileCol = 0; tileCol < SIDE_TILES_PER_MAP * SIDE_MAPS_PER_BLOCK; tileCol++)
                            {
                                Vector tilePos = new Vector(blockPos.X + tileCol * TILE_SIZE, blockPos.Y - tileRow * TILE_SIZE);
                                GameEngine.WriteSquare(downLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                                GameEngine.WriteSquare(upLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                            }
                    }
                }

            upLayerVB.Unlock();
            downLayerVB.Unlock();
        }

        public void Dispose()
        {
            if (downLayerVB != null)
            {
                downLayerVB.Dispose();
                downLayerVB = null;
            }

            if (upLayerVB != null)
            {
                upLayerVB.Dispose();
                upLayerVB = null;
            }
        }
    }
}
