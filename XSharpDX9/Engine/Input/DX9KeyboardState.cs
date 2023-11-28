using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.DirectInput;

using XSharp.Interop;

namespace XSharp.Engine.Input;

public class DX9KeyboardState : IKeyboardState
{
    internal KeyboardState state;

    public DX9KeyboardState()
    {
    }

    public DX9KeyboardState(KeyboardState state)
    {
        this.state = state;
    }

    public bool IsPressed(Key key)
    {
        return state.IsPressed(key.ToDX9Key());
    }
}