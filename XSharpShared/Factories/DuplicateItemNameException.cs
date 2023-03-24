using System;

namespace XSharp.Factories;

public class DuplicateItemNameException<ItemType> : Exception where ItemType : INamedFactoryItem
{
    public string Name
    {
        get;
    }

    public DuplicateItemNameException(string name) : this(name, $"Duplicate item name '{name}'.")
    {
    }

    public DuplicateItemNameException(string name, string message) : base(message)
    {
        Name = name;
    }

    public DuplicateItemNameException(string name, string message, Exception innerException) : base(message, innerException)
    {
        Name = name;
    }
}