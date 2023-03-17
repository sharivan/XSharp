using System;

namespace XSharp.Engine.Sound;

public class DuplicatePrecachedSoundNameException : Exception
{
    public string Name
    {
        get;
    }

    public DuplicatePrecachedSoundNameException(string name) : this(name, $"Duplicate precached sound name '{name}'.")
    {
    }

    public DuplicatePrecachedSoundNameException(string name, string message) : base(message)
    {
        Name = name;
    }

    public DuplicatePrecachedSoundNameException(string name, string message, Exception innerException) : base(message, innerException)
    {
        Name = name;
    }
}