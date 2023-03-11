using System;

namespace XSharp.Serialization;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class NotSerializableAttribute : Attribute
{
}