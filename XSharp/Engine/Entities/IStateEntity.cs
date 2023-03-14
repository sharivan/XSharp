using System;

namespace XSharp.Engine.Entities;

internal interface IStateEntity<T> where T : struct, Enum
{
    public T State
    {
        get;
        set;
    }
}