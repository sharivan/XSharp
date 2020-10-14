using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using SharpDX.Windows;

using MMX.ROM;

using Color = SharpDX.Color;
using System.Reflection;

namespace D3D9Test
{
    public class MainClass
    {
        private const int TILE_SIZE = 8;
        private const int MAP_SIZE = 2 * TILE_SIZE;
        private const int SIDE_TILES_PER_MAP = 2;

        private static readonly string ROM_NAME = "Mega Man X (U) (V1.0) [!]";
        private const int LEVEL = 0;

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

        private struct Cell
        {
            private int row;
            private int col;

            public int Row
            {
                get
                {
                    return row;
                }
            }

            public int Col
            {
                get
                {
                    return col;
                }
            }

            public Cell(int row, int col)
            {
                this.row = row;
                this.col = col;
            }

            public override int GetHashCode()
            {
                return 65536 * row + col;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                if (!(obj is Cell))
                    return false;

                Cell other = (Cell) obj;
                return other.row == row && other.col == col;
            }

            public override string ToString()
            {
                return row + "," + col;
            }
        }

        private class Tile
        {
            internal readonly int id;
            internal byte[] data;
            internal Texture tex;

            internal Tile(Device device, int id, byte[] data)
            {
                this.id = id;
                this.data = data;

                tex = new Texture(device, TILE_SIZE, TILE_SIZE, 1, Usage.None, Format.L8, Pool.Managed);
                DataRectangle rect = tex.LockRectangle(0, LockFlags.Discard);
                Marshal.Copy(data, 0, rect.DataPointer, TILE_SIZE * TILE_SIZE * sizeof(byte));
                tex.UnlockRectangle(0);
            }
        }

        private const VertexFormat D3DFVF_TLVERTEX = VertexFormat.PositionRhw | VertexFormat.Diffuse | VertexFormat.Texture1;

        private static void WriteVertex(DataStream vbData, float x, float y, float z, float rhw, Color4 color, float u, float v)
        {
            vbData.Write(x);
            vbData.Write(y);
            vbData.Write(z);
            vbData.Write(rhw);
            vbData.Write(color.ToRgba());
            vbData.Write(u);
            vbData.Write(v);
        }

        private static void BlitD3D(Device device, VertexBuffer vb, Texture texture, Texture palette, SharpDX.Rectangle rDest, Color4 vertexColor, bool flipped, bool mirrored)
        {
            DataStream vbData = vb.Lock(0, 0, LockFlags.Discard);

            if (flipped)
            {
                if (mirrored)
                {
                    WriteVertex(vbData, rDest.Left - 0.5f, rDest.Top - 0.5f, 0, 1, vertexColor, 1, 1);
                    WriteVertex(vbData, rDest.Right - 0.5f, rDest.Top - 0.5f, 0, 1, vertexColor, 0, 1);
                    WriteVertex(vbData, rDest.Right - 0.5f, rDest.Bottom - 0.5f, 0, 1, vertexColor, 0, 0);
                    WriteVertex(vbData, rDest.Left - 0.5f, rDest.Bottom - 0.5f, 0, 1, vertexColor, 1, 0);
                }
                else
                {
                    WriteVertex(vbData, rDest.Left - 0.5f, rDest.Top - 0.5f, 0, 1, vertexColor, 0, 1);
                    WriteVertex(vbData, rDest.Right - 0.5f, rDest.Top - 0.5f, 0, 1, vertexColor, 1, 1);
                    WriteVertex(vbData, rDest.Right - 0.5f, rDest.Bottom - 0.5f, 0, 1, vertexColor, 1, 0);
                    WriteVertex(vbData, rDest.Left - 0.5f, rDest.Bottom - 0.5f, 0, 1, vertexColor, 0, 0);
                }
            }
            else if (mirrored)
            {
                WriteVertex(vbData, rDest.Left - 0.5f, rDest.Top - 0.5f, 0, 1, vertexColor, 1, 0);
                WriteVertex(vbData, rDest.Right - 0.5f, rDest.Top - 0.5f, 0, 1, vertexColor, 0, 0);
                WriteVertex(vbData, rDest.Right - 0.5f, rDest.Bottom - 0.5f, 0, 1, vertexColor, 0, 1);
                WriteVertex(vbData, rDest.Left - 0.5f, rDest.Bottom - 0.5f, 0, 1, vertexColor, 1, 1);
            }
            else
            {
                WriteVertex(vbData, rDest.Left - 0.5f, rDest.Top - 0.5f, 0, 1, vertexColor, 0, 0);
                WriteVertex(vbData, rDest.Right - 0.5f, rDest.Top - 0.5f, 0, 1, vertexColor, 1, 0);
                WriteVertex(vbData, rDest.Right - 0.5f, rDest.Bottom - 0.5f, 0, 1, vertexColor, 1, 1);
                WriteVertex(vbData, rDest.Left - 0.5f, rDest.Bottom - 0.5f, 0, 1, vertexColor, 0, 1);
            }

            vb.Unlock();

            device.SetTexture(0, texture);
            device.SetTexture(1, palette);
            device.DrawPrimitives(PrimitiveType.TriangleFan, 0, 2);
        }

