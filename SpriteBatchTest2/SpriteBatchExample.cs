using SharpDX;
using _d2d = SharpDX.Direct2D1;
using _d3d = SharpDX.Direct3D;
using _d3d11 = SharpDX.Direct3D11;
using _dxgi = SharpDX.DXGI;
using _wic = SharpDX.WIC;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using System;
using SharpDX.IO;
using SharpDX.Mathematics.Interop;

namespace XSharp
{
    public class SpriteBatchExample
    {
        [STAThread]
        static void Main(string[] args)
        {
            var app = new SpriteBatchExample();
            app.Run();
        }

        #region Variables
        _d3d11.Device d3d11Device;
        SwapChain swapChain;
        _dxgi.Factory1 dxgiFactory;
        _d2d.Factory d2dFactory;
        _d2d.Factory4 d2dFactory4;

        _dxgi.Device dxgiDevice;
        _d2d.Device3 d2dDevice3;
        _d2d.DeviceContext3 d2dDeviceContext3;

        Bitmap1 sourceImage;
        SpriteBatch spriteBatch;
        Bitmap1 d2dTarget;
        #endregion

        ~SpriteBatchExample()
        {
            SafeDispose(ref d3d11Device);
            SafeDispose(ref swapChain);
            SafeDispose(ref dxgiFactory);
            SafeDispose(ref d2dFactory);
            SafeDispose(ref d2dFactory4);
            SafeDispose(ref dxgiDevice);
            SafeDispose(ref d2dDevice3);
            SafeDispose(ref d2dDeviceContext3);
            SafeDispose(ref sourceImage);
            SafeDispose(ref spriteBatch);
            SafeDispose(ref d2dTarget);
        }

        public void Run()
        {
            #region setup resources      
            var mainForm = new RenderForm();

            var scDescription = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(
                        0,
                        0,
                        new Rational(60, 1),
                        Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = mainForm.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            //DeviceCreationFlags.Debug flag below will show debug layer messages in your output window.
            //Need proper version of windows sdk for it to work, otherwise it will throw an exception.
            //You also need to right click your project->properties->debug (on the left panel)-> check "enable native code debugging"

            // Create Device and SwapChain
            _d3d11.Device.CreateWithSwapChain(
                DriverType.Hardware,
                DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug,
                new[] { _d3d.FeatureLevel.Level_10_0 },
                scDescription,
                out d3d11Device,
                out swapChain);

            // Ignore all windows events
            dxgiFactory = swapChain.GetParent<_dxgi.Factory1>();
            dxgiFactory.MakeWindowAssociation(mainForm.Handle, WindowAssociationFlags.IgnoreAll);

            d2dFactory = new _d2d.Factory();
            d2dFactory4 = d2dFactory.QueryInterface<_d2d.Factory4>();

            dxgiDevice = d3d11Device.QueryInterface<_dxgi.Device>();
            d2dDevice3 = new _d2d.Device3(d2dFactory4, dxgiDevice);
            d2dDeviceContext3 = new _d2d.DeviceContext3(d2dDevice3, DeviceContextOptions.None);
            #endregion

            #region create drawing input
            sourceImage = CreateD2DBitmap(@"resources\tiles\Gator_Stage_Floor_Block.png", d2dDeviceContext3);

            spriteBatch = new SpriteBatch(d2dDeviceContext3);

            const float x = 0;
            const float y = 0;

            const int TILE_SIZE = 8;
            const int BLOCK_SIZE = 4 * TILE_SIZE;

            const int SRC_BLOCK_COUNT = 128;
            const int SRC_TILE_COUNT = 4 * SRC_BLOCK_COUNT;
            const int DST_BLOCK_COUNT = 128;
            const int DST_TILE_COUNT = 4 * DST_BLOCK_COUNT;

            var destinationRects = new RawRectangleF[DST_TILE_COUNT * DST_TILE_COUNT];
            for (int col = 0; col < DST_TILE_COUNT; col++)
                for (int row = 0; row < DST_TILE_COUNT; row++)
                    destinationRects[DST_TILE_COUNT * col + row] = new RectangleF(x + TILE_SIZE * col, y + TILE_SIZE * row, TILE_SIZE, TILE_SIZE);

            var sourceRects = new RawRectangle[SRC_TILE_COUNT * SRC_TILE_COUNT];
            for (int col = 0; col < SRC_TILE_COUNT; col++)
                for (int row = 0; row < SRC_TILE_COUNT; row++)
                    sourceRects[SRC_TILE_COUNT * col + row] = new RectangleF(TILE_SIZE * col % BLOCK_SIZE, TILE_SIZE * row % BLOCK_SIZE, TILE_SIZE, TILE_SIZE);
            #endregion

            spriteBatch.AddSprites(
                destinationRects.Length,
                destinationRects,
                sourceRects,
                null,
                null,
                destinationRectanglesStride: 4 * sizeof(float),
                sourceRectanglesStride: 4 * sizeof(float),
                colorsStride: 0,
                transformsStride: 0);

            if (d2dTarget != null)
            {
                d2dTarget.Dispose();
                d2dTarget = null;
            }

            using (var backBuffer = _d3d11.Resource.FromSwapChain<Texture2D>(swapChain, 0))
            {
                using (var surface = backBuffer.QueryInterface<Surface>())
                {
                    var bmpProperties = new BitmapProperties1(
                        new PixelFormat(Format.R8G8B8A8_UNorm, _d2d.AlphaMode.Premultiplied),
                        dpiX: 96,
                        dpiY: 96,
                        bitmapOptions: BitmapOptions.Target | BitmapOptions.CannotDraw);

                    d2dTarget = new Bitmap1(
                        d2dDeviceContext3,
                        surface,
                        bmpProperties);

                    d2dDeviceContext3.Target = d2dTarget;
                }
            }

            d2dDeviceContext3.AntialiasMode = AntialiasMode.Aliased;

            #region mainLoop
            RenderLoop.Run(mainForm, () =>
            {
                //the key missing piece: cannot use per primitive antialiasing with spritebatch                
                d2dDeviceContext3.BeginDraw();

                d2dDeviceContext3.DrawSpriteBatch(
                    spriteBatch: spriteBatch,
                    startIndex: 0,
                    spriteCount: spriteBatch.SpriteCount,
                    bitmap: sourceImage,
                    interpolationMode: BitmapInterpolationMode.Linear,
                    spriteOptions: SpriteOptions.ClampToSourceRectangle);

                d2dDeviceContext3.EndDraw();

                //first param set to 1 would indicate waitVerticalBlanking
                swapChain.Present(1, PresentFlags.None);
            });
            #endregion
        }

        void SafeDispose<T>(ref T disposable) where T : class, IDisposable
        {
            if (disposable != null)
            {
                disposable.Dispose();
                disposable = null;
            }
        }

        Bitmap1 CreateD2DBitmap(string filePath, _d2d.DeviceContext deviceContext)
        {
            var imagingFactory = new _wic.ImagingFactory();

            var fileStream = new NativeFileStream(
                filePath,
                NativeFileMode.Open,
                NativeFileAccess.Read);

            var bitmapDecoder = new _wic.BitmapDecoder(imagingFactory, fileStream, _wic.DecodeOptions.CacheOnDemand);
            var frame = bitmapDecoder.GetFrame(0);

            var converter = new _wic.FormatConverter(imagingFactory);
            converter.Initialize(frame, _wic.PixelFormat.Format32bppPRGBA);

            var newBitmap = Bitmap1.FromWicBitmap(deviceContext, converter);

            return newBitmap;
        }
    }
}