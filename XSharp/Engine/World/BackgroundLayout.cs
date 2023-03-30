using SharpDX.Direct3D9;

using XSharp.Engine.Entities;
using XSharp.Engine.Graphics;
using XSharp.Math.Geometry;

using static XSharp.Engine.World.World;

using Box = XSharp.Math.Geometry.Box;

namespace XSharp.Engine.World;

public class BackgroundLayout : Layout
{
    public override Palette Palette => Engine.BackgroundPalette;

    public override Texture Tilemap => Engine.BackgroundTilemap;

    internal BackgroundLayout(int sceneRowCount, int sceneColCount) : base(sceneRowCount, sceneColCount)
    {
    }

    public override void Render(IRenderTarget target)
    {
    }

    public void Render(int layer)
    {
        Checkpoint checkpoint = Engine.CurrentCheckpoint;
        if (checkpoint == null)
            return;

        var camera = Engine.Camera;
        if (camera == null)
            return;

        Vector screenLT = camera.LeftTop;
        Vector screenRB = camera.RightBottom;
        Vector backgroundPos = checkpoint.BackgroundPos;

        Vector screenDelta = (checkpoint.Scroll & 0x2) != 0 ? Vector.NULL_VECTOR : (screenLT + checkpoint.CameraPos).Scale(0.5f) - backgroundPos;

        Cell start = GetSceneCellFromPos(screenLT - screenDelta);
        Cell end = GetSceneCellFromPos(screenRB - screenDelta);

        for (int col = start.Col; col <= end.Col + 1; col++)
        {
            if (col < 0)
                continue;

            if ((checkpoint.Scroll & 0x10) == 0 && col >= SceneColCount)
                continue;

            int bkgCol = (checkpoint.Scroll & 0x10) != 0 ? col % 2 : col;

            for (int row = start.Row; row <= end.Row + 1; row++)
            {
                if (row < 0 || row >= SceneRowCount)
                    continue;

                Scene scene = scenes[row, bkgCol];
                if (scene != null)
                {
                    Vector sceneLT = GetSceneLeftTop(row, col);
                    Box sceneBox = GetSceneBoundingBoxFromPos(sceneLT);
                    Engine.RenderVertexBuffer(scene.layers[layer], GameEngine.VERTEX_SIZE, Scene.PRIMITIVE_COUNT, Tilemap, Palette, FadingControl, sceneBox + screenDelta);
                }
            }
        }
    }
}