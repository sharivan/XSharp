using System.Diagnostics;
using System.Reflection;
using System.Drawing.Drawing2D;
using System.Numerics;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.D3DCompiler;

using MMX.ROM;

using static System.Windows.Forms.DataFormats;

using Color = SharpDX.Color;
using D3D11Device = SharpDX.Direct3D11.Device;
using Matrix = SharpDX.Matrix;
using Buffer = SharpDX.Direct3D11.Buffer;
using Format = SharpDX.DXGI.Format;
using Vector2 = SharpDX.Vector2;
using RectangleF = SharpDX.RectangleF;

namespace D3D11Test
{
    public class MainClass
    {
        private static readonly byte[] VERTEX_SHADER_BYTECODE = new byte[]
        {
              0,   2, 254, 255, 254, 255,
             20,   0,  67,  84,  65,  66,
             28,   0,   0,   0,  35,   0,
              0,   0,   0,   2, 254, 255,
              0,   0,   0,   0,   0,   0,
              0,   0,   0,   1,   0,   0,
             28,   0,   0,   0, 118, 115,
             95,  50,  95,  48,   0,  77,
            105,  99, 114, 111, 115, 111,
            102, 116,  32,  40,  82,  41,
             32,  72,  76,  83,  76,  32,
             83, 104,  97, 100, 101, 114,
             32,  67, 111, 109, 112, 105,
            108, 101, 114,  32,  49,  48,
             46,  49,   0, 171,  31,   0,
              0,   2,   0,   0,   0, 128,
              0,   0,  15, 144,  31,   0,
              0,   2,  10,   0,   0, 128,
              1,   0,  15, 144,   1,   0,
              0,   2,   0,   0,  15, 192,
              0,   0, 228, 144,   1,   0,
              0,   2,   0,   0,  15, 208,
              1,   0, 228, 144, 255, 255,
              0,   0
        };

        private static readonly byte[] PIXEL_SHADER_BYTECODE = new byte[]
        {
              0,   2, 255, 255, 254, 255,
             42,   0,  67,  84,  65,  66,
             28,   0,   0,   0, 123,   0,
              0,   0,   0,   2, 255, 255,
              2,   0,   0,   0,  28,   0,
              0,   0,   0,   1,   0,   0,
            116,   0,   0,   0,  68,   0,
              0,   0,   3,   0,   0,   0,
              1,   0,   0,   0,  76,   0,
              0,   0,   0,   0,   0,   0,
             92,   0,   0,   0,   3,   0,
              1,   0,   1,   0,   0,   0,
            100,   0,   0,   0,   0,   0,
              0,   0, 105, 109,  97, 103,
            101,   0, 171, 171,   4,   0,
             12,   0,   1,   0,   1,   0,
              1,   0,   0,   0,   0,   0,
              0,   0, 112,  97, 108, 101,
            116, 116, 101,   0,   4,   0,
             11,   0,   1,   0,   1,   0,
              1,   0,   0,   0,   0,   0,
              0,   0, 112, 115,  95,  50,
             95,  48,   0,  77, 105,  99,
            114, 111, 115, 111, 102, 116,
             32,  40,  82,  41,  32,  72,
             76,  83,  76,  32,  83, 104,
             97, 100, 101, 114,  32,  67,
            111, 109, 112, 105, 108, 101,
            114,  32,  49,  48,  46,  49,
              0, 171,  81,   0,   0,   5,
              0,   0,  15, 160,   0,   0,
            127,  63,   0,   0,   0,  59,
              0,   0,   0,   0,   0,   0,
              0,   0,  31,   0,   0,   2,
              0,   0,   0, 128,   0,   0,
              3, 176,  31,   0,   0,   2,
              0,   0,   0, 144,   0,   8,
             15, 160,  31,   0,   0,   2,
              0,   0,   0, 144,   1,   8,
             15, 160,  66,   0,   0,   3,
              0,   0,  15, 128,   0,   0,
            228, 176,   0,   8, 228, 160,
              4,   0,   0,   4,   0,   0,
              3, 128,   0,   0,   0, 128,
              0,   0,   0, 160,   0,   0,
             85, 160,  66,   0,   0,   3,
              0,   0,  15, 128,   0,   0,
            228, 128,   1,   8, 228, 160,
              1,   0,   0,   2,   0,   8,
             15, 128,   0,   0, 228, 128,
            255, 255,   0,   0
        };

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remover membros particulares não lidos", Justification = "<Pendente>")]
        private static readonly byte[] EMPTY_TILE = new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0
        };

        private const int TILE_SIZE = 8;
        private const int SIDE_TILES_PER_MAP = 2;
        private const int MAP_SIZE = SIDE_TILES_PER_MAP * TILE_SIZE;

        private const int TILESET_COLOR_SIZE = sizeof(byte);
        private const int TILEMAP_COLOR_SIZE = sizeof(ushort);

        private static readonly string ROM_NAME = "ShittyDash.mmx";
        private const int LEVEL = 0;

        private const int VERTEX_SIZE = 24;

        private const int TILESET_WIDTH = 32 * TILE_SIZE;
        private const int TILESET_HEIGHT = 32 * TILE_SIZE;

        private const int TILEMAP_WIDTH = 32 * MAP_SIZE;
        private const int TILEMAP_HEIGHT = 32 * MAP_SIZE;

        private const float TILE_FRAC_SIZE_IN_TILESET = (float) TILE_SIZE / TILESET_WIDTH;
        private const float TILE_FRAC_SIZE_IN_TILEMAP = (float) TILE_SIZE / TILEMAP_WIDTH;

        private const int TILES_PER_ROW_IN_IMAGE = 64;
        private const int TILES_PER_COL_IN_IMAGE = 16;

        private const int MAPS_PER_ROW_IN_IMAGE = 64;
        private const int MAPS_PER_COL_IN_IMAGE = 16;

        private const int TILESET_IMAGE_WIDTH = TILES_PER_COL_IN_IMAGE * TILE_SIZE;
        private const int TILESET_IMAGE_HEIGHT = TILES_PER_ROW_IN_IMAGE * TILE_SIZE;

        private const int TILEMAP_IMAGE_WIDTH = MAPS_PER_COL_IN_IMAGE * MAP_SIZE;
        private const int TILEMAP_IMAGE_HEIGHT = MAPS_PER_ROW_IN_IMAGE * MAP_SIZE;

        private const int IMAGE_WIDTH = TILESET_IMAGE_WIDTH + TILEMAP_IMAGE_WIDTH;
        private const int IMAGE_HEIGHT = TILESET_IMAGE_HEIGHT > TILEMAP_IMAGE_HEIGHT ? TILESET_IMAGE_HEIGHT : TILEMAP_IMAGE_HEIGHT;

        private const int SCREEN_WIDTH = IMAGE_WIDTH;
        private const int SCREEN_HEIGHT = IMAGE_HEIGHT;
        private const int SCREEN_SCALE = 4;

        private enum CollisionData
        {
            NONE = 0x00,
            SLOPE_16_8 = 0x01,
            SLOPE_8_0 = 0x02,
            SLOPE_8_16 = 0x03,
            SLOPE_0_8 = 0x04,
            SLOPE_16_12 = 0x05,
            SLOPE_12_8 = 0x06,
            SLOPE_8_4 = 0x07,
            SLOPE_4_0 = 0x08,
            SLOPE_12_16 = 0x09,
            SLOPE_8_12 = 0x0A,
            SLOPE_4_8 = 0x0B,
            SLOPE_0_4 = 0x0C,
            WATER = 0x0D,
            WATER_SURFACE = 0x0E,
            UNKNOW0F = 0x0F,
            UNKNOW10 = 0x10,
            MUD = 0x11,
            LADDER = 0x12,
            TOP_LADDER = 0x13,
            UNKNOW14 = 0x14,
            UNKNOW15 = 0x15,
            UNKNOW16 = 0x16,
            UNKNOW17 = 0x17,
            UNKNOW18 = 0x18,
            UNKNOW19 = 0x19,
            UNKNOW1A = 0x1A,
            UNKNOW1B = 0x1B,
            UNKNOW1C = 0x1C,
            UNKNOW1D = 0x1D,
            UNKNOW1E = 0x1E,
            UNKNOW1F = 0x1F,
            UNKNOW20 = 0x20,
            UNKNOW21 = 0x21,
            UNKNOW22 = 0x22,
            UNKNOW23 = 0x23,
            UNKNOW24 = 0x24,
            UNKNOW25 = 0x25,
            UNKNOW26 = 0x26,
            UNKNOW27 = 0x27,
            UNKNOW28 = 0x28,
            UNKNOW29 = 0x29,
            UNKNOW2A = 0x2A,
            UNKNOW2B = 0x2B,
            UNKNOW2C = 0x2C,
            UNKNOW2D = 0x2D,
            UNKNOW2E = 0x2E,
            UNKNOW2F = 0x2F,
            UNKNOW30 = 0x30,
            UNKNOW31 = 0x31,
            UNKNOW32 = 0x32,
            LAVA = 0x33,
            UNKNOW34 = 0x34,
            UNKNOW35 = 0x35,
            UNCLIMBABLE_SOLID = 0x36,
            LEFT_TREADMILL = 0x37,
            RIGHT_TREADMILL = 0x38,
            UP_SLOPE_BASE = 0x39,
            DOWN_SLOPE_BASE = 0x3A,
            SOLID = 0x3B,
            BREAKABLE = 0x3C,
            DOOR = 0x3D,
            NON_LETHAL_SPIKE = 0x3E,
            LETHAL_SPIKE = 0x3F,
            UNKNOW40 = 0x40,
            UNKNOW41 = 0x41,
            UNKNOW42 = 0x42,
            UNKNOW43 = 0x43,
            UNKNOW44 = 0x44,
            LEFT_TREADMILL_SLOPE_16_12 = 0x45,
            LEFT_TREADMILL_SLOPE_12_8 = 0x46,
            LEFT_TREADMILL_SLOPE_8_4 = 0x47,
            LEFT_TREADMILL_SLOPE_4_0 = 0x48,
            RIGHT_TREADMILL_SLOPE_12_16 = 0x49,
            RIGHT_TREADMILL_SLOPE_8_12 = 0x4A,
            RIGHT_TREADMILL_SLOPE_4_8 = 0x4B,
            RIGHT_TREADMILL_SLOPE_0_4 = 0x4C,
            UNKNOW4D = 0x4D,
            UNKNOW4E = 0x4E,
            UNKNOW4F = 0x4F,
            ICE_SLOPE_16_12 = 0x85,
            ICE_SLOPE_12_8 = 0x86,
            ICE_SLOPE_8_4 = 0x87,
            ICE_SLOPE_4_0 = 0x88,
            ICE_SLOPE_12_16 = 0x89,
            ICE_SLOPE_8_12 = 0x8A,
            ICE_SLOPE_4_8 = 0x8B,
            ICE_SLOPE_0_4 = 0x8C,
            ICE = 0xBB
        }

        private readonly struct Cell
        {
            public int Row
            {
                get;
            }

            public int Col
            {
                get;
            }

            public Cell(int row, int col)
            {
                Row = row;
                Col = col;
            }

            public override int GetHashCode() => 65536 * Row + Col;

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                if (!(obj is Cell))
                    return false;

                var other = (Cell) obj;
                return other.Row == Row && other.Col == Col;
            }

            public override string ToString() => Row + "," + Col;
        }

        private class Tile
        {
            internal readonly int id;
            internal byte[] data;

            internal Tile(D3D11Device device, int id, byte[] data)
            {
                this.id = id;
                this.data = data;
            }
        }

        private static void WriteVertex(DataStream vbData, float x, float y, float u, float v)
        {
            vbData.Write(x - 0.5f);
            vbData.Write(y - 0.5f);
            vbData.Write(1f);
            vbData.Write(0xffffffff);
            vbData.Write(u);
            vbData.Write(v);
        }

        private class Map
        {
            internal int id;
            internal CollisionData collisionData;

            internal Tile[,] tiles;
            internal int[,] subPalette;
            internal bool[,] flipped;
            internal bool[,] mirrored;
            internal bool[,] upLayer;

            public Tile this[int row, int col]
            {
                get => tiles[row, col];

                set => tiles[row, col] = value;
            }

            public bool IsNull
            {
                get
                {
                    for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
                        for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
                        {
                            Tile tile = tiles[row, col];
                            if (tile != null)
                                return false;
                        }

                    return true;
                }
            }

            internal Map(int id, CollisionData collisionData = CollisionData.NONE)
            {
                this.id = id;
                this.collisionData = collisionData;

                tiles = new Tile[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
                subPalette = new int[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
                flipped = new bool[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
                mirrored = new bool[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
                upLayer = new bool[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
            }

            public void RemoveTile(Tile tile)
            {
                if (tile == null)
                    return;

                for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
                    for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
                        if (tiles[row, col] == tile)
                        {
                            tiles[row, col] = null;
                            flipped[row, col] = false;
                            mirrored[row, col] = false;
                            upLayer[row, col] = false;
                        }
            }

            public void SetTile(int row, int col, Tile tile, int subPalette, bool flipped = false, bool mirrored = false, bool upLayer = false) => SetTile(new Cell(row, col), tile, subPalette, flipped, mirrored, upLayer);

            public void SetTile(Cell cell, Tile tile, int subPalette, bool flipped = false, bool mirrored = false, bool upLayer = false)
            {
                tiles[cell.Row, cell.Col] = tile;
                this.subPalette[cell.Row, cell.Col] = subPalette;
                this.flipped[cell.Row, cell.Col] = flipped;
                this.mirrored[cell.Row, cell.Col] = mirrored;
                this.upLayer[cell.Row, cell.Col] = upLayer;
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            var app = new MainClass();
            app.Run();
        }

        private MMXCore core;
        private Texture2D tileset;
        private Texture2D tilemap;
        private Texture1D palette;
        private Tile[] tiles;
        private Map[] maps;

        private int Transform(int color, bool notTransparent) => !notTransparent ? 0 : (int) (((color & 0x1F) << 3) | ((color & 0x3E0) << 6) | ((color & 0x7C00) << 9) | 0xFF000000);

        private Tile AddTile(D3D11Device device, uint tile, bool transparent = false)
        {
            uint tileNum = tile & 0x3FF;
            uint image = tileNum << 6;

            byte[] imageData = new byte[TILE_SIZE * TILE_SIZE * sizeof(byte)];
            bool notNull = false;
            using (var ms = new MemoryStream(imageData))
            {
                using var writter = new BinaryWriter(ms);
                for (int i = 0; i < TILE_SIZE * TILE_SIZE; i++, image++)
                {
                    var v = core.vramCache[image];
                    bool notTransparent = v != 0 || !transparent;
                    notNull |= notTransparent;
                    writter.Write(v);
                }
            }

            //if (!notNull)
            //    return null;

            var wtile = new Tile(device, (int) tileNum, imageData);
            return wtile;
        }

        private void WriteTile(DataRectangle tilesetRect, byte[] data, int tileRow, int tileCol)
        {
            IntPtr ptr = tilesetRect.DataPointer;
            ptr += TILE_SIZE * tileCol * TILESET_COLOR_SIZE;
            ptr += TILESET_WIDTH * TILE_SIZE * tileRow * TILESET_COLOR_SIZE;

            for (int row = 0; row < TILE_SIZE; row++, ptr += TILESET_WIDTH * TILESET_COLOR_SIZE)
            {
                int dataIndex = row * TILE_SIZE;

                using var stream = new DataStream(ptr, TILE_SIZE * TILESET_COLOR_SIZE, true, true);
                for (int col = 0; col < TILE_SIZE; col++)
                    stream.Write((byte) (data != null ? data[dataIndex++] : 0));
            }
        }

        private void WriteTileInMap(DataRectangle tilemapRect, byte[] data, int mapIndex, int tileRow, int tileCol, int subPalette)
        {
            int mapRow = mapIndex / 32;
            int mapCol = mapIndex % 32;

            IntPtr ptr = tilemapRect.DataPointer;
            ptr += mapCol * MAP_SIZE * TILEMAP_COLOR_SIZE;
            ptr += TILEMAP_WIDTH * mapRow * MAP_SIZE * TILEMAP_COLOR_SIZE;
            ptr += TILE_SIZE * tileCol * TILEMAP_COLOR_SIZE;
            ptr += TILEMAP_WIDTH * TILE_SIZE * tileRow * TILEMAP_COLOR_SIZE;

            for (int row = 0; row < TILE_SIZE; row++, ptr += TILEMAP_WIDTH * TILEMAP_COLOR_SIZE)
            {
                int dataIndex = row * TILE_SIZE;

                using var stream = new DataStream(ptr, TILE_SIZE * TILEMAP_COLOR_SIZE, true, true);
                for (int col = 0; col < TILE_SIZE; col++)
                    stream.Write((ushort) ((subPalette << 8) | (data != null ? data[dataIndex++] : 0)));
            }
        }

        private void LoadTileset(D3D11Device device)
        {
            var description = new Texture2DDescription
            {
                Width = TILESET_WIDTH,
                Height = TILESET_HEIGHT,
                Usage = ResourceUsage.Default,
                Format = Format.P8
            };

            DataRectangle rect = new DataRectangle();
            tiles = new Tile[0x400];

            for (int tileIndex = 0; tileIndex < 0x400; tileIndex++)
            {
                Tile tile = AddTile(device, (uint) tileIndex, true);
                int row = tileIndex / (TILESET_WIDTH / TILE_SIZE);
                int col = tileIndex % (TILESET_WIDTH / TILE_SIZE);
                WriteTile(rect, tile?.data, row, col);
                tiles[tileIndex] = tile;
            }

            tileset = new Texture2D(device, description, rect);
        }

        private void LoadTilemap(D3D11Device device)
        {
            var description = new Texture2DDescription
            {
                Width = TILEMAP_WIDTH,
                Height = TILEMAP_HEIGHT,
                Usage = ResourceUsage.Default,
                Format = Format.A8P8
            };

            DataRectangle rect = new DataRectangle();

            maps = new Map[0x400];

            uint map = core.pMaps;
            for (int i = 0; i < core.numMaps; i++)
            {
                byte colisionByte = core.rom[core.pCollisions + i];
                var collisionData = (CollisionData) colisionByte;
                var wmap = new Map(i, collisionData);

                uint tileData = core.ReadWord(map);
                uint tileNum = tileData & 0x3FF;
                byte subPalette = (byte) ((tileData >> 6) & 0x70);
                bool flipped = (tileData & 0x8000) != 0;
                bool mirrored = (tileData & 0x4000) != 0;
                bool upLayer = (tileData & 0x2000) != 0;
                map += 2;
                Tile tile = tiles[tileNum];
                wmap.SetTile(0, 0, tile, subPalette, flipped, mirrored, upLayer);
                WriteTileInMap(rect, tile?.data, i, 0, 0, subPalette);

                tileData = core.ReadWord(map);
                tileNum = tileData & 0x3FF;
                subPalette = (byte) ((tileData >> 6) & 0x70);
                flipped = (tileData & 0x8000) != 0;
                mirrored = (tileData & 0x4000) != 0;
                upLayer = (tileData & 0x2000) != 0;
                map += 2;
                tile = tiles[tileNum];
                wmap.SetTile(0, 1, tile, subPalette, flipped, mirrored, upLayer);
                WriteTileInMap(rect, tile?.data, i, 0, 1, subPalette);

                tileData = core.ReadWord(map);
                tileNum = tileData & 0x3FF;
                subPalette = (byte) ((tileData >> 6) & 0x70);
                flipped = (tileData & 0x8000) != 0;
                mirrored = (tileData & 0x4000) != 0;
                upLayer = (tileData & 0x2000) != 0;
                map += 2;
                tile = tiles[tileNum];
                wmap.SetTile(1, 0, tile, subPalette, flipped, mirrored, upLayer);
                WriteTileInMap(rect, tile?.data, i, 1, 0, subPalette);

                tileData = core.ReadWord(map);
                tileNum = tileData & 0x3FF;
                subPalette = (byte) ((tileData >> 6) & 0x70);
                flipped = (tileData & 0x8000) != 0;
                mirrored = (tileData & 0x4000) != 0;
                upLayer = (tileData & 0x2000) != 0;
                map += 2;
                tile = tiles[tileNum];
                wmap.SetTile(1, 1, tile, subPalette, flipped, mirrored, upLayer);
                WriteTileInMap(rect, tile?.data, i, 1, 1, subPalette);

                maps[i] = wmap.IsNull ? null : wmap;
            }

            tilemap = new Texture2D(device, description, rect);
        }

        /// <summary>
        /// Loads a bitmap using WIC.
        /// </summary>
        /// <param name="deviceManager"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static SharpDX.WIC.BitmapSource LoadBitmap(SharpDX.WIC.ImagingFactory2 factory, string filename)
        {
            var bitmapDecoder = new SharpDX.WIC.BitmapDecoder(
                factory,
                filename,
                SharpDX.WIC.DecodeOptions.CacheOnDemand
                );

            var formatConverter = new SharpDX.WIC.FormatConverter(factory);

            formatConverter.Initialize(
                bitmapDecoder.GetFrame(0),
                SharpDX.WIC.PixelFormat.Format32bppPRGBA,
                SharpDX.WIC.BitmapDitherType.None,
                null,
                0.0,
                SharpDX.WIC.BitmapPaletteType.Custom);

            return formatConverter;
        }

        /// <summary>
        /// Creates a <see cref="Texture1D"/> from a WIC <see cref="SharpDX.WIC.BitmapSource"/>
        /// </summary>
        /// <param name="device">The Direct3D11 device</param>
        /// <param name="bitmapSource">The WIC bitmap source</param>
        /// <returns>A Texture2D</returns>
        public static Texture1D CreateTexture1DFromBitmap(D3D11Device device, SharpDX.WIC.BitmapSource bitmapSource)
        {
            // Allocate DataStream to receive the WIC image pixels
            int stride = bitmapSource.Size.Width * 4;
            using var buffer = new DataStream(stride, true, true);
            // Copy the content of the WIC to the buffer
            bitmapSource.CopyPixels(stride, buffer);
            return new Texture1D(device, new Texture1DDescription()
            {
                Width = bitmapSource.Size.Width,
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource,
                Usage = ResourceUsage.Immutable,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.R8G8B8A8_UNorm,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
            }, new DataRectangle(buffer.DataPointer, stride));
        }

        /// <summary>
        /// Creates a <see cref="Texture2D"/> from a WIC <see cref="SharpDX.WIC.BitmapSource"/>
        /// </summary>
        /// <param name="device">The Direct3D11 device</param>
        /// <param name="bitmapSource">The WIC bitmap source</param>
        /// <returns>A Texture2D</returns>
        public static Texture2D CreateTexture2DFromBitmap(D3D11Device device, SharpDX.WIC.BitmapSource bitmapSource)
        {
            // Allocate DataStream to receive the WIC image pixels
            int stride = bitmapSource.Size.Width * 4;
            using var buffer = new DataStream(bitmapSource.Size.Height * stride, true, true);
            // Copy the content of the WIC to the buffer
            bitmapSource.CopyPixels(stride, buffer);
            return new Texture2D(device, new Texture2DDescription()
            {
                Width = bitmapSource.Size.Width,
                Height = bitmapSource.Size.Height,
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource,
                Usage = ResourceUsage.Immutable,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.R8G8B8A8_UNorm,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
            }, new DataRectangle(buffer.DataPointer, stride));
        }

        private void LoadPalette(D3D11Device device)
        {
            palette = new Texture1D(device, 256, 1, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);
            DataRectangle rect = palette.LockRectangle(0, LockFlags.Discard);

            using (var stream = new DataStream(rect.DataPointer, 256 * 1 * sizeof(int), true, true))
            {
                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 16; j++)
                        stream.Write(new Color(Transform(core.palCache[(i << 4) | j], j != 0)).ToRgba());
            }

            palette.UnlockRectangle(0);
        }

        private static void WriteTriangle(DataStream vbData, Vector2 r0, Vector2 r1, Vector2 r2, Vector2 t0, Vector2 t1, Vector2 t2)
        {
            WriteVertex(vbData, r0.X, r0.Y, t0.X, t0.Y);
            WriteVertex(vbData, r1.X, r1.Y, t1.X, t1.Y);
            WriteVertex(vbData, r2.X, r2.Y, t2.X, t2.Y);
        }

        private static void WriteTile(DataStream vbData, Vector2 vSource, Vector2 vDest)
        {
            if (vSource.X < 0 || vSource.X > 1 || vSource.Y < 0 || vSource.Y > 1)
                throw new Exception();

            var r0 = new Vector2(vDest.X, vDest.Y);
            var r1 = new Vector2(vDest.X + TILE_SIZE, vDest.Y);
            var r2 = new Vector2(vDest.X + TILE_SIZE, vDest.Y - TILE_SIZE);
            var r3 = new Vector2(vDest.X, vDest.Y - TILE_SIZE);

            var t0 = new Vector2(vSource.X, vSource.Y);
            var t1 = new Vector2(vSource.X + TILE_FRAC_SIZE_IN_TILESET, vSource.Y);
            var t2 = new Vector2(vSource.X + TILE_FRAC_SIZE_IN_TILESET, vSource.Y + TILE_FRAC_SIZE_IN_TILESET);
            var t3 = new Vector2(vSource.X, vSource.Y + TILE_FRAC_SIZE_IN_TILESET);

            if (t0.X < 0 || t0.X > 1 || t0.Y < 0 || t0.Y > 1)
                throw new Exception();

            if (t1.X < 0 || t1.X > 1 || t1.Y < 0 || t1.Y > 1)
                throw new Exception();

            if (t2.X < 0 || t2.X > 1 || t2.Y < 0 || t2.Y > 1)
                throw new Exception();

            if (t3.X < 0 || t3.X > 1 || t3.Y < 0 || t3.Y > 1)
                throw new Exception();

            WriteTriangle(vbData, r0, r1, r2, t0, t1, t2);
            WriteTriangle(vbData, r0, r2, r3, t0, t2, t3);
        }

        private static void WriteTileInMap(DataStream vbData, Vector2 vSource, Vector2 vDest, bool flipped, bool mirrored)
        {
            if (vSource.X < 0 || vSource.X > 1 || vSource.Y < 0 || vSource.Y > 1)
                throw new Exception();

            var r0 = new Vector2(vDest.X, vDest.Y);
            var r1 = new Vector2(vDest.X + TILE_SIZE, vDest.Y);
            var r2 = new Vector2(vDest.X + TILE_SIZE, vDest.Y - TILE_SIZE);
            var r3 = new Vector2(vDest.X, vDest.Y - TILE_SIZE);

            var t0 = new Vector2(vSource.X, vSource.Y);
            var t1 = new Vector2(vSource.X + TILE_FRAC_SIZE_IN_TILEMAP, vSource.Y);
            var t2 = new Vector2(vSource.X + TILE_FRAC_SIZE_IN_TILEMAP, vSource.Y + TILE_FRAC_SIZE_IN_TILEMAP);
            var t3 = new Vector2(vSource.X, vSource.Y + TILE_FRAC_SIZE_IN_TILEMAP);

            if (t0.X < 0 || t0.X > 1 || t0.Y < 0 || t0.Y > 1)
                throw new Exception();

            if (t1.X < 0 || t1.X > 1 || t1.Y < 0 || t1.Y > 1)
                throw new Exception();

            if (t2.X < 0 || t2.X > 1 || t2.Y < 0 || t2.Y > 1)
                throw new Exception();

            if (t3.X < 0 || t3.X > 1 || t3.Y < 0 || t3.Y > 1)
                throw new Exception();

            if (flipped)
            {
                if (mirrored)
                {
                    WriteTriangle(vbData, r0, r1, r2, t2, t3, t0);
                    WriteTriangle(vbData, r0, r2, r3, t2, t0, t1);
                }
                else
                {
                    WriteTriangle(vbData, r0, r1, r2, t3, t2, t1);
                    WriteTriangle(vbData, r0, r2, r3, t3, t1, t0);
                }
            }
            else if (mirrored)
            {
                WriteTriangle(vbData, r0, r1, r2, t1, t0, t3);
                WriteTriangle(vbData, r0, r2, r3, t1, t3, t2);
            }
            else
            {
                WriteTriangle(vbData, r0, r1, r2, t0, t1, t2);
                WriteTriangle(vbData, r0, r2, r3, t0, t2, t3);
            }
        }

        private void TessellateTiles(Buffer tilesetVB, Buffer tileMapVB)
        {
            DataStream tilesetVBData = tilesetVB.Lock(0, 4 * VERTEX_SIZE, LockFlags.None);

            for (int row = 0; row < TILES_PER_ROW_IN_IMAGE; row++)
                for (int col = 0; col < TILES_PER_COL_IN_IMAGE; col++)
                {
                    int tileIndex = TILES_PER_COL_IN_IMAGE * row + col;
                    Tile tile = tiles[tileIndex];
                    var tilePos = new Vector2(col * TILE_SIZE, -row * TILE_SIZE);

                    if (tile != null)
                    {
                        int tileRow = tileIndex / (TILESET_WIDTH / TILE_SIZE);
                        int tileCol = tileIndex % (TILESET_WIDTH / TILE_SIZE);
                        var tilesetPos = new Vector2((float) tileCol * TILE_SIZE / TILESET_WIDTH, (float) tileRow * TILE_SIZE / TILESET_HEIGHT);
                        WriteTile(tilesetVBData, tilesetPos, tilePos);
                    }
                    else
                        WriteTile(tilesetVBData, Vector2.Zero, tilePos);
                }

            tilesetVB.Unlock();

            DataStream tilemapVBData = tileMapVB.Lock(0, 4 * VERTEX_SIZE, LockFlags.None);

            for (int row = 0; row < MAPS_PER_ROW_IN_IMAGE; row++)
                for (int col = 0; col < MAPS_PER_COL_IN_IMAGE; col++)
                {
                    int mapIndex = MAPS_PER_COL_IN_IMAGE * row + col;
                    Map map = maps[mapIndex];
                    var mapPos = new Vector2(col * MAP_SIZE, -row * MAP_SIZE);

                    if (map != null)
                    {
                        for (int tileRow = 0; tileRow < SIDE_TILES_PER_MAP; tileRow++)
                            for (int tileCol = 0; tileCol < SIDE_TILES_PER_MAP; tileCol++)
                            {
                                Tile tile = map.tiles[tileRow, tileCol];
                                var tilePos = new Vector2(mapPos.X + tileCol * TILE_SIZE, mapPos.Y - tileRow * TILE_SIZE);

                                if (tile != null)
                                {
                                    var tilemapPos = new Vector2((float) (mapIndex % 32 * MAP_SIZE + tileCol * TILE_SIZE) / TILEMAP_WIDTH, (float) (mapIndex / 32 * MAP_SIZE + tileRow * TILE_SIZE) / TILEMAP_HEIGHT);
                                    WriteTileInMap(tilemapVBData, tilemapPos, tilePos, map.flipped[tileRow, tileCol], map.mirrored[tileRow, tileCol]);
                                }
                                else
                                    WriteTileInMap(tilemapVBData, Vector2.Zero, tilePos, false, false);
                            }
                    }
                    else
                    {
                        for (int tileRow = 0; tileRow < SIDE_TILES_PER_MAP; tileRow++)
                            for (int tileCol = 0; tileCol < SIDE_TILES_PER_MAP; tileCol++)
                            {
                                var tilePos = new Vector2(mapPos.X + tileCol * TILE_SIZE, mapPos.Y - tileRow * TILE_SIZE);
                                WriteTileInMap(tilemapVBData, Vector2.Zero, tilePos, false, false);
                            }
                    }
                }

            tileMapVB.Unlock();
        }

        private void RenderAllTiles(D3D11Device device, Buffer vb, RectangleF rDest)
        {
            device.SetStreamSource(0, vb, 0, VERTEX_SIZE);

            float x = rDest.Left - SCREEN_WIDTH * 0.5f;
            float y = -rDest.Top + SCREEN_HEIGHT * 0.5f;

            var matScaling = Matrix.Scaling(1, 1, 1);
            var matTranslation = Matrix.Translation(x, y, 0);
            Matrix matTransform = matScaling * matTranslation;

            device.SetTransform(TransformState.World, matTransform);
            device.SetTexture(0, tileset);
            device.SetTexture(1, palette);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2 * TILES_PER_COL_IN_IMAGE * TILES_PER_ROW_IN_IMAGE);
        }

        private void RenderAllTiles(D3D11Device device, Buffer vb) => RenderAllTiles(device, vb, new RectangleF(TILEMAP_IMAGE_WIDTH, 0, TILESET_IMAGE_WIDTH, TILESET_IMAGE_HEIGHT));

        private void RenderAllMaps(D3D11Device device, Buffer vb, RectangleF rDest)
        {
            device.SetStreamSource(0, vb, 0, VERTEX_SIZE);

            float x = rDest.Left - SCREEN_WIDTH * 0.5f;
            float y = -rDest.Top + SCREEN_HEIGHT * 0.5f;

            var matScaling = Matrix.Scaling(1, 1, 1);
            var matTranslation = Matrix.Translation(x, y, 0);
            Matrix matTransform = matScaling * matTranslation;

            device.SetTransform(TransformState.World, matTransform);
            device.SetTexture(0, tilemap);
            device.SetTexture(1, palette);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2 * MAPS_PER_COL_IN_IMAGE * SIDE_TILES_PER_MAP * MAPS_PER_ROW_IN_IMAGE * SIDE_TILES_PER_MAP);
        }

        private void RenderAllMaps(D3D11Device device, Buffer vb) => RenderAllMaps(device, vb, new RectangleF(0, 0, TILEMAP_IMAGE_WIDTH, TILEMAP_IMAGE_HEIGHT));

        public void Run()
        {
            var form = new RenderForm("D3D9 Test")
            {
                ClientSize = new Size(SCREEN_WIDTH * SCREEN_SCALE, SCREEN_HEIGHT * SCREEN_SCALE)
            };

            // Declare the device and swapChain vars
            D3D11Device device;
            SwapChain swapChain;

            #region Direct3D Initialization
            // Create the device and swapchain
            D3D11Device.CreateWithSwapChain(
                SharpDX.Direct3D.DriverType.Hardware,
                DeviceCreationFlags.None,
                new[] {
                    SharpDX.Direct3D.FeatureLevel.Level_11_1,
                    SharpDX.Direct3D.FeatureLevel.Level_11_0,
                    SharpDX.Direct3D.FeatureLevel.Level_10_1,
                    SharpDX.Direct3D.FeatureLevel.Level_10_0,
                },
                new SwapChainDescription()
                {
                    ModeDescription =
                        new ModeDescription(
                            SCREEN_WIDTH,
                            SCREEN_HEIGHT,
                            new Rational(60, 1),
                            Format.R8G8B8A8_UNorm
                        ),
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = Usage.BackBuffer | Usage.RenderTargetOutput,
                    BufferCount = 1,
                    Flags = SwapChainFlags.None,
                    IsWindowed = true,
                    OutputHandle = form.Handle,
                    SwapEffect = SwapEffect.Discard,
                },
                out device, out swapChain
            );

            var context = device.ImmediateContext;

            // Create references for backBuffer and renderTargetView
            var backBuffer = SharpDX.Direct3D11.Resource.FromSwapChain<Texture2D>(swapChain, 0);
            var target = new RenderTargetView(device, backBuffer);

            #endregion

            ShaderBytecode function;

            function = ShaderBytecode.CompileFromFile("PaletteShader.hlsl", "main", "ps_2_0"); // new ShaderBytecode(PIXEL_SHADER_BYTECODE);            
            var pShader = new PixelShader(device, function);

            context.PixelShader.Set(pShader);

            var tilesetVB = Buffer.Create(device, BindFlags.VertexBuffer, VERTEX_SIZE * 2 * 3 * TILES_PER_COL_IN_IMAGE * TILES_PER_ROW_IN_IMAGE, Usage.WriteOnly, D3DFVF_TLVERTEX, Pool.Managed);
            var tilemapVB = Buffer.Create(device, VERTEX_SIZE * 2 * 3 * MAPS_PER_COL_IN_IMAGE * SIDE_TILES_PER_MAP * MAPS_PER_ROW_IN_IMAGE * SIDE_TILES_PER_MAP, Usage.WriteOnly, D3DFVF_TLVERTEX, Pool.Managed);

            device.SetRenderState(RenderState.Lighting, false);
            device.SetRenderState(RenderState.AlphaBlendEnable, true);
            device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            device.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);

            core = new MMXCore();
            core.LoadNewRom(Assembly.GetExecutingAssembly().GetManifestResourceStream("D3D11Test.roms." + ROM_NAME));
            core.Init();

            if (core.CheckROM() != 0)
            {
                core.LoadFont();
                core.LoadProperties();

                core.SetLevel(LEVEL, 0);
                core.LoadLevel();

                core.LoadBackground();

                core.UpdateVRAMCache();

                LoadPalette(device);
                LoadTileset(device);
                LoadTilemap(device);
                TessellateTiles(tilesetVB, tilemapVB);
            }

            // Use clock
            var clock = new Stopwatch();
            clock.Start();

            #region Render loop
            // Create and run the render loop
            RenderLoop.Run(form, () =>
            {
                var time = clock.ElapsedMilliseconds / 1000.0f;

                // Clear the render target with light blue
                device.ImmediateContext.ClearRenderTargetView(
                  target,
                  Color.LightBlue);

                var orthoLH = Matrix.OrthoLH(SCREEN_WIDTH, SCREEN_HEIGHT, 1.0f, 10.0f);
                device.SetTransform(TransformState.Projection, orthoLH);
                device.SetTransform(TransformState.World, Matrix.Identity);
                device.SetTransform(TransformState.View, Matrix.Identity);

                RenderAllTiles(device, tilesetVB);
                RenderAllMaps(device, tilemapVB);

                // Present the frame
                swapChain.Present(0, PresentFlags.None);
            });
            #endregion

            #region Direct3D Cleanup
            target.Dispose();
            backBuffer.Dispose();
            device.Dispose();
            swapChain.Dispose();
            #endregion
        }
    }
}