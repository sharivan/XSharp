using SharpDX;
using _d2d = SharpDX.Direct2D1;
using _d3d = SharpDX.Direct3D;
using _d3d11 = SharpDX.Direct3D11;
using _dxgi = SharpDX.DXGI;
using _directWrite = SharpDX.DirectWrite;
using _wic = SharpDX.WIC;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            safeDispose(ref d3d11Device);
            safeDispose(ref swapChain);
            safeDispose(ref dxgiFactory);
            safeDispose(ref d2dFactory);
            safeDispose(ref d2dFactory4);
            safeDispose(ref dxgiDevice);
            safeDispose(ref d2dDevice3);
            safeDispose(ref d2dDeviceContext3);
            safeDispose(ref sourceImage);
            safeDispose(ref spriteBatch);
            safeDispose(ref d2dTarget);
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
                new[] { _d3d.FeatureLevel.Level_12_1 },
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
            sourceImage = createD2DBitmap(@"c:\yourFile.png", d2dDeviceContext3);

            spriteBatch = new SpriteBatch(d2dDeviceContext3);
            var destinationRects = new RawRectangleF[1];
            destinationRects[0] = new RectangleF(100, 50, sourceImage.Size.Width, sourceImage.Size.Height);

            var sourceRects = new RawRectangle[1];
            sourceRects[0] = new RectangleF(0, 0, sourceImage.Size.Width, sourceImage.Size.Height);
            #endregion

            #region mainLoop
            RenderLoop.Run(mainForm, () =>
            {
                if (d2dTarget != null)
                {
                    d2dTarget.Dispose();
                    d2dTarget = null;
                }

                using (var backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0))
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

                //the key missing piece: cannot use per primitive antialiasing with spritebatch
                d2dDeviceContext3.AntialiasMode = AntialiasMode.Aliased;
                d2dDeviceContext3.BeginDraw();

                spriteBatch.Clear();
                spriteBatch.AddSprites(
                    1,
                    destinationRects,
                    sourceRects,
                    null,
                    null,
                    destinationRectanglesStride: 0, //0 stride because there is only 1 element
                    sourceRectanglesStride: 0,
                    colorsStride: 0,
                    transformsStride: 0);

                d2dDeviceContext3.DrawSpriteBatch(
                    spriteBatch: spriteBatch,
                    startIndex: 0,
                    spriteCount: 1,
                    bitmap: sourceImage,
                    interpolationMode: BitmapInterpolationMode.Linear,
                    spriteOptions: SpriteOptions.ClampToSourceRectangle);

                d2dDeviceContext3.EndDraw();

                //first param set to 1 would indicate waitVerticalBlanking
                swapChain.Present(0, PresentFlags.None);
            });
            #endregion
        }

        void safeDispose<T>(ref T disposable) where T : class, IDisposable
        {
            if (disposable != null)
            {
                disposable.Dispose();
                disposable = null;
            }
        }

        Bitmap1 createD2DBitmap(string filePath, _d2d.DeviceContext deviceContext)
        {
            var imagingFactory = new _wic.ImagingFactory();

            var fileStream = new NativeFileStream(
                filePath,
                NativeFileMode.Open,
                NativeFileAccess.Read);

            var bitmapDecoder = new _wic.BitmapDecoder(imagingFactory, fileStream, _wic.DecodeOptions.CacheOnDemand);
            var frame = bitmapDecoder.GetFrame(0);

            var converter = new _wic.FormatConverter(imagingFactory);
            converter.Initialize(frame, SharpDX.WIC.PixelFormat.Format32bppPRGBA);

            var newBitmap = SharpDX.Direct2D1.Bitmap1.FromWicBitmap(deviceContext, converter);

            return newBitmap;
        }
    }
}