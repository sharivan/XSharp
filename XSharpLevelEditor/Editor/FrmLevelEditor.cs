using System;
using System.Windows.Forms;

using SharpDX;
using SharpDX.Direct3D9;

using XSharp.Engine;

namespace XSharp.Editor;

public partial class FrmLevelEditor : Form
{
    private class TileRender
    {
        private Control control;

        public TileRender(Control control)
        {
            this.control = control;
        }

        public void Render()
        {
            var device = GameEngine.Engine.Device;

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();

            var tilemap = GameEngine.Engine.ForegroundTilemap;
            int tilemapWidth = tilemap.GetLevelDescription(0).Width;
            int tilemapHeight = tilemap.GetLevelDescription(0).Height;
            var palette = GameEngine.Engine.ForegroundPalette;
            GameEngine.Engine.DrawTexture(tilemap, palette);

            device.EndScene();
            device.Present(new Rectangle(0, 0, tilemapWidth, tilemapHeight), new Rectangle(0, 0, control.ClientSize.Width, control.ClientSize.Height), control.Handle);
        }
    }

    private TileRender tileRender;

    public FrmLevelEditor()
    {
        InitializeComponent();
    }

    private void FrmLevelEditor_Load(object sender, System.EventArgs e)
    {
        GameEngine.Initialize(sdxRender);
        GameEngine.Engine.Editing = true;

        tileRender = new TileRender(sdxTiles);
    }

    private void timer1_Tick(object sender, System.EventArgs e)
    {
        GameEngine.RunSingleFrame();
        tileRender.Render();
    }

    private void FrmLevelEditor_FormClosed(object sender, FormClosedEventArgs e)
    {
        GameEngine.Dispose();
    }

    private void btnPointer_Click(object sender, EventArgs e)
    {

    }

    private void btnHand_Click(object sender, EventArgs e)
    {

    }
}