using System;
using System.Reflection;

namespace XSharp.Engine.Graphics;

public class PrecacheAction
{
    public Type Type
    {
        get;
    }

    public MethodInfo Method
    {
        get;
        internal set;
    } = null;

    public PrecacheAction Parent
    {
        get;
        internal set;
    } = null;

    public bool Called
    {
        get;
        internal set;
    } = false;

    internal PrecacheAction(Type type)
    {
        Type = type;
    }

    internal void Reset()
    {
        Called = false;
        Parent?.Reset();
    }

    internal void Call()
    {
        Parent?.Call();

        if (!Called)
        {
            Method?.Invoke(null, null);
            Called = true;
        }
    }
}