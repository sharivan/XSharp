using System.Windows.Forms;

using XSharp.Engine;

namespace XSharpLevelEditor;

public partial class FrmLevelEditor : Form
{
    public FrmLevelEditor()
    {
        InitializeComponent();
    }

    private void FrmLevelEditor_Load(object sender, System.EventArgs e)
    {
        GameEngine.Initialize(renderControl1);
    }

    private void timer1_Tick(object sender, System.EventArgs e)
    {
        GameEngine.RunSingleFrame();
    }

    private void FrmLevelEditor_FormClosed(object sender, FormClosedEventArgs e)
    {
        GameEngine.Dispose();
    }
}