using System;
using System.Diagnostics;
using System.Threading;

using SharpDX.Direct3D9;
using SharpDX.Windows;

using MMX.Engine;

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

        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var form = new RenderForm("Mega Man Xvan");
            form.ClientSize = new System.Drawing.Size((int) DEFAULT_CLIENT_WIDTH * 4, (int) DEFAULT_CLIENT_HEIGHT * 4);
            form.Resize += new EventHandler(Form_Resize);

            // Creates the Device
            var direct3D = new Direct3D();
            var device = new Device(direct3D, 0, DeviceType.Hardware, form.Handle, CreateFlags.HardwareVertexProcessing | CreateFlags.FpuPreserve | CreateFlags.Multithreaded, new PresentParameters((int) DEFAULT_CLIENT_WIDTH, (int) DEFAULT_CLIENT_HEIGHT));

            engine = new GameEngine(form, device);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

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

                    // Determine the time it took to render the frame
                    double deltaTime = 1000 * (clock.ElapsedTicks / clockFrequency - totalSeconds);
                    int delta = (int) (maxTimeToWait - deltaTime);
                    if (delta > 0)
                        Thread.Sleep(delta);
                });

                #endregion
            }
            finally
            {
                engine.Dispose();

                // Release all resources
                device.Dispose();
                direct3D.Dispose();
            }
        }
    }
}
