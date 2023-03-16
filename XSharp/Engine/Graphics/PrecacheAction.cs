﻿using System;
using System.Reflection;

using XSharp.Serialization;

namespace XSharp.Engine.Graphics;

public class PrecacheAction : ISerializable
{
    private string parent;

    public Type Type
    {
        get;
        private set;
    }

    public MethodInfo Method
    {
        get;
        internal set;
    } = null;

    public PrecacheAction Parent
    {
        get => parent != null ? GameEngine.Engine.precacheActions[parent] : null;

        internal set => parent = value?.Type.FullName;
    }

    public bool Called
    {
        get;
        internal set;
    } = false;

    internal PrecacheAction(Type type)
    {
        Type = type;
    }

    internal PrecacheAction(EngineBinarySerializer serializer)
    {
        Deserialize(serializer);
    }

    public void Deserialize(BinarySerializer input)
    {
        var serializer = (EngineBinarySerializer) input;

        parent = serializer.ReadString();

        string typeName = serializer.ReadString(false);
        Type = Type.GetType(typeName);

        bool hasMethod = serializer.ReadBool();
        Method = hasMethod ? serializer.ReadMethodInfo() : null;

        Called = serializer.ReadBool();
    }

    public void Serialize(BinarySerializer output)
    {
        var serializer = (EngineBinarySerializer) output;

        serializer.WriteString(parent);
        serializer.WriteString(Type.FullName, false);

        if (Method != null)
        {
            serializer.WriteBool(true);
            serializer.WriteMethodInfo(Method);
        }
        else
            serializer.WriteBool(false);

        serializer.WriteBool(Called);
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