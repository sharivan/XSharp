using XSharp.Engine.Graphics;
using XSharp.Graphics;
using XSharp.Math.Fixed.Geometry;
using static XSharp.Engine.Functions;

using Box = XSharp.Math.Fixed.Geometry.Box;

namespace XSharp.Engine.World;

public class ForegroundLayout : Layout
{
    public override Palette Palette => Engine.ForegroundPalette;

    public override ITexture Tilemap => Engine.ForegroundTilemap;

    internal ForegroundLayout(int sceneRowCount, int sceneColCount) : base(sceneRowCount, sceneColCount)
    {
    }

    public override void Render(IRenderTarget target)
    {
    }

    public void Render(int layer)
    {
        var camera = Engine.Camera;
        if (camera == null)
            return;

        Vector screenLT = camera.LeftTop;
        Vector screenRB = camera.RightBottom;

        Cell start = GetSceneCellFromPos(screenLT);
        Cell end = GetSceneCellFromPos(screenRB);

        for (int col = start.Col; col <= end.Col + 1; col++)
        {
            if (col < 0 || col >= SceneColCount)
                continue;

            for (int row = start.Row; row <= end.Row + 1; row++)
            {
                if (row < 0 || row >= SceneRowCount)
                    continue;

                Scene scene = scenes[row, col];
                if (scene != null)
                {
                    var sceneLT = GetSceneLeftTop(row, col);
                    Box sceneBox = GetSceneBoundingBoxFromPos(sceneLT);
                    scene.RenderLayer(layer, Tilemap, Palette, FadingControl, sceneBox);
                }
            }
        }
    }
}