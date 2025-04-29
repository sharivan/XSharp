using SharpDX.Direct3D9;
using XSharp.Engine.Graphics;

namespace XSharp.Graphics;

public class DX9RenderTarget(Surface surface, bool disposeSurface = true) : IRenderTarget
{
    internal Surface surface = surface;

    public bool DisposeSurface
    {
        get;
        set;
    } = disposeSurface;

    public void Dispose()
    {
        if (DisposeSurface)
            surface?.Dispose();
    }

    public static implicit operator Surface(DX9RenderTarget renderTarget)
    {
        return renderTarget?.surface;
    }

    public static implicit operator DX9RenderTarget(Surface surface)
    {
        return surface != null ? new DX9RenderTarget(surface) : null;
    }
}