using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Format = XSharp.Graphics.Format;
using DXGIFormat = SharpDX.DXGI.Format;

namespace XSharp.Interop;

public static class FormatExtensions
{
    public static DXGIFormat ToDGIFormat(this Format format)
    {
        return (DXGIFormat) format;
    }

    public static Format ToFormat(this DXGIFormat format)
    {
        return (Format) format;
    }
}