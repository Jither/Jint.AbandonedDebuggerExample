using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Jint.DebuggerExample.Commands
{
    public class CommandManager
    {
        private Dictionary<string, Command> commandsByName = new Dictionary<string, Command>();
        private List<Command> commands = new List<Command>();
        private Regex ArgumentSeparator = new Regex(@"[ \t]+");

        public bool Parse(string commandLine)
        {
            List<string> parts = ArgumentSeparator.Split(commandLine).Where(p => p != string.Empty).ToList();
            if (parts.Count == 0)
            {
                // No command is OK
                return true;
            }
            string commandName = parts[0].ToLowerInvariant();
            parts.RemoveAt(0);

            if (commandsByName.TryGetValue(commandName, out Command command))
            {
                command.Handler(parts.ToArray());
                return true;
            }

            return false;
        }

        public CommandManager Add(Command command)
        {
            commands.Add(command);
            commandsByName.Add(command.Name, command);
            if (command.ShortName != null)
            {
                commandsByName.Add(command.ShortName, command);
            }
            return this;
        }

        public string BuildHelp()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var command in commands)
            {
                builder.AppendLine($"    {command.Name,-15} {command.ShortName,-3} {command.Description}");
            }
            return builder.ToString();
        }
    }
}
