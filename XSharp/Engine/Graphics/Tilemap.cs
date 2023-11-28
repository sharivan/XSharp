using System;
using XSharp.Graphics;

namespace XSharp.Engine.Graphics;

public class Tilemap : IDisposable
{
    public ITexture Texture
    {
        get;
    }

    public Tilemap()
    {
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}