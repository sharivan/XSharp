using MaterialSkin;
using MaterialSkin.Controls;
using SharpDX;
using SharpDX.Direct3D9;
using System;
using System.Windows.Forms;
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

    private class TileRender(Control control)
    {
        private Control control = control;

        public void Render()
        {
            var device = DX9Engine.Engine.Device;

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();

            var tilemap = BaseEngine.Engine.ForegroundTilemap;
            int tilemapWidth = tilemap.Width;
            int tilemapHeight = tilemap.Height;
            var palette = BaseEngine.Engine.ForegroundPalette;
            BaseEngine.Engine.DrawTexture(tilemap, palette);

            device.EndScene();
            device.Present(new SharpDX.Rectangle(0, 0, tilemapWidth, tilemapHeight), new SharpDX.Rectangle(0, 0, control.ClientSize.Width, control.ClientSize.Height), control.Handle);
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

        var foregroundLayoutWidth = BaseEngine.Engine.World.ForegroundLayout.Width;
        var foregroundLayoutHeight = BaseEngine.Engine.World.ForegroundLayout.Height;
        var max = FixedSingle.Max(foregroundLayoutWidth, foregroundLayoutHeight);
        var ratio = BaseEngine.Engine.StageSize.Y / BaseEngine.Engine.StageSize.X;
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
        BaseEngine.Engine.Camera.Size = SCREEN_SIZE * scale;
    }

    private void Timer1_Tick(object sender, EventArgs e)
    {
        BaseEngine.RunSingleFrame();
        tileRender.Render();
    }

    private void FrmLevelEditor_FormClosed(object sender, FormClosedEventArgs e)
    {
        BaseEngine.DisposeEngine();
    }

    private void BtnPointer_Click(object sender, EventArgs e)
    {

    }

    private void BtnHand_Click(object sender, EventArgs e)
    {

    }

    private void SdxRender_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            startX = e.X;
            startY = e.Y;
            startCameraOrigin = BaseEngine.Engine.Camera.Origin;
            moving = true;
        }
    }

    private void SdxRender_MouseMove(object sender, MouseEventArgs e)
    {
        if (moving)
        {
            double dx = e.X - startX;
            double dy = e.Y - startY;

            dx *= (double) BaseEngine.Engine.Camera.Width / sdxRender.Width;
            dy *= (double) BaseEngine.Engine.Camera.Height / sdxRender.Height;
            BaseEngine.Engine.Camera.SetOrigin(startCameraOrigin - (dx, dy), false);
        }
    }

    private void SdxRender_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            moving = false;
    }
}