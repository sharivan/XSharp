using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D11;

using XSharp.Engine.Graphics;
using XSharp.Interop;

namespace XSharp.Graphics;

public class DX11Texture : ITexture
{
    internal Texture2D impl;
    private bool canDispose = true;
    private DX11RenderTarget renderTargetImpl;

    public int Width => impl.Description.Width;

    public int Height => impl.Description.Height;

    public Format Format => impl.Description.Format.ToFormat();

    public IRenderTarget RenderTarget => renderTargetImpl;

    public DX11Texture(Texture2D impl, bool canDispose = true)
    {
        this.impl = impl;
        this.canDispose = canDispose;

        renderTargetImpl = new DX11RenderTarget(impl.GetSurfaceLevel(0));
    }

    public DX11Texture(Device device, int width, int height, int levelCount, ResourceUsage usage, Format format)
    {
        var description = new Texture2DDescription
        {
            Width = width,
            Height = height,
            Usage = usage,
            Format = format.ToDGIFormat()
        };

        impl = new Texture2D(device, description);
        renderTargetImpl = new DX11RenderTarget(impl.GetSurfaceLevel(0));
    }

    public void Dispose()
    {
        if (canDispose)
        {
            impl?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public SurfaceDescription GetLevelDescription(int level)
    {
        return impl.GetLevelDescription(level);
    }

    public Surface GetSurfaceLevel(int level)
    {
        return impl.GetSurfaceLevel(level);
    }

    public DataRectangle LockRectangle(bool discard = false)
    {
        return LockRectangle(0, discard ? LockFlags.Discard : LockFlags.None);
    }

    public DataRectangle LockRectangle(int level, LockFlags flags)
    {
        return impl.LockRectangle(level, flags).ToDataRectangle();
    }

    public void UnlockRectangle()
    {
        UnlockRectangle(0);
    }

    public void UnlockRectangle(int level)
    {
        impl.UnlockRectangle(level);
    }

    public static implicit operator Texture2D(DX11Texture texture)
    {
        return texture?.impl;
    }

    public static implicit operator DX11Texture(Texture2D texture)
    {
        return texture != null ? new DX11Texture(texture) : null;
    }
}