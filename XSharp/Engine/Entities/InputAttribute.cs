using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Engine.Entities;

[AttributeUsage(AttributeTargets.Method)]
public class InputAttribute(string name = null) : Attribute
{
    public string Name
    {
        get;
    } = name;
}