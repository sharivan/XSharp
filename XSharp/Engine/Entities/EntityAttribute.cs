using System;

namespace XSharp.Engine.Entities;

[AttributeUsage(AttributeTargets.Class)]
public class EntityAttribute(string className = null, EntityAttributeIncludeFlags flags = EntityAttributeIncludeFlags.INCLUDE_NONE) : Attribute
{
    public string ClassName { get; } = className;

    public EntityAttributeIncludeFlags Flags { get; } = flags;
}