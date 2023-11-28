using System;
using XSharp.Graphics;

namespace XSharp.Engine.Graphics;

public class Tileset : IDisposable
{
    public ITexture Texture
    {
        get;
    }

    public Tileset()
    {
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}