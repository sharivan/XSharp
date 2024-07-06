using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Key = XSharp.Engine.Input.Key;
using DX11Key = SharpDX.DirectInput.Key;

namespace XSharp.Interop;

public static class KeyExtensions
{
    public static DX11Key ToDX9Key(this Key key)
    {
        return (DX11Key) key;
    }

    public static Key ToKey(this DX11Key key)
    {
        return (Key) key;
    }
}