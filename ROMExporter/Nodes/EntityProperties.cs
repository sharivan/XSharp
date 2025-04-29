using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSharp.Exporter.Map;

internal class EntityProperties(string className, dynamic properties)
{
    public string ClassName
    {
        get;
    } = className;

    public dynamic Properties
    {
        get;
    } = properties;
}