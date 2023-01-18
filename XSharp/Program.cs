using System;
using System.Threading;

using SharpDX.Direct3D9;
using SharpDX.Windows;

using MMX.Engine;

using static MMX.Engine.Consts;
using System.Windows.Forms;

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

        static private void Form_Closing(object sender, EventArgs e)
        {
            engine.Running = false;
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
            form.FormClosing += new FormClosingEventHandler(Form_Closing);

            engine = new GameEngine(form);
            try
            {
                engine.Run();
            }
            finally
            {
                engine.Dispose();
            }
        }
    }
}
