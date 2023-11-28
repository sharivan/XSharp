using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Engine.Input;

public interface ICapabilities
{
    public int ButtonCount
    {
        get;
    }
}