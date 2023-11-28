using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Engine.Input;

public interface IKeyboardState
{
    public bool IsPressed(Key key);
}