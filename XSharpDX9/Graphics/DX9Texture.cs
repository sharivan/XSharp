using SharpDX.Direct3D9;
using XSharp.Engine.Graphics;
using XSharp.Interop;

namespace XSharp.Graphics;

public class DX9Texture : ITexture
{
    internal Texture impl;
    private bool canDispose = true;
    private DX9RenderTarget renderTargetImpl;

    public int Width => impl.GetLevelDescription(0).Width;

    public int Height => impl.GetLevelDescription(0).Height;

    public Format Format => impl.GetLevelDescription(0).Format.ToFormat();

    public IRenderTarget RenderTarget => renderTargetImpl;

    public DX9Texture(Texture impl, bool canDispose = true)
    {
        this.impl = impl;
        this.canDispose = canDispose;

        renderTargetImpl = new DX9RenderTarget(impl.GetSurfaceLevel(0));
    }

    public DX9Texture(Device device, int width, int height, int levelCount, Usage usage, Format format, Pool pool)
    {
        impl = new Texture(device, width, height, levelCount, usage, format.ToDX9Format(), pool);
        renderTargetImpl = new DX9RenderTarget(impl.GetSurfaceLevel(0));
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

    public static implicit operator Texture(DX9Texture texture)
    {
        return texture?.impl;
    }

    public static implicit operator DX9Texture(Texture texture)
    {
        return texture != null ? new DX9Texture(texture) : null;
    }
}