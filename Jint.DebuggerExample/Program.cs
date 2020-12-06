using Jint.DebuggerExample.Commands;
using Jint.DebuggerExample.Debug;
using Jint.DebuggerExample.UI;
using Jint.DebuggerExample.Utilities;
using Jint.Native;
using Jint.Native.Array;
using Jint.Native.Object;
using Jint.Runtime.Debugger;
using System;
using System.Collections.Generic;
using System.Text;
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
        private static InfoDisplay infoDisplay;
        private static HelpDisplay helpDisplay;

        private static AreaToggler mainDisplayToggler;

        static void Main(string[] args)
        {
            commandManager = new CommandManager()
                .Add(new Command("continue", "c", "Run without stepping", Run))
                .Add(new Command("pause", "p", "Pause execution", Pause))
                .Add(new Command("kill", "Cancel execution", Stop))
                .Add(new Command("step", "s", "Step into", StepInto))
                .Add(new Command("next", "n", "Step over", StepOver))
                .Add(new Command("up", "Step out", StepOut))
                .Add(new Command("break", "bp", "Toggle breakpoint", ToggleBreakPoint))
                .Add(new Command("list", "l", "Display source view", ShowSource))
                .Add(new Command("help", "h", "Display list of commands", ShowHelp))
                .Add(new Command("exit", "x", "Exit the debuger", Exit));

            SetupDebugger(args);

            SetupUI();

            prompt.Command += Prompt_Command;

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

            scriptDisplay = new ScriptDisplay(display, debugger, new Bounds(0, 0, Length.Percent(60), -2));
            infoDisplay = new InfoDisplay(display, new Bounds(Length.Percent(60), 0, Length.Percent(40), -2));
            helpDisplay = new HelpDisplay(display, new Bounds(0, 0, Length.Percent(100), -2));
            prompt = new Prompt(display, new Bounds(0, -2, Length.Percent(100), 1));
            errorDisplay = new ErrorDisplay(display, new Bounds(0, -1, Length.Percent(100), 1));

            mainDisplayToggler
                .Add(scriptDisplay)
                .Add(helpDisplay);

            display.Add(scriptDisplay);
            display.Add(helpDisplay);
            display.Add(infoDisplay);
            display.Add(prompt);
            display.Add(errorDisplay);
        }

        private static void SetupDebugger(string[] paths)
        {
            debugger = new Debugger();

            debugger.Pause += Debugger_Pause;
            debugger.Continue += Debugger_Continue;

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

        private static void Debugger_Continue()
        {
            // We're being called from the Jint thread. Execute on UI thread:
            Dispatcher.Invoke(() =>
            {
                scriptDisplay.ExecutingLocation = null;
            });
        }

        private static void UpdateDisplay(DebugInformation info)
        {
            string source = info.CurrentStatement.Location.Source;
            int startLine = info.CurrentStatement.Location.Start.Line;
            int endLine = info.CurrentStatement.Location.End.Line;

            scriptDisplay.Script = scriptLoader.GetScript(source);
            scriptDisplay.ExecutingLocation = info.CurrentStatement.Location;

            StringBuilder infoBuilder = new StringBuilder();
            infoBuilder.AppendLine(Colorizer.Foreground("Scopes", Colors.Header));
            infoBuilder.AppendLine(Colorizer.Foreground("Local", Colors.Header2));
            BuildScope(infoBuilder, info.Locals);

            infoBuilder.AppendLine(Colorizer.Foreground("Global", Colors.Header2));
            BuildScope(infoBuilder, info.Globals);

            infoBuilder.AppendLine();
            infoBuilder.AppendLine(Colorizer.Foreground("Call stack", Colors.Header));
            foreach (var item in info.CallStack)
            {
                infoBuilder.AppendLine($"  {item}");
            }
            infoDisplay.Content = infoBuilder.ToString();

            mainDisplayToggler.Show(scriptDisplay);
        }

        private static void BuildScope(StringBuilder builder, Dictionary<string, JsValue> scope)
        {
            foreach (var item in scope)
            {
                string value = item.Value switch
                {
                    ArrayInstance _ => "[...]",
                    ObjectInstance _ => "{...}",
                    _ => item.Value.ToString()
                };
                builder.AppendLine($"  {item.Key} : {value}");
            }
        }

        private static void Prompt_Command(string commandLine)
        {
            try
            {
                bool handled = commandManager.Parse(commandLine);
                if (!handled)
                {
                    throw new CommandException($"Unknown command '{commandLine}'. Type 'help' for a list of commands.");
                }
                else
                {
                    errorDisplay.Error = null;
                }
            }
            catch (CommandException ex)
            {
                errorDisplay.Error = ex.Message;
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

        private static void ToggleBreakPoint(string[] arguments)
        {
            string scriptId = scriptDisplay.Script.Id;
            int line;
            switch (arguments.Length)
            {
                case 1:
                    line = Int32.Parse(arguments[0]);
                    break;
                case 2:
                    scriptId = arguments[0];
                    line = Int32.Parse(arguments[1]);
                    break;
                default:
                    throw new CommandException("Usage: bp <script id> [line number]");
            }

            if (debugger.HasBreakPoint(scriptId, line))
            {
                debugger.RemoveBreakPoint(scriptId, line);
            }
            else
            {
                if (!debugger.TryAddBreakPoint(scriptId, line))
                {
                    throw new CommandException($"Failed adding breakpoint at {scriptId} line {line}");
                }
            }

            if (scriptDisplay.Script.Id == scriptId)
            {
                scriptDisplay.Invalidate();
            }
        }

        private static void ShowSource(string[] arguments)
        {
            mainDisplayToggler.Show(scriptDisplay);
        }

        private static void ShowHelp(string[] arguments)
        {
            var help = Colorizer.Foreground("Debugger commands", Colors.Header) +
                Environment.NewLine + 
                commandManager.BuildHelp();
            helpDisplay.Content = help;
            mainDisplayToggler.Show(helpDisplay);
        }

        private static void Exit(string[] arguments)
        {
            debugger.Cancel();
            prompt.Stop();
            display.Stop();
        }
    }
}
