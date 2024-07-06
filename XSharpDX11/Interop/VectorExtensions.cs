using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Vector2 = XSharp.Math.Geometry.Vector2;
using Vector3 = XSharp.Math.Geometry.Vector3;
using Vector4 = XSharp.Math.Geometry.Vector4;
using DX11Vector2 = SharpDX.Vector2;
using DX11Vector3 = SharpDX.Vector3;
using DX11Vector4 = SharpDX.Vector4;

namespace XSharp.Interop;

public static class VectorExtensions
{
    public static DX11Vector2 ToDX9Vector2(this Vector2 vec)
    {
        return new DX11Vector2(vec.X, vec.Y);
    }

    public static Vector2 ToVector2(this DX11Vector2 vec)
    {
        return new Vector2(vec.X, vec.Y);
    }

    public static DX11Vector3 ToDX9Vector3(this Vector3 vec)
    {
        return new DX11Vector3(vec.X, vec.Y, vec.Z);
    }

    public static Vector3 ToVector3(this DX11Vector3 vec)
    {
        return new Vector3(vec.X, vec.Y, vec.Z);
    }

    public static DX11Vector4 ToDX9Vector4(this Vector4 vec)
    {
        return new DX11Vector4(vec.X, vec.Y, vec.Z, vec.W);
    }

    public static Vector4 ToVector4(this DX11Vector4 vec)
    {
        return new Vector4(vec.X, vec.Y, vec.Z, vec.W);
    }
}