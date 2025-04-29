using SharpDX;
using System.Runtime.InteropServices;

namespace XSharpDX11.Engine.Graphics;

[StructLayout(LayoutKind.Sequential)]
public struct FadingParams
{
    public Vector4 fadingLevel = Vector4.One;
    public Vector4 fadingColor = Vector4.One;

    public FadingParams()
    {
    }
}