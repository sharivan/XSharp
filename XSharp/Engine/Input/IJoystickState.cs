using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Engine.Input;

public interface IJoystickState
{
    public bool[] Buttons
    {
        get;
    }

    public int[] PointOfViewControllers
    {
        get;
    }
}