using System;
using System.Diagnostics;

using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using MMX.Engine;
using System.Threading;

using static MMX.Engine.Consts;
using MMX.Math;
using SharpDX;

namespace Mega_Man_Xvan
{
    static class Program
    {
        private static GameEngine engine;

        static private void Form_Resize(object sender, EventArgs e)
        {
            //if (engine != null)
            //    engine.UpdateScale();
        }

        static private Rational Rationalize(FixedSingle number)
        {
            int intPart = (int) number;
            int numerator = intPart;
            int denominator = 1;
            while (number != intPart)
            {
                number -= intPart;
                number *= 10;
                intPart = (int) number;
                numerator = 10 * numerator + intPart;
                denominator *= 10;
            }

            return new Rational(numerator, denominator);
        }

        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var form = new RenderForm("Mega Man Xvan");
            form.ClientSize = new System.Drawing.Size((int) DEFAULT_CLIENT_WIDTH, (int) DEFAULT_CLIENT_HEIGHT);
            form.Resize += new EventHandler(Form_Resize);

            // SwapChain description
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                                   new ModeDescription(form.ClientSize.Width, form.ClientSize.Height,
                                                       Rationalize(TICKRATE), Format.B8G8R8A8_UNorm),
                IsWindowed = true,
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Create Device and SwapChain
            SharpDX.Direct3D11.Device.CreateWithSwapChain(
                DriverType.Hardware,
                DeviceCreationFlags.BgraSupport
#if DEBUG
                | DeviceCreationFlags.Debug
#endif
                ,
                new SharpDX.Direct3D.FeatureLevel[] {
                    SharpDX.Direct3D.FeatureLevel.Level_11_1,
                    SharpDX.Direct3D.FeatureLevel.Level_11_0,
                    SharpDX.Direct3D.FeatureLevel.Level_10_1,
                    SharpDX.Direct3D.FeatureLevel.Level_10_0,
                    SharpDX.Direct3D.FeatureLevel.Level_9_3,
                    SharpDX.Direct3D.FeatureLevel.Level_9_2,
                    SharpDX.Direct3D.FeatureLevel.Level_9_1 },
                desc,
                out SharpDX.Direct3D11.Device device,
                out SwapChain swapChain);

            var dxgiDevice = ComObject.As<SharpDX.DXGI.Device>(device.NativePointer);
            var d2DDevice = new SharpDX.Direct2D1.Device(dxgiDevice);

            var d2dFactory = d2DDevice.Factory; //new SharpDX.Direct2D1.Factory();
            var wicFactory = new SharpDX.WIC.ImagingFactory();
            var dwFactory = new SharpDX.DirectWrite.Factory();

            var parentFactory = swapChain.GetParent<SharpDX.DXGI.Factory>();
            parentFactory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);

            // New RenderTargetView from the backbuffer
            Texture2D backBuffer = SharpDX.Direct3D11.Resource.FromSwapChain<Texture2D>(swapChain, 0);
            var renderView = new RenderTargetView(device, backBuffer);

            Surface surface = backBuffer.QueryInterface<Surface>();

            /*var d2dRenderTarget = new RenderTarget(d2dFactory, surface, new RenderTargetProperties(new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied)))
            {
                TextAntialiasMode = TEXT_ANTIALIAS_MODE,
                AntialiasMode = ANTIALIAS_MODE
            };*/

            var d2DDeviceContext = new SharpDX.Direct2D1.DeviceContext(surface)
            {
                DotsPerInch = new Size2F(form.DeviceDpi, form.DeviceDpi),
                TextAntialiasMode = TEXT_ANTIALIAS_MODE,
                AntialiasMode = ANTIALIAS_MODE
            };

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            engine = new GameEngine(form, d2dFactory, wicFactory, dwFactory, d2DDeviceContext/*d2dRenderTarget*/);

            try
            {
                #region Render loop

                // Create Clock and FPS counters
                Stopwatch clock = new Stopwatch();
                double clockFrequency = Stopwatch.Frequency;
                clock.Start();
                Stopwatch fpsTimer = new Stopwatch();
                fpsTimer.Start();
                double fps = 0.0;
                int fpsFrames = 0;

                double maxTimeToWait = 1000D / TICKRATE;


                // Main loop
                RenderLoop.Run(form, () =>
                {
                    // Time in seconds
                    var totalSeconds = clock.ElapsedTicks / clockFrequency;

                    #region FPS and title update
                    fpsFrames++;
                    if (fpsTimer.ElapsedMilliseconds > 1000)
                    {
                        fps = 1000.0 * fpsFrames / fpsTimer.ElapsedMilliseconds;

                        // Update window title with FPS once every second
                        form.Text = string.Format("Mega Man Xvan - FPS: {0:F2} ({1:F2}ms/frame)", fps, (float) fpsTimer.ElapsedMilliseconds / fpsFrames);

                        // Restart the FPS counter
                        fpsTimer.Reset();
                        fpsTimer.Start();
                        fpsFrames = 0;
                    }
                    #endregion

                    engine.Render();

                    swapChain.Present(VSYNC ? 1 : 0, PresentFlags.None);

                    if (!VSYNC)
                    {
                        // Determine the time it took to render the frame
                        double deltaTime = 1000 * (clock.ElapsedTicks / clockFrequency - totalSeconds);
                        int delta = (int) (maxTimeToWait - deltaTime);
                        if (delta > 0)
                            Thread.Sleep(delta);
                    }
                });

                #endregion
            }
            finally
            {
                engine.Dispose();

                // Release all resources
                renderView.Dispose();
                backBuffer.Dispose();
                d2DDevice.Dispose();
                device.ImmediateContext.ClearState();
                device.ImmediateContext.Flush();
                device.Dispose();
                swapChain.Dispose();
                parentFactory.Dispose();
            }
        }
    }
}
