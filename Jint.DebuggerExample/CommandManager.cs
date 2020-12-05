using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Jint.DebuggerExample
{
    public class Command
    {
        public string Name { get; }
        public string Description { get; }
        public Action<string[]> Handler { get; }

        public Command(string name, string description, Action<string[]> handler)
        {
            Name = name;
            Description = description;
            Handler = handler;
        }
    }
    public class CommandManager
    {
        private Dictionary<string, Command> commands = new Dictionary<string, Command>();
        private Regex ArgumentSeparator = new Regex(@"[ \t]+");

        public bool Parse(string commandLine)
        {
            List<string> parts = ArgumentSeparator.Split(commandLine).Where(p => p != String.Empty).ToList();
            if (parts.Count == 0)
            {
                // No command is OK
                return true;
            }
            string commandName = parts[0].ToLowerInvariant();
            parts.RemoveAt(0);

            if (commands.TryGetValue(commandName, out Command command))
            {
                command.Handler(parts.ToArray());
                return true;
            }

            return false;
        }

        public void Add(Command command)
        {
            commands.Add(command.Name, command);
        }

        public string BuildHelp()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Debugger commands:");
            foreach (var command in commands.Values)
            {
                builder.AppendLine($"{command.Name,-15}{command.Description}");
            }
            return builder.ToString();
        }
    }
}
