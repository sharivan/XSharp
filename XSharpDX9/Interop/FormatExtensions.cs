using DX9Format = SharpDX.Direct3D9.Format;
using Format = XSharp.Graphics.Format;

namespace XSharp.Interop;

public static class FormatExtensions
{
    public static DX9Format ToDX9Format(this Format format)
    {
        return (DX9Format) format;
    }

    public static Format ToFormat(this DX9Format format)
    {
        return (Format) format;
    }
}