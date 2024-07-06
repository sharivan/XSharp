using SharpDX.Windows;
using System.Dynamic;
using XSharp.Engine;
using static XSharp.Engine.Consts;

namespace XSharp;

internal static class Program
{
    static private void Form_Load(object sender, EventArgs e)
    {
        DX9Engine.Engine.LoadConfig();
    }

    static private void Form_Resize(object sender, EventArgs e)
    {
        //if (engine != null)
        //    engine.UpdateScale();
    }

    static private void Form_Closing(object sender, EventArgs e)
    {
        DX9Engine.Engine.SaveConfig();
        BaseEngine.DisposeEngine();
    }

    /// <summary>
    /// Ponto de entrada principal para o aplicativo.
    /// </summary>
    [STAThread]
    static void Main()
    {
        var form = new RenderForm("X#")
        {
            StartPosition = FormStartPosition.Manual,
            ClientSize = new Size((int) DEFAULT_CLIENT_WIDTH, (int) DEFAULT_CLIENT_HEIGHT)
        };

        form.Load += new EventHandler(Form_Load);
        form.Resize += new EventHandler(Form_Resize);
        form.FormClosing += new FormClosingEventHandler(Form_Closing);

        dynamic initializers = new ExpandoObject();
        initializers.control = form;

        try
        {
            BaseEngine.Initialize<DX9Engine>(initializers);
            BaseEngine.Run();
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
            BaseEngine.DisposeEngine();
        }
    }
}