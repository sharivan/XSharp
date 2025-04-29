using SharpDX.Direct3D9;

using XSharp.Graphics;
using XSharp.Interop;
using XSharp.Math.Geometry;

using DX9Vector2 = SharpDX.Vector2;

namespace XSharp.Engine.Graphics;

public class DX9Line(Line line) : ILine
{
    private Line line = line;

    public float Width
    {
        get => line.Width;
        set => line.Width = value;
    }

    public void Begin()
    {
        line.Begin();
    }

    public void Dispose()
    {
        line?.Dispose();
    }

    public void Draw(Vector2[] points, Color color)
    {
        var pts = new DX9Vector2[points.Length];
        for (int i = 0; i < pts.Length; i++)
            pts[i] = points[i].ToDX9Vector2();

        line.Draw(pts, color.ToDX9Color());
    }

    public void End()
    {
        line.End();
    }
}