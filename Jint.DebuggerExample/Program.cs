using Jint.DebuggerExample.UI;
using System;
using System.Threading;

namespace Jint.DebuggerExample
{
    class Program
    {
        private static CommandManager commandManager;
        private static Display display;
        private static MainDisplay mainDisplay;
        private static Prompt prompt;
        private static ErrorDisplay errorDisplay;

        static void Main(string[] args)
        {
            commandManager = new CommandManager();
            commandManager.Add(new Command("help", "Displays list of commands", Help));
            commandManager.Add(new Command("exit", "Exit the debuger", Exit));
            
            display = new Display();
            
            Dispatcher.Init(display);
            SynchronizationContext.SetSynchronizationContext(new ConsoleSynchronizationContext(display));

            mainDisplay = new MainDisplay(display);

            prompt = new Prompt(display);
            prompt.Command += Prompt_Command;

            errorDisplay = new ErrorDisplay(display);

            display.Add(mainDisplay);
            display.Add(prompt);
            display.Add(errorDisplay);

            prompt.Start();
            display.Start();
        }

        private static void Prompt_Command(string commandLine)
        {
            bool handled = commandManager.Parse(commandLine);
            if (!handled)
            {
                errorDisplay.Error = $"Unknown command '{commandLine}'. Type 'help' for a list of commands.";
            }
            else
            {
                errorDisplay.Error = null;
            }
        }

        private static void Help(string[] arguments)
        {
            var help = commandManager.BuildHelp();
            mainDisplay.Content = help;
        }

        private static void Exit(string[] arguments)
        {
            prompt.Stop();
            display.Stop();
        }
    }
}
