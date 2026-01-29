using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Engine.Entities;

[Flags]
public enum EntityAttributeIncludeFlags
{
    INCLUDE_NONE = 0,
    INCLUDE_ALL_INPUTS = 1,
    INCLUDE_ALL_OUTPUTS = 2
}