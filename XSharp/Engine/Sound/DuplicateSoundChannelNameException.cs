using System;

namespace XSharp.Engine.Sound;

public class DuplicateSoundChannelNameException : Exception
{
    public string Name
    {
        get;
    }

    public DuplicateSoundChannelNameException(string name) : this(name, $"Duplicate sound channel name '{name}'.")
    {
    }

    public DuplicateSoundChannelNameException(string name, string message) : base(message)
    {
        Name = name;
    }

    public DuplicateSoundChannelNameException(string name, string message, Exception innerException) : base(message, innerException)
    {
        Name = name;
    }
}