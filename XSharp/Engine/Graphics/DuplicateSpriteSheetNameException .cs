using System;

namespace XSharp.Engine.Graphics;

public class DuplicateSpriteSheetNameException : Exception
{
    public string Name
    {
        get;
    }

    public DuplicateSpriteSheetNameException(string name) : this(name, $"Duplicate sprite sheet name '{name}'.")
    {
    }

    public DuplicateSpriteSheetNameException(string name, string message) : base(message)
    {
        Name = name;
    }

    public DuplicateSpriteSheetNameException(string name, string message, Exception innerException) : base(message, innerException)
    {
        Name = name;
    }
}