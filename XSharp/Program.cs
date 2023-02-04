using SharpDX.Windows;
using System;
using System.Windows.Forms;
using XSharp.Engine;
using static XSharp.Engine.Consts;

namespace XSharp
{
    static class Program
    {
        static private void Form_Resize(object sender, EventArgs e)
        {
            //if (engine != null)
            //    engine.UpdateScale();
        }

        static private void Form_Closing(object sender, EventArgs e)
        {
            GameEngine.Engine.Running = false;
        }

        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var form = new RenderForm("X#")
            {
                ClientSize = new System.Drawing.Size((int) (DEFAULT_CLIENT_WIDTH * 4), (int) (DEFAULT_CLIENT_HEIGHT * 4))
            };

            form.Resize += new EventHandler(Form_Resize);
            form.FormClosing += new FormClosingEventHandler(Form_Closing);


            try
            {
                GameEngine.Initialize(form);
                GameEngine.Run();
            }
#if !DEBUG
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine(e.StackTrace);
            }
#endif
            finally
            {
                GameEngine.Dispose();
            }
        }
    }
}