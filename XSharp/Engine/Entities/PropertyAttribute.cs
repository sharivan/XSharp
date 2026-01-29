using System;

namespace XSharp.Engine.Entities;

[AttributeUsage(AttributeTargets.Property)]
public class PropertyAttribute(string name = null) : Attribute
{
    public string Name
    {
        get;
    } = name;
}