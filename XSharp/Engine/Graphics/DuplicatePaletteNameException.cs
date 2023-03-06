using System;

namespace XSharp.Engine.Graphics;

public class DuplicatePaletteNameException : Exception
{
    public string Name
    {
        get;
    }

    public DuplicatePaletteNameException(string name) : this(name, $"Duplicate palette name '{name}'.")
    {
    }

    public DuplicatePaletteNameException(string name, string message) : base(message)
    {
        Name = name;
    }

    public DuplicatePaletteNameException(string name, string message, Exception innerException) : base(message, innerException)
    {
        Name = name;
    }
}