using System;

namespace XSharp.Engine.Entities;

internal interface IFSMEntity<TState> where TState : struct, Enum
{
    public TState State
    {
        get;
        set;
    }
}

internal interface IFSMEntity<TState, TSubState> : IFSMEntity<TState>
    where TState : struct, Enum
    where TSubState : struct, Enum
{
    public TSubState SubState
    {
        get;
        set;
    }
}