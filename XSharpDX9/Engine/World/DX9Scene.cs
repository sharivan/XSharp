using SharpDX.Direct3D9;
using XSharp.Engine.Graphics;
using XSharp.Graphics;
using XSharp.Math.Fixed.Geometry;
using static XSharp.Engine.Consts;
using Box = XSharp.Math.Fixed.Geometry.Box;

namespace XSharp.Engine.World;

public class DX9Scene : Scene
{
    new public static DX9Engine Engine => (DX9Engine) Scene.Engine;

    public const int TILE_PRIMITIVE_SIZE = 2 * DX9Engine.VERTEX_SIZE * 3;
    public const int MAP_PRIMITIVE_SIZE = SIDE_TILES_PER_MAP * SIDE_TILES_PER_MAP * TILE_PRIMITIVE_SIZE;
    public const int BLOCK_PRIMITIVE_SIZE = SIDE_MAPS_PER_BLOCK * SIDE_MAPS_PER_BLOCK * MAP_PRIMITIVE_SIZE;
    public const int SCENE_PRIMITIVE_SIZE = SIDE_BLOCKS_PER_SCENE * SIDE_BLOCKS_PER_SCENE * BLOCK_PRIMITIVE_SIZE;
    public const int PRIMITIVE_COUNT = SCENE_PRIMITIVE_SIZE / (DX9Engine.VERTEX_SIZE * 3);

    internal VertexBuffer[] layers;

    internal DX9Scene(int id)
        : base(id)
    {
        layers = new VertexBuffer[3];
    }

    protected override void RefreshLayersImpl(int startRow, int startCol, int endRow, int endCol)
    {
        int startPos = BLOCK_PRIMITIVE_SIZE * (SIDE_BLOCKS_PER_SCENE * startCol + startRow);
        int sizeToLock = BLOCK_PRIMITIVE_SIZE * (endRow - startRow);

        for (int col = startCol; col < endCol; col++)
        {
            DX9DataStream downLayerVBData = layers[0].Lock(startPos, sizeToLock, LockFlags.None);
            DX9DataStream upLayerVBData = layers[1].Lock(startPos, sizeToLock, LockFlags.None);

            for (int row = startRow; row < endRow; row++)
            {
                var blockPos = new Vector(col * BLOCK_SIZE, -row * BLOCK_SIZE);
                Block block = blocks[row, col];

                if (block != null)
                {
                    TesselateBlock(block, downLayerVBData, upLayerVBData, blockPos);
                }
                else
                {
                    for (int tileRow = 0; tileRow < SIDE_TILES_PER_MAP * SIDE_MAPS_PER_BLOCK; tileRow++)
                    {
                        for (int tileCol = 0; tileCol < SIDE_TILES_PER_MAP * SIDE_MAPS_PER_BLOCK; tileCol++)
                        {
                            var tilePos = new Vector(blockPos.X + tileCol * TILE_SIZE, blockPos.Y - tileRow * TILE_SIZE);
                            BaseEngine.Engine.WriteSquare(downLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                            BaseEngine.Engine.WriteSquare(upLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                        }
                    }
                }
            }

            layers[0].Unlock();
            layers[1].Unlock();

            startPos += SIDE_BLOCKS_PER_SCENE * BLOCK_PRIMITIVE_SIZE;
        }
    }

    protected override void TesselateImpl()
    {
        layers[0] = new VertexBuffer(DX9Engine.Engine.Device, SCENE_PRIMITIVE_SIZE, Usage.WriteOnly, DX9Engine.D3DFVF_TLVERTEX, Pool.Managed);
        layers[1] = new VertexBuffer(DX9Engine.Engine.Device, SCENE_PRIMITIVE_SIZE, Usage.WriteOnly, DX9Engine.D3DFVF_TLVERTEX, Pool.Managed);

        DX9DataStream downLayerVBData = layers[0].Lock(0, 0, LockFlags.None);
        DX9DataStream upLayerVBData = layers[1].Lock(0, 0, LockFlags.None);

        for (int col = 0; col < SIDE_BLOCKS_PER_SCENE; col++)
        {
            for (int row = 0; row < SIDE_BLOCKS_PER_SCENE; row++)
            {
                var blockPos = new Vector(col * BLOCK_SIZE, -row * BLOCK_SIZE);
                Block block = blocks[row, col];

                if (block != null)
                {
                    TesselateBlock(block, downLayerVBData, upLayerVBData, blockPos);
                }
                else
                {
                    for (int tileRow = 0; tileRow < SIDE_TILES_PER_MAP * SIDE_MAPS_PER_BLOCK; tileRow++)
                    {
                        for (int tileCol = 0; tileCol < SIDE_TILES_PER_MAP * SIDE_MAPS_PER_BLOCK; tileCol++)
                        {
                            var tilePos = new Vector(blockPos.X + tileCol * TILE_SIZE, blockPos.Y - tileRow * TILE_SIZE);
                            BaseEngine.Engine.WriteSquare(downLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                            BaseEngine.Engine.WriteSquare(upLayerVBData, Vector.NULL_VECTOR, tilePos, World.TILE_FRAC_SIZE_VECTOR, World.TILE_SIZE_VECTOR);
                        }
                    }
                }
            }
        }

        layers[0].Unlock();
        layers[1].Unlock();
    }

    public override void Dispose()
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
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    protected override void RenderLayer(int layer, ITexture tilemap, Palette palette, FadingControl fadingControl, Box sceneBox)
    {
        Engine.RenderVertexBuffer(layers[layer], DX9Engine.VERTEX_SIZE, PRIMITIVE_COUNT, tilemap, palette, fadingControl, sceneBox);
    }
}