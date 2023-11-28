using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Key = XSharp.Engine.Input.Key;
using DX9Key = SharpDX.DirectInput.Key;

namespace XSharp.Interop;

public static class KeyExtensions
{
    public static DX9Key ToDX9Key(this Key key)
    {
        return (DX9Key) key;
    }

    public static Key ToKey(this DX9Key key)
    {
        return (Key) key;
    }
}