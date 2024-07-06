using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Color = XSharp.Graphics.Color;
using DX11Color = SharpDX.Color;

namespace XSharp.Interop;

public static class ColorExtensions
{
    public static DX11Color ToDX11Color(this Color color)
    {
        return new DX11Color(color.ToRgba());
    }

    public static Color ToColor(this DX11Color color)
    {
        return new Color(color.ToRgba());
    }
}