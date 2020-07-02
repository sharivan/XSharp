using MMX.Geometry;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using System;

using static MMX.Engine.Consts;

namespace MMX.Engine
{
    public class Scene : IDisposable
    {
        private World world;
        private int id;
        internal Block[,] blocks;

        //internal Bitmap downLayerImage;
        internal BitmapRenderTarget downLayerTarget;

        //internal Bitmap upLayerImage;
        internal BitmapRenderTarget upLayerTarget;

        private bool updating;

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

                if (!updating)
                {
                    if (value != null)
                        PaintBlock(row, col, value);
                    else
                        ClearBlock(row, col);
                }
            }
        }

        internal Scene(World world, int id)
        {
            this.world = world;
            this.id = id;

            blocks = new Block[SIDE_BLOCKS_PER_SCENE, SIDE_BLOCKS_PER_SCENE];

            var size = new Size2(SCENE_SIZE, SCENE_SIZE);
            var sizef = new Size2F(SCENE_SIZE, SCENE_SIZE);
            var pixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied);

            downLayerTarget = new BitmapRenderTarget(world.Engine.Target, CompatibleRenderTargetOptions.None, sizef, size, pixelFormat);
            upLayerTarget = new BitmapRenderTarget(world.Engine.Target, CompatibleRenderTargetOptions.None, sizef, size, pixelFormat);
        }

        private void PaintBlock(int row, int col, Block block)
        {
            downLayerTarget.BeginDraw();
            block.PaintDownLayer(downLayerTarget, new Vector(col * BLOCK_SIZE, row * BLOCK_SIZE));
            downLayerTarget.Flush();
            downLayerTarget.EndDraw();

            upLayerTarget.BeginDraw();
            block.PaintUpLayer(upLayerTarget, new Vector(col * BLOCK_SIZE, row * BLOCK_SIZE));
            upLayerTarget.Flush();
            upLayerTarget.EndDraw();
        }

        private void ClearBlock(int row, int col)
        {
            using (Brush brush = new SolidColorBrush(downLayerTarget, Color.Transparent))
            {
                downLayerTarget.FillRectangle(new RectangleF(col * BLOCK_SIZE, row * BLOCK_SIZE, BLOCK_SIZE, BLOCK_SIZE), brush);   
            }

            using (Brush brush = new SolidColorBrush(upLayerTarget, Color.Transparent))
            {
                upLayerTarget.FillRectangle(new RectangleF(col * BLOCK_SIZE, row * BLOCK_SIZE, BLOCK_SIZE, BLOCK_SIZE), brush);
            }
        }

        internal void UpdateLayers()
        {
            UpdateDownLayer();
            UpdateUpLayer();
        }

        private void UpdateDownLayer()
        {
            downLayerTarget.BeginDraw();
            downLayerTarget.Transform = Matrix3x2.Identity;
            downLayerTarget.Clear(Color.Transparent);

            for (int col = 0; col < SIDE_BLOCKS_PER_SCENE; col++)
                for (int row = 0; row < SIDE_BLOCKS_PER_SCENE; row++)
                {
                    Block block = blocks[row, col];
                    if (block != null)
                        block.PaintDownLayer(downLayerTarget, new Vector(col * BLOCK_SIZE, row * BLOCK_SIZE));
                }

            //downLayerTarget.Flush();
            downLayerTarget.EndDraw();
        }

        private void UpdateUpLayer()
        {
            upLayerTarget.BeginDraw();
            upLayerTarget.Transform = Matrix3x2.Identity;
            upLayerTarget.Clear(Color.Transparent);

            for (int col = 0; col < SIDE_BLOCKS_PER_SCENE; col++)
                for (int row = 0; row < SIDE_BLOCKS_PER_SCENE; row++)
                {
                    Block block = blocks[row, col];
                    if (block != null)
                        block.PaintUpLayer(upLayerTarget, new Vector(col * BLOCK_SIZE, row * BLOCK_SIZE));
                }

            //upLayerTarget.Flush();
            upLayerTarget.EndDraw();
        }

        internal void BeginUpdate()
        {
            updating = true;
        }

        internal void EndUpdate()
        {
            if (!updating)
                return;

            updating = false;
            UpdateLayers();
        }

        public void Dispose()
        {
            downLayerTarget.Dispose();
            upLayerTarget.Dispose();          
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
                    {
                        blocks[row, col] = null;
                        ClearBlock(row, col);
                    }
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

            if (!updating)
                PaintBlock(cell.Row, cell.Col, block);
        }

        public void SetBlock(Vector pos, Block block)
        {
            Cell cell = World.GetBlockCellFromPos(pos);
            blocks[cell.Row, cell.Col] = block;

            if (!updating)
            {
                if (block != null)
                    PaintBlock(cell.Row, cell.Col, block);
                else
                    ClearBlock(cell.Row, cell.Col);
            }
        }

        public void Fill(Bitmap source, Point offset, CollisionData collisionData = CollisionData.NONE, bool flipped = false, bool mirrored = false, bool upLayer = false)
        {
            BeginUpdate();
            for (int col = 0; col < SIDE_BLOCKS_PER_SCENE; col++)
                for (int row = 0; row < SIDE_BLOCKS_PER_SCENE; row++)
                {
                    Block block = world.AddBlock();
                    block.Fill(source, new Point((int) (offset.X + col * BLOCK_SIZE), (int) (offset.Y + row * BLOCK_SIZE)), collisionData, flipped, mirrored, upLayer);
                    blocks[row, col] = block;
                }

            EndUpdate();
        }

        public void FillRectangle(Box box, Map map)
        {
            Vector boxLT = box.LeftTop;
            Vector boxSize = box.DiagonalVector;

            int col = (int) (boxLT.X / MAP_SIZE);
            int row = (int) (boxLT.Y / MAP_SIZE);
            int cols = (int) (boxSize.X / MAP_SIZE);
            int rows = (int) (boxSize.Y / MAP_SIZE);

            BeginUpdate();
            for (int c = 0; c < cols; c++)
                for (int r = 0; r < rows; r++)
                    SetMap(new Vector((col + c) * MAP_SIZE, (row + r) * MAP_SIZE), map);

            EndUpdate();
        }

        public void FillRectangle(Box box, Block block)
        {
            Vector boxLT = box.LeftTop;
            Vector boxSize = box.DiagonalVector;

            int col = (int) (boxLT.X / BLOCK_SIZE);
            int row = (int) (boxLT.Y / BLOCK_SIZE);
            int cols = (int) (boxSize.X / BLOCK_SIZE);
            int rows = (int) (boxSize.Y / BLOCK_SIZE);

            BeginUpdate();
            for (int c = 0; c < cols; c++)
                for (int r = 0; r < rows; r++)
                    SetBlock(new Vector((col + c) * BLOCK_SIZE, (row + r) * BLOCK_SIZE), block);

            EndUpdate();
        }

        public void OnFrame()
        {
        }
    }
}
