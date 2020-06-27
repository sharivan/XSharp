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

        static private Rational Rationalize(MMXFloat number)
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
            form.Resize += new System.EventHandler(Form_Resize);

            // SwapChain description
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                                   new ModeDescription(form.ClientSize.Width, form.ClientSize.Height,
                                                       Rationalize(TICKRATE), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Create Device and SwapChain
            SharpDX.Direct3D11.Device device;
            SwapChain swapChain;
            SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, new SharpDX.Direct3D.FeatureLevel[] { SharpDX.Direct3D.FeatureLevel.Level_10_0 }, desc, out device, out swapChain);

            var d2dFactory = new SharpDX.Direct2D1.Factory();
            var imgFactory = new SharpDX.WIC.ImagingFactory();
            var dwFactory = new SharpDX.DirectWrite.Factory();

            SharpDX.DXGI.Factory parentFactory = swapChain.GetParent<SharpDX.DXGI.Factory>();
            parentFactory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);

            // New RenderTargetView from the backbuffer
            Texture2D backBuffer = SharpDX.Direct3D11.Resource.FromSwapChain<Texture2D>(swapChain, 0);
            var renderView = new RenderTargetView(device, backBuffer);

            Surface surface = backBuffer.QueryInterface<Surface>();

            var d2dRenderTarget = new RenderTarget(d2dFactory, surface, new RenderTargetProperties(new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied)));
            d2dRenderTarget.TextAntialiasMode = TextAntialiasMode.Cleartype;
            d2dRenderTarget.AntialiasMode = AntialiasMode.Aliased;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            engine = new GameEngine(form, d2dFactory, imgFactory, dwFactory, d2dRenderTarget);

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
                device.ImmediateContext.ClearState();
                device.ImmediateContext.Flush();
                device.Dispose();
                swapChain.Dispose();
                parentFactory.Dispose();
            }
        }
    }
}
