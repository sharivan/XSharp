using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Format = XSharp.Graphics.Format;
using DX9Format = SharpDX.Direct3D9.Format;

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