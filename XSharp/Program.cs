using System;
using System.Diagnostics;
using System.Threading;

using SharpDX.Direct3D9;
using SharpDX.Windows;

using MMX.Engine;

using static MMX.Engine.Consts;

namespace XSharp
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
            var form = new RenderForm("X#")
            {
                ClientSize = new System.Drawing.Size((int) DEFAULT_CLIENT_WIDTH * 4, (int) DEFAULT_CLIENT_HEIGHT * 4)
            };
            form.Resize += new EventHandler(Form_Resize);

            // Creates the Device
            var direct3D = new Direct3D();
            var device = new Device(direct3D, 0, DeviceType.Hardware, form.Handle, CreateFlags.HardwareVertexProcessing | CreateFlags.FpuPreserve | CreateFlags.Multithreaded, new PresentParameters((int) DEFAULT_CLIENT_WIDTH, (int) DEFAULT_CLIENT_HEIGHT));

            engine = new GameEngine(form, device);

            try
            {
                // Main loop
                RenderLoop.Run(form, engine.Render);
                Thread.Sleep(4);
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
