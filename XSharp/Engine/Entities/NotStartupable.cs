using System;

namespace XSharp.Engine.Entities;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class NotStartupableAttribute : Attribute
{
}