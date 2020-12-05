using System;
using System.Collections.Generic;

namespace Jint.DebuggerExample.Commands
{
    public class CommandException : Exception
    {
        public CommandException(string message) : base(message)
        {
        }
    }

    public class Command
    {
        public string Name { get; }
        public string ShortName { get; }
        public string Description { get; }
        public Action<string[]> Handler { get; }

        public Command(string name, string shortName, string description, Action<string[]> handler)
        {
            Name = name;
            ShortName = shortName;
            Description = description;
            Handler = handler;
        }

        public Command(string name, string description, Action<string[]> handler) : this(name, null, description, handler)
        {
        }
    }
}
