using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Direct3D11;

using XSharp.Engine.Graphics;

namespace XSharp.Graphics;

public class DX11RenderTarget : IRenderTarget
{
    internal Surface surface;

    public DX11RenderTarget(Surface surface)
    {
        this.surface = surface;
    }

    public void Dispose()
    {
        surface?.Dispose();
    }

    public static implicit operator Surface(DX11RenderTarget renderTarget)
    {
        return renderTarget != null ? renderTarget.surface : null;
    }

    public static implicit operator DX11RenderTarget(Surface surface)
    {
        return surface !=  null ? new DX11RenderTarget(surface) : null;
    }
}