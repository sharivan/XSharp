using System;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using MMX.Math;
using MMX.Geometry;

using static MMX.Engine.Consts;

namespace MMX.Engine
{
    public class Tile : IDisposable
    {
        private World world;
        private int id;

        internal BitmapRenderTarget target;

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

        internal Tile(World world, int id)
        {
            this.world = world;
            this.id = id;

            var size = new Size2(TILE_SIZE, TILE_SIZE);
            var sizef = new Size2F(TILE_SIZE, TILE_SIZE);
            var pixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied);

            target = new BitmapRenderTarget(world.Engine.Context, CompatibleRenderTargetOptions.None, sizef, size, pixelFormat)
            {
                AntialiasMode = ANTIALIAS_MODE
            };
        }

        internal Tile(World world, int id, Color color) :
            this(world, id)
        {
            FillColor(color);
        }

        /*internal Tile(World world, int id, Color[,] pixels) :
            this(world, id)
        {
            SetPixels(pixels);
        }*/

        /*internal Tile(World world, int id, Color[,] pixels, Point offset) :
            this(world, id)
        {
            SetPixels(pixels, offset);
        }*/

        internal Tile(World world, int id, Bitmap source) :
            this(world, id)
        {
            SetPixels(source);
        }

        internal Tile(World world, int id, Bitmap source, Point offset) :
            this(world, id)
        {
            SetPixels(source, offset);
        }

        internal Tile(World world, int id, byte[] source) :
            this(world, id)
        {
            SetPixels(source);
        }

        /*public Color GetPixel(int x, int y)
        {
            unsafe
            {
                IntPtr ptr = bitmap.NativePointer + (TILE_SIZE * x + y) * sizeof(int);
                return *((Color*) ptr);
            }
        }*/

        /*public void SetPixel(int x, int y, Color value)
        {
            unsafe
            {
                IntPtr ptr = bitmap.NativePointer + (TILE_SIZE * x + y) * sizeof(int);
                *((Color*) ptr) = value;
            }
        }*/

        /*public void SetPixels(Color[,] pixels)
        {
            SetPixels(pixels, Point.Zero);
        }*/

        /*public void SetPixels(Color[,] pixels, Point offset)
        {
            unsafe
            {
                for (int x = 0; x < TILE_SIZE; x++)
                    for (int y = 0; y < TILE_SIZE; y++)
                    {
                        IntPtr ptr = bitmap.NativePointer + (TILE_SIZE * x + y) * sizeof(int);
                        *((Color*) ptr) = pixels[x + offset.X, offset.Y + y];
                    }
            }
        }*/

        public void SetPixels(Bitmap source)
        {
            SetPixels(source, Point.Zero);
        }

        public void SetPixels(Bitmap source, Point offset)
        {
            target.BeginDraw();
            target.DrawBitmap(source, new RectangleF(0, 0, TILE_SIZE, TILE_SIZE), 1, INTERPOLATION_MODE, new RectangleF(offset.X, offset.Y, TILE_SIZE, TILE_SIZE));
            //target.Flush();
            target.EndDraw();
        }

        public void SetPixels(byte[] source)
        {
            target.Bitmap.CopyFromMemory(source, sizeof(int) * TILE_SIZE);
        }

        public void FillColor(Color color)
        {
            target.BeginDraw();
            target.Clear(color);
            //target.Flush();
            target.EndDraw();
        }

        public void Dispose()
        {
            target.Dispose();
        }
    }
}
