using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Engine.Input;

public class DX9JoystickState : IJoystickState
{
    internal JoystickState state;

    public bool[] Buttons => state.Buttons;

    public int[] PointOfViewControllers => state.PointOfViewControllers;

    public DX9JoystickState()
    {
    }

    public DX9JoystickState(JoystickState state)
    {
        this.state = state;
    }
}