using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Engine.Entities;

[AttributeUsage(AttributeTargets.Event)]
public class OutputAttribute(string name = null) : Attribute
{
    public string Name
    {
        get;
    } = name;
}