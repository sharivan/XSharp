using System;
using System.IO;

using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.WIC;
using SharpDX.Mathematics.Interop;

using MMX.Math;
using MMX.Geometry;

using D2DBitmap = SharpDX.Direct2D1.Bitmap;
//using WicBitmap = SharpDX.WIC.Bitmap;
using D2DPixelFormat = SharpDX.Direct2D1.PixelFormat;
//using WicPixelFormat = SharpDX.WIC.PixelFormat;

using static MMX.Engine.Consts;

namespace MMX.Engine
{
    public class Tile : IDisposable
    {
        private readonly World world;
        private readonly int id;

        //internal WicBitmap bitmap;
        //internal WicRenderTarget target;

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
            var pixelFormat = new D2DPixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied);

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

        internal Tile(World world, int id, D2DBitmap source) :
            this(world, id)
        {
            SetPixels(source);
        }

        internal Tile(World world, int id, D2DBitmap source, Point offset) :
            this(world, id)
        {
            SetPixels(source, offset);
        }

        internal Tile(World world, int id, byte[] source, Color[] palette = null) :
            this(world, id)
        {
            SetPixels(source, palette);
        }

        /*private void CreateEmptyBitmap()
        {
            bitmap = new WicBitmap(world.Engine.WicFactory, TILE_SIZE, TILE_SIZE, WicPixelFormat.Format32bppRGBA, BitmapCreateCacheOption.CacheOnDemand);

            var properties = new RenderTargetProperties();
            target = new WicRenderTarget(world.Engine.D2DFactory, bitmap, properties)
            {
                AntialiasMode = ANTIALIAS_MODE
            };
        }*/

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

        public void SetPixels(D2DBitmap source)
        {
            SetPixels(source, Point.Zero);
        }

        public void SetPixels(D2DBitmap source, Point offset)
        {
            //if (target == null)
            //    CreateEmptyBitmap();

            target.BeginDraw();
            target.DrawBitmap(source, new RectangleF(0, 0, TILE_SIZE, TILE_SIZE), 1, BITMAP_INTERPOLATION_MODE, new RectangleF(offset.X, offset.Y, TILE_SIZE, TILE_SIZE));
            //target.Flush();
            target.EndDraw();
        }

        public void SetPixels(byte[] source, Color[] palette = null)
        {
            /*if (palette != null)
            {
                target.Bitmap.CopyFromMemory(source, sizeof(byte) * TILE_SIZE);
            }
            else*/
                target.Bitmap.CopyFromMemory(source, sizeof(int) * TILE_SIZE);

            /*Dispose();

            using (MemoryStream stream = new MemoryStream(source))
            {
                BitmapDecoder bitmapDecoder = new BitmapDecoder(world.Engine.WicFactory, stream, palette != null ? WicPixelFormat.Format4bppIndexed : WicPixelFormat.Format32bppRGBA, DecodeOptions.CacheOnDemand);
                BitmapFrameDecode frame = bitmapDecoder.GetFrame(0);
                FormatConverter converter = new FormatConverter(world.Engine.WicFactory);

                if (palette != null)
                {
                    Palette p = new Palette(world.Engine.WicFactory);
                    p.Initialize(palette);
                    converter.Initialize(frame, WicPixelFormat.Format4bppIndexed, BitmapDitherType.None, p, 0, BitmapPaletteType.Custom);
                }
                else
                    converter.Initialize(frame, WicPixelFormat.Format32bppPRGBA);

                bitmap = new WicBitmap(world.Engine.WicFactory, converter, new RawBox(0, 0, TILE_SIZE, TILE_SIZE));

                var properties = new RenderTargetProperties();
                target = new WicRenderTarget(world.Engine.D2DFactory, bitmap, properties)
                {
                    AntialiasMode = ANTIALIAS_MODE
                };
            }*/
        }

        public void FillColor(Color color)
        {
            //if (target == null)
            //    CreateEmptyBitmap();

            target.BeginDraw();
            target.Clear(color);
            //target.Flush();
            target.EndDraw();
        }

        public void Dispose()
        {
            if (target != null)
            {
                target.Dispose();
                target = null;

                //bitmap.Dispose();
                //bitmap = null;
            }
        }
    }
}
