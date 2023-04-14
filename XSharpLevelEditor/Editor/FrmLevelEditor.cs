using System;
using System.Windows.Forms;

using MaterialSkin;
using MaterialSkin.Controls;

using SharpDX;
using SharpDX.Direct3D9;

using XSharp.Engine;
using XSharp.Math;
using XSharp.Math.Geometry;

using static XSharp.Engine.Consts;

namespace XSharp.Editor;

public partial class FrmLevelEditor : MaterialForm
{
    public const float WHELL_SCALE_FACTOR = 1.1f;
    public const float MIN_SCALE = 0.25f;
    public const float MAX_SCALE = 10.0f;

    private class TileRender
    {
        private Control control;

        public TileRender(Control control)
        {
            this.control = control;
        }

        public void Render()
        {
            var device = Engine.BaseEngine.Engine.Device;

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();

            var tilemap = Engine.BaseEngine.Engine.ForegroundTilemap;
            int tilemapWidth = tilemap.GetLevelDescription(0).Width;
            int tilemapHeight = tilemap.GetLevelDescription(0).Height;
            var palette = Engine.BaseEngine.Engine.ForegroundPalette;
            Engine.BaseEngine.Engine.DrawTexture(tilemap, palette);

            device.EndScene();
            device.Present(new Rectangle(0, 0, tilemapWidth, tilemapHeight), new Rectangle(0, 0, control.ClientSize.Width, control.ClientSize.Height), control.Handle);
        }
    }

    private TileRender tileRender;

    private float scale = 1;
    private bool moving = false;
    private int startX;
    private int startY;
    private Vector startCameraOrigin;

    public FrmLevelEditor()
    {
        InitializeComponent();

        var materialSkinManager = MaterialSkinManager.Instance;
        materialSkinManager.AddFormToManage(this);
        materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
        materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
    }

    private void FrmLevelEditor_Load(object sender, EventArgs e)
    {
        BaseEngine.Initialize<EditorEngine>(sdxRender);
        BaseEngine.Engine.Editing = true;

        var foregroundLayoutWidth = Engine.BaseEngine.Engine.World.ForegroundLayout.Width;
        var foregroundLayoutHeight = Engine.BaseEngine.Engine.World.ForegroundLayout.Height;
        var max = FixedSingle.Max(foregroundLayoutWidth, foregroundLayoutHeight);
        var ratio = Engine.BaseEngine.Engine.StageSize.Y / Engine.BaseEngine.Engine.StageSize.X;
        //GameEngine.Engine.Camera.Size = (max, max * ratio);
        //GameEngine.Engine.Camera.LeftTop = (0, 0);

        tileRender = new TileRender(sdxTiles);

        sdxRender.MouseWheel += SdxRender_MouseWheel;
    }

    private void SdxRender_MouseWheel(object? sender, MouseEventArgs e)
    {
        var delta = e.Delta;
        if (delta == 0)
            return;

        var scrollDelta = SystemInformation.MouseWheelScrollDelta;
        var newScale = scale;

        if (delta > 0)
            newScale /= WHELL_SCALE_FACTOR * delta / scrollDelta;
        else
            newScale *= WHELL_SCALE_FACTOR * -delta / scrollDelta;

        if (newScale is < MIN_SCALE or > MAX_SCALE)
            return;

        scale = newScale;
        Engine.BaseEngine.Engine.Camera.Size = SCREEN_SIZE * scale;
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        Engine.BaseEngine.RunSingleFrame();
        tileRender.Render();
    }

    private void FrmLevelEditor_FormClosed(object sender, FormClosedEventArgs e)
    {
        Engine.BaseEngine.Dispose();
    }

    private void btnPointer_Click(object sender, EventArgs e)
    {

    }

    private void btnHand_Click(object sender, EventArgs e)
    {

    }

    private void sdxRender_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            startX = e.X;
            startY = e.Y;
            startCameraOrigin = Engine.BaseEngine.Engine.Camera.Origin;
            moving = true;
        }
    }

    private void sdxRender_MouseMove(object sender, MouseEventArgs e)
    {
        if (moving)
        {
            double dx = e.X - startX;
            double dy = e.Y - startY;

            dx *= (double) Engine.BaseEngine.Engine.Camera.Width / sdxRender.Width;
            dy *= (double) Engine.BaseEngine.Engine.Camera.Height / sdxRender.Height;
            Engine.BaseEngine.Engine.Camera.SetOrigin(startCameraOrigin - (dx, dy), false);
        }
    }

    private void sdxRender_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            moving = false;
    }
}