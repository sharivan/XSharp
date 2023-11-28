using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Color = XSharp.Graphics.Color;
using DX9Color = SharpDX.Color;

namespace XSharp.Interop;

public static class ColorExtensions
{
    public static DX9Color ToDX9Color(this Color color)
    {
        return new DX9Color(color.ToRgba());
    }

    public static Color ToColor(this DX9Color color)
    {
        return new Color(color.ToRgba());
    }
}