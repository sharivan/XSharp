using System;

using XSharp.Factories;

namespace XSharp.Engine.Graphics;

public class DuplicateAnimationNameException : DuplicateItemNameException<Animation>
{
    public DuplicateAnimationNameException(string name) : base(name, $"Duplicate animation name '{name}'.")
    {
    }

    public DuplicateAnimationNameException(string name, string message) : base(name, message)
    {
    }

    public DuplicateAnimationNameException(string name, string message, Exception innerException) : base(name, message, innerException)
    {
    }
}