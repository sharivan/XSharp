using System;

using XSharp.Graphics;
using XSharp.Math.Geometry;

namespace XSharp.Engine.Graphics;

public interface ILine : IDisposable
{
    public float Width
    {
        get;
        set;
    }

    public void Begin();

    public void End();

    public void Draw(float x1, float y1, float x2, float y2, Color color)
    {
        Draw([new Vector2(x1, y1), new Vector2(x2, y2)], color);
    }

    public void Draw(Vector2 from, Vector2 to, Color color)
    {
        Draw([from, to], color);
    }

    public void Draw(Vector2[] points, Color color);
}