using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSharp.Engine.Graphics;

namespace XSharp.Graphics;

public class DX9RenderTarget : IRenderTarget
{
    internal Surface surface;

    public DX9RenderTarget(Surface surface)
    {
        this.surface = surface;
    }

    public void Dispose()
    {
        surface?.Dispose();
    }

    public static implicit operator Surface(DX9RenderTarget renderTarget)
    {
        return renderTarget != null ? renderTarget.surface : null;
    }

    public static implicit operator DX9RenderTarget(Surface surface)
    {
        return surface !=  null ? new DX9RenderTarget(surface) : null;
    }
}