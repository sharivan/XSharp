using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSharp.Engine.Graphics;

namespace XSharp.Graphics;

public interface ITexture : IDisposable
{
    public int Width
    {
        get;
    }

    public int Height
    {
        get;
    }

    public Format Format
    {
        get;
    }

    public IRenderTarget RenderTarget
    {
        get;
    }

    public DataRectangle LockRectangle(bool discard = false);

    public void UnlockRectangle();
}