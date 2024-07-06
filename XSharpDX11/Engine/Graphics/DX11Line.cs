using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D11;

using XSharp.Graphics;
using XSharp.Interop;
using XSharp.Math.Geometry;

using DX9Vector2 = SharpDX.Vector2;

namespace XSharp.Engine.Graphics;

public class DX11Line : ILine
{
    private Line line;

    public float Width
    { 
        get => line.Width;
        set => line.Width = value;
    }

    public DX11Line(Line line)
    {
        this.line = line;
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
            pts[i] = points[i].ToDX11Vector2();

        line.Draw(pts, color.ToDX11Color());
    }

    public void End()
    {
        line.End();
    }
}