using System;

namespace XSharp.Engine.Entities
{
    public class DuplicateEntityNameException : Exception
    {
        public string Name
        {
            get;
        }

        public DuplicateEntityNameException(string name) : this(name, $"Duplicate entity name '{name}'.")
        {
        }

        public DuplicateEntityNameException(string name, string message) : base(message)
        {
            Name = name;
        }

        public DuplicateEntityNameException(string name, string message, Exception innerException) : base(message, innerException)
        {
            Name = name;
        }
    }
}