        private class Map
        {
            internal int id;
            internal Texture[] paletteTextures;
            internal CollisionData collisionData;

            internal Tile[,] tiles;           
            internal int[,] palette;
            internal bool[,] flipped;
            internal bool[,] mirrored;
            internal bool[,] upLayer;

            public Tile this[int row, int col]
            {
                get
                {
                    return tiles[row, col];
                }

                set
                {
                    tiles[row, col] = value;
                }
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

            internal Map(int id, Texture[] paletteTextures, CollisionData collisionData = CollisionData.NONE)
            {
                this.id = id;
                this.paletteTextures = paletteTextures;
                this.collisionData = collisionData;

                tiles = new Tile[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
                palette = new int[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
                flipped = new bool[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
                mirrored = new bool[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
                upLayer = new bool[SIDE_TILES_PER_MAP, SIDE_TILES_PER_MAP];
            }

            private void PaintTile(Device device, VertexBuffer vb, Tile tile, int x, int y, int palette, bool flipped, bool mirrored)
            {               
                BlitD3D(device, vb, tile.tex, paletteTextures[palette], new SharpDX.Rectangle(x, y, TILE_SIZE, TILE_SIZE), new Color4(0xFFFFFFFF), flipped, mirrored);
            }

            internal void PaintDownLayer(Device device, VertexBuffer vb, int x, int y)
            {
                for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
                    for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
                    {
                        Tile tile = tiles[row, col];
                        if (tile != null && !upLayer[row, col])
                            PaintTile(device, vb, tile, x + col * TILE_SIZE, y + row * TILE_SIZE, palette[row, col], flipped[row, col], mirrored[row, col]);
                    }
            }

            internal void PaintUpLayer(Device device, VertexBuffer vb, int x, int y)
            {
                for (int col = 0; col < SIDE_TILES_PER_MAP; col++)
                    for (int row = 0; row < SIDE_TILES_PER_MAP; row++)
                    {
                        Tile tile = tiles[row, col];
                        if (tile != null && upLayer[row, col])
                            PaintTile(device, vb, tile, x + col * TILE_SIZE, y + row * TILE_SIZE, palette[row, col], flipped[row, col], mirrored[row, col]);
                    }
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

            public void SetTile(int row, int col, Tile tile, int palette, bool flipped = false, bool mirrored = false, bool upLayer = false)
            {
                SetTile(new Cell(row, col), tile, palette, flipped, mirrored, upLayer);
            }

            public void SetTile(Cell cell, Tile tile, int palette, bool flipped = false, bool mirrored = false, bool upLayer = false)
            {
                tiles[cell.Row, cell.Col] = tile;
                this.palette[cell.Row, cell.Col] = palette;
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

        private static readonly byte[] g_ps20_main = new byte[]
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
            127,  65,   0,   0,   0,  61,
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

        private static readonly byte[] bytes = new byte[]
        {
            0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
            5, 5, 6, 6, 6, 6, 5, 5, 5, 5, 6, 6, 6, 6, 5, 5, 5, 5, 6, 6, 6, 6, 5, 5, 5, 5, 6, 6, 6, 6, 5, 5,
            7, 8, 9, 10, 10, 9, 8, 7, 7, 8, 9, 10, 10, 9, 8, 7, 7, 8, 9, 10, 10, 9, 8, 7, 7, 8, 9, 10, 10, 9, 8, 7,
            10, 10, 11, 11, 11, 11, 10, 10, 10, 10, 11, 11, 11, 11, 10, 10, 10, 10, 11, 11, 11, 11, 10, 10, 10, 10, 11, 11, 11, 11, 10, 10,
            2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
            1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
            12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12,
            2, 2, 12, 2, 2, 12, 2, 2, 2, 2, 12, 2, 2, 12, 2, 2, 2, 2, 12, 2, 2, 12, 2, 2, 2, 2, 12, 2, 2, 12, 2, 2,
            13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13,
            12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12,
            14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 1, 1, 15, 14, 14,
            0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0,
            0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0
        };

        private MMXCore core;
        private Texture[] paletteTextures = new Texture[8];
        private Map[] maps;

        private int Transform(int color, bool notTransparent)
        {
            return !notTransparent ? 0 : (int) (((color & 0x1F) << 3) | ((color & 0x3E0) << 6) | ((color & 0x7C00) << 9) | 0xFF000000);
        }

        private Tile AddTile(Device device, uint tile, bool transparent = false)
        {           
            uint image = (tile & 0x3FF) << 6;

            byte[] imageData = new byte[TILE_SIZE * TILE_SIZE * sizeof(byte)];
            bool notNull = false;
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                using (BinaryWriter writter = new BinaryWriter(ms))
                {
                    for (int i = 0; i < TILE_SIZE * TILE_SIZE; i++, image++)
                    {
                        var v = core.vramCache[image];
                        bool notTransparent = v != 0 || !transparent;
                        notNull |= notTransparent;
                        writter.Write(v);
                    }
                }
            }

            if (!notNull)
                return null;

            Tile wtile = new Tile(device, (int) tile, imageData);
            return wtile;
        }

        private void RefreshMapCache(Device device)
        {
            maps = new Map[0x400];

            uint map = core.pMaps;
            /* I didn't write this function, but basically the above loses a lot of data because size of a WORD is max 65535 and pMaps is a DWORD */
            for (int i = 0; i < 0x400; i++)
            {
                byte colisionByte = core.rom[core.pCollisions + i];
                CollisionData collisionData = (CollisionData) colisionByte;
                Map wmap = new Map(i, paletteTextures, collisionData);

                uint tileData = core.ReadWord(map);
                byte palette = (byte) ((tileData >> 10) & 7);
                map += 2;
                Tile tile = AddTile(device, tileData, true);
                wmap.SetTile(0, 0, tile, palette, (tileData & 0x8000) != 0, (tileData & 0x4000) != 0, (tileData & 0x2000) != 0);

                tileData = core.ReadWord(map);
                palette = (byte) ((tileData >> 10) & 7);
                map += 2;
                tile = AddTile(device, tileData, true);
                wmap.SetTile(0, 1, tile, palette, (tileData & 0x8000) != 0, (tileData & 0x4000) != 0, (tileData & 0x2000) != 0);

                tileData = core.ReadWord(map);
                palette = (byte) ((tileData >> 10) & 7);
                map += 2;
                tile = AddTile(device, tileData, true);
                wmap.SetTile(1, 0, tile, palette, (tileData & 0x8000) != 0, (tileData & 0x4000) != 0, (tileData & 0x2000) != 0);

                tileData = core.ReadWord(map);
                palette = (byte) ((tileData >> 10) & 7);
                map += 2;
                tile = AddTile(device, tileData, true);
                wmap.SetTile(1, 1, tile, palette, (tileData & 0x8000) != 0, (tileData & 0x4000) != 0, (tileData & 0x2000) != 0);

                maps[i] = wmap.IsNull ? null : wmap;
            }
        }

        private void LoadPalette(Device device)
        {
            for (int i = 0; i < 8; i++)
            {
                Texture paletteTexture = new Texture(device, 16, 1, 1, Usage.Dynamic, Format.X8R8G8B8, Pool.Default);

                DataRectangle rect = paletteTexture.LockRectangle(0, LockFlags.Discard);
                using (DataStream stream = new DataStream(rect.DataPointer, 16 * 1 * sizeof(int), true, true))
                {
                    for (int j = 0; j < 16; j++)
                        stream.Write(new Color(Transform(core.palCache[(i << 4) | j], j != 0)).ToRgba());
                }

                paletteTexture.UnlockRectangle(0);

                paletteTextures[i] = paletteTexture;
            }
        }

        private void RenderAllMaps(Device device, VertexBuffer vb)
        {
            for (int row = 0; row < 64; row++)
                for (int col = 0; col < 16; col++)
                {
                    int index = 16 * row + col;
                    Map map = maps[index];
                    if (map == null)
                        continue;

                    int x = MAP_SIZE * col;
                    int y = MAP_SIZE * row;

                    map.PaintDownLayer(device, vb, x, y);
                    map.PaintUpLayer(device, vb, x, y);
                }
        }

        /*private void LoadMap(int x, int y, ushort index, bool background = false)
        {
            if (index < maps.Length)
            {
                Map map = maps[index];
                if (map != null)
                    world.SetMap(new Vector(x * MAP_SIZE, y * MAP_SIZE), map, background);
            }
        }

        private void LoadSceneEx(int x, int y, ushort index, bool background = false)
        {
            x <<= 4;
            y <<= 4;
            uint pmap = (uint) (index << 8);
            for (int iy = 0; iy < 16; iy++)
                for (int ix = 0; ix < 16; ix++)
                {
                    LoadMap(x + ix, y + iy, core.mapping[pmap], background);
                    pmap++;
                }
        }

        public void LoadToWorld(bool background = false)
        {
            world.BeginUpdate();
            world.Resize(core.levelHeight, core.levelWidth, background);
            RefreshMapCache();

            uint tmpLayout = 0;
            for (int y = 0; y < core.levelHeight; y++)
                for (int x = 0; x < core.levelWidth; x++)
                    LoadSceneEx(x, y, core.sceneLayout[tmpLayout++], background);

            world.EndUpdate();
        }*/

        public void Run()
        {
            const int width = 16 * MAP_SIZE;
            const int height = 16 * MAP_SIZE;

            var form = new RenderForm("SharpDX - MiniCube Direct3D9 Sample")
            {
                ClientSize = new Size(width, height)
            };

            // Creates the Device
            var direct3D = new Direct3D();
            var device = new Device(direct3D, 0, DeviceType.Hardware, form.Handle, CreateFlags.HardwareVertexProcessing | CreateFlags.FpuPreserve | CreateFlags.Multithreaded, new PresentParameters(width, height));

            //Texture tex = Texture.FromFile(device, "Gator_Stage_Floor_Block.png");
            //Texture paletteTexture = Texture.FromFile(device, "color_map.dds");
            
            Texture paletteTexture = new Texture(device, 256, 1, 1, Usage.Dynamic, Format.X8R8G8B8, Pool.Default);
            DataRectangle rect = paletteTexture.LockRectangle(0, LockFlags.Discard);
            using (DataStream stream = new DataStream(rect.DataPointer, 256 * 1 * sizeof(int), true, true))
            {
                stream.Write(new Color(0, 0, 33, 255).ToBgra()); // 0
                stream.Write(new Color(0, 57, 173, 255).ToBgra()); // 1
                stream.Write(new Color(0, 107, 189, 255).ToBgra()); // 2
                stream.Write(new Color(0, 222, 239, 255).ToBgra()); // 3
                stream.Write(new Color(198, 181, 198, 255).ToBgra()); // 4
                stream.Write(new Color(41, 33, 41, 255).ToBgra()); // 5
                stream.Write(new Color(16, 8, 16, 255).ToBgra()); // 6
                stream.Write(new Color(255, 33, 255, 255).ToBgra()); // 7
                stream.Write(new Color(132, 16, 0, 255).ToBgra()); // 8
                stream.Write(new Color(99, 33, 0, 255).ToBgra()); // 9
                stream.Write(new Color(66, 24, 0, 255).ToBgra()); // 10
                stream.Write(new Color(16, 24, 0, 255).ToBgra()); // 11
                stream.Write(new Color(24, 16, 24, 255).ToBgra()); // 12
                stream.Write(new Color(33, 24, 33, 255).ToBgra()); // 13
                stream.Write(new Color(115, 82, 115, 255).ToBgra()); // 14
                stream.Write(new Color(57, 41, 57, 255).ToBgra()); // 15

                for (int i = 16; i < 256; i++)
                    stream.Write(new Color(0, 0, 0, 255).ToBgra());
            }

            paletteTexture.UnlockRectangle(0);

            Texture tex = new Texture(device, 32, 32, 1, Usage.Dynamic, Format.X8R8G8B8, Pool.Default);
            rect = tex.LockRectangle(0, LockFlags.Discard);

            using (DataStream stream = new DataStream(rect.DataPointer, 32 * 32 * sizeof(int), true, true))
            {
                for (int i = 0; i < bytes.Length; i++)
                    stream.Write(new Color((int) bytes[i], 0, 0, 255).ToBgra());
            }

            tex.UnlockRectangle(0);

            Sprite sprite = new Sprite(device);
            // to resize/rotate/position sprite.Transform = some 4x4 affine transform matrix (SharpDX.Matrix)

            ShaderBytecode function = new ShaderBytecode(g_ps20_main);
            PixelShader shader = new PixelShader(device, function);

            core = new MMXCore();
            core.LoadNewRom(Assembly.GetExecutingAssembly().GetManifestResourceStream("D3D9Test.roms." + ROM_NAME + ".smc"));
            core.Init();

            if (core.CheckROM() != 0)
            {
                core.LoadFont();
                core.LoadProperties();

                core.SetLevel((ushort) LEVEL, 0);
                core.LoadLevel();

                core.UpdateVRAMCache();

                LoadPalette(device);
                RefreshMapCache(device);
            }

            device.VertexShader = null;
            device.PixelShader = shader;
            device.VertexFormat = D3DFVF_TLVERTEX;

            const int vSize = 28;
            VertexBuffer vb = new VertexBuffer(device, vSize * 4, Usage.WriteOnly, D3DFVF_TLVERTEX, Pool.Managed);

            device.SetStreamSource(0, vb, 0, vSize);

            device.SetRenderState(RenderState.Lighting, false);
            device.SetRenderState(RenderState.AlphaBlendEnable, true);
            device.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            device.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
            device.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);           

            // Use clock
            var clock = new Stopwatch();
            clock.Start();

            RenderLoop.Run(form, () =>
            {
                var time = clock.ElapsedMilliseconds / 1000.0f;

                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
                device.BeginScene();

                device.SetTexture(1, paletteTexture);
                //BlitD3D(device, vb, tex, new SharpDX.Rectangle(64, 64, 128, 128), Color4.White, 0);
                
                RenderAllMaps(device, vb);

                device.EndScene();
                device.Present();
            });

            device.Dispose();
            direct3D.Dispose();
        }
    }
}
