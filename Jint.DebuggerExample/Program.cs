using Jint.DebuggerExample.Commands;
using Jint.DebuggerExample.Debug;
using Jint.DebuggerExample.UI;
using Jint.Runtime.Debugger;
using System;
using System.Threading;

namespace Jint.DebuggerExample
{
    class Program
    {
        private static CommandManager commandManager;
        private static Debugger debugger;
        private static ScriptLoader scriptLoader;

        private static Display display;
        private static ScriptDisplay scriptDisplay;
        private static Prompt prompt;
        private static ErrorDisplay errorDisplay;
        private static TextDisplay textDisplay;

        private static AreaToggler mainDisplayToggler;

        static void Main(string[] args)
        {
            commandManager = new CommandManager()
                .Add(new Command("run", "Run without stepping", Run))
                .Add(new Command("pause", "Pause execution", Pause))
                .Add(new Command("stop", "Pause execution", Stop))
                .Add(new Command("out", "Step out", StepOut))
                .Add(new Command("over", "Step over", StepOver))
                .Add(new Command("into", "Step into", StepInto))
                .Add(new Command("help", "Display list of commands", ShowHelp))
                .Add(new Command("source", "Display source view", ShowSource))
                .Add(new Command("exit", "Exit the debuger", Exit));

            SetupUI();

            prompt.Command += Prompt_Command;

            SetupDebugger(args);

            prompt.Start();
            display.Start();
        }

        private static void SetupUI()
        {
            display = new Display();
            display.Ready += Display_Ready;

            // Simple Dispatcher and SynchronizationContext do the same job as in a typical GUI:
            // - SynchronizationContext Keeps async code continuations running on the main (UI) thread
            // - Dispatcher allows other threads to invoke Actions on the main (UI) thread
            SynchronizationContext.SetSynchronizationContext(new ConsoleSynchronizationContext(display));
            Dispatcher.Init(display);

            mainDisplayToggler = new AreaToggler();

            textDisplay = new TextDisplay(display);
            scriptDisplay = new ScriptDisplay(display);

            mainDisplayToggler
                .Add(textDisplay)
                .Add(scriptDisplay);

            prompt = new Prompt(display);

            errorDisplay = new ErrorDisplay(display);

            display.Add(scriptDisplay);
            display.Add(textDisplay);
            display.Add(prompt);
            display.Add(errorDisplay);
        }

        private static void SetupDebugger(string[] paths)
        {
            debugger = new Debugger();

            debugger.Pause += Debugger_Pause;

            scriptLoader = new ScriptLoader(debugger);

            foreach (var path in paths)
            {
                scriptLoader.Load(path);
            }
        }

        private static void Display_Ready()
        {
            debugger.Execute();
        }

        private static void Debugger_Pause(DebugInformation info)
        {
            // We're being called from the Jint thread. Execute on UI thread:
            Dispatcher.Invoke(() => UpdateDisplay(info));
        }

        private static void UpdateDisplay(DebugInformation info)
        {
            string source = info.CurrentStatement.Location.Source;
            int startLine = info.CurrentStatement.Location.Start.Line;
            int endLine = info.CurrentStatement.Location.End.Line;

            scriptDisplay.Script = scriptLoader.GetScript(source);
            scriptDisplay.ExecutingLocation = info.CurrentStatement.Location;

            mainDisplayToggler.Show(scriptDisplay);
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

        private static void Run(string[] arguments)
        {
            debugger.Run();
        }

        private static void Pause(string[] arguments)
        {
            debugger.Stop();
        }

        private static void Stop(string[] arguments)
        {
            debugger.Cancel();
        }

        private static void StepOver(string[] arguments)
        {
            debugger.StepOver();
        }

        private static void StepOut(string[] arguments)
        {
            debugger.StepOut();
        }

        private static void StepInto(string[] arguments)
        {
            debugger.StepInto();
        }

        private static void ShowSource(string[] arguments)
        {
            mainDisplayToggler.Show(scriptDisplay);
        }

        private static void ShowHelp(string[] arguments)
        {
            var help = commandManager.BuildHelp();
            textDisplay.Content = help;
            mainDisplayToggler.Show(textDisplay);
        }

        private static void Exit(string[] arguments)
        {
            debugger.Cancel();
            prompt.Stop();
            display.Stop();
        }
    }
}
