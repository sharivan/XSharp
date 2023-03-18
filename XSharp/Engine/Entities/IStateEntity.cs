using System;

namespace XSharp.Engine.Entities;

internal interface IStateEntity<TState> where TState : struct, Enum
{
    public TState State
    {
        get;
        set;
    }
}

internal interface IStateEntity<TState, TSubState> : IStateEntity<TState>
    where TState : struct, Enum
    where TSubState : struct, Enum
{
    public TSubState SubState
    {
        get;
        set;
    }
}