using System;

using XSharp.Factories;

namespace XSharp.Engine.Entities;

public class DuplicateEntityNameException : DuplicateItemNameException<Entity>
{
    public DuplicateEntityNameException(string name) : this(name, $"Duplicate entity name '{name}'.")
    {
    }

    public DuplicateEntityNameException(string name, string message) : base(message)
    {
    }

    public DuplicateEntityNameException(string name, string message, Exception innerException) : base(message, message, innerException)
    {
    }
}