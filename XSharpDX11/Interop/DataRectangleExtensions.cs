using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using XSharp.Graphics;

using DataRectangle = XSharp.Graphics.DataRectangle;
using DX11DataRectanbgle = SharpDX.DataRectangle;

namespace XSharp.Interop;

public static class DataRectangleExtensions
{
    public static DX11DataRectanbgle ToDX9DataRectangle(this DataRectangle rect)
    {
        return new DX11DataRectanbgle(rect.DataPointer, rect.Pitch);
    }

    public static DataRectangle ToDataRectangle(this DX11DataRectanbgle rect)
    {
        return new DataRectangle(rect.DataPointer, rect.Pitch);
    }
}