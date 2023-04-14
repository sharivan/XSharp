using System;
using System.Reflection;

using XSharp.Serialization;

namespace XSharp.Engine.Graphics;

public class PrecacheAction : ISerializable
{
    private string parent;
    private bool calling = false;

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
        get => parent != null ? BaseEngine.Engine.precacheActions[parent] : null;

        internal set => parent = value?.Type.AssemblyQualifiedName;
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

    public void Deserialize(ISerializer input)
    {
        var serializer = (EngineBinarySerializer) input;

        parent = serializer.ReadString();

        string typeName = serializer.ReadString(false);
        Type = Type.GetType(typeName, true);

        bool hasMethod = serializer.ReadBool();
        Method = hasMethod ? serializer.ReadMethodInfo() : null;

        Called = serializer.ReadBool();
    }

    public void Serialize(ISerializer output)
    {
        var serializer = (EngineBinarySerializer) output;

        serializer.WriteString(parent);
        serializer.WriteString(Type.AssemblyQualifiedName, false);

        if (Method != null)
        {
            serializer.WriteBool(true);
            serializer.WriteMethodInfo(Method);
        }
        else
            serializer.WriteBool(false);

        serializer.WriteBool(Called);
    }

    internal void Reset(bool recursive = true)
    {
        Called = false;

        if (recursive)
            Parent?.Reset(true);
    }

    internal void Call()
    {
        if (calling)
            return;

        calling = true;

        try
        {
            Parent?.Call();

            if (!Called)
            {
                Method?.Invoke(null, null);
                Called = true;
            }
        }
        finally
        {
            calling = false;
        }
    }
}