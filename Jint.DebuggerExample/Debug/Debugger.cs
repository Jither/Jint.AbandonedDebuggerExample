using Esprima;
using Esprima.Ast;
using Jint.Runtime.Debugger;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jint.DebuggerExample.Debug
{
    /// <summary>
    /// Exception used to cancel execution of scripts in the debugger.
    /// </summary>
    public class CancelExecutionException : Exception
    {

    }

    public class Debugger
    {
        /// <summary>
        /// Triggered when a breakpoint or step is hit. This event is where UI would be updated.
        /// </summary>
        public event Action<DebugInformation> Pause;

        /// <summary>
        /// Triggered when debugger is done executing all its scripts.
        /// </summary>
        public event Action Done;

        public bool IsRunning { get; private set; }

        private readonly Queue<ScriptData> scripts = new Queue<ScriptData>();
        private readonly Engine engine;

        private StepMode nextStep;
        private bool noStepping;

        private readonly ManualResetEvent waitForInterface = new ManualResetEvent(false);
        private CancellationTokenSource cts;

        public Debugger()
        {
            // Tell Jint to use debug mode and to trigger a Break event when encountering
            // a "debugger" JavaScript statement.
            engine = new Engine(options => options
                .DebugMode()
                .DebuggerStatementHandling(DebuggerStatementHandling.Script));

            engine.Step += Engine_Step;
            engine.Break += Engine_Break;
        }

        /// <summary>
        /// Adds a script to the execution queue.
        /// </summary>
        /// <param name="data">Script metadata</param>
        public void AddScript(ScriptData data)
        {
            // We need to parse the script ourselves (rather than using the Jint Engine constructor
            // that takes the source code as string) - in order to get the parser to assign our ID
            // as Source in the AST nodes' Location property.
            var parser = new JavaScriptParser(data.Script, new ParserOptions(data.Id)
            {
                AdaptRegexp = true,
                Loc = true,
                Tolerant = true
            });
            data.Ast = parser.ParseScript();

            scripts.Enqueue(data);
        }

        /// <summary>
        /// Executes the scripts in the debugger.
        /// </summary>
        public void Execute()
        {
            IsRunning = true;
            cts = new CancellationTokenSource();
            try
            {
                // We run the debugger script execution on a separate thread. Why?
                // In a GUI app this is necessary, since Jint signals reaching a breakpoint
                // or step by triggering an event. When the event handler returns, script execution
                // continues. That means we'd be blocking the UI thread until the entire script is done
                // executing (if it ever is).
                // In a console app, it would be doable - do e.g. a Console.ReadLine when handling
                // the Step/Break event. However, we also want to be able to run a script without stepping,
                // and then stop/pause it if, for example, it ends up in an infinite loop. So, even in our
                // console app, the "UI thread" (Console read/write) must not be blocked while Jint is
                // executing the script.
                Task.Run(() =>
                {
                    try
                    {
                        while (scripts.Count > 0)
                        {
                            ScriptData script = scripts.Dequeue();
                            engine.Execute(script.Ast);
                        }
                    }
                    catch (CancelExecutionException)
                    {
                        // Do nothing - the exception is just used to signal cancellation.
                    }
                    Done?.Invoke();
                }, cts.Token);
            }
            finally
            {
                IsRunning = false;
            }
        }

        private StepMode OnPause(DebugInformation e)
        {
            if (cts.IsCancellationRequested)
            {
                throw new CancelExecutionException();
            }
            if (noStepping)
            {
                // If we aren't stepping, immediately step (into) to the next statement.
                return StepMode.Into;
            }

            IsRunning = false;
            Pause?.Invoke(e);

            // We've now handed control over to the UI. Keep the debugger thread
            // waiting until the UI gets back to us:
            waitForInterface.WaitOne();
            // ... then start the cycle over:
            waitForInterface.Reset();

            IsRunning = true;

            return nextStep;
        }

        /// <summary>
        /// Runs the script without stepping.
        /// </summary>
        public void Run()
        {
            // We can't use StepMode.None here. We want to be able to pause again manually (see Stop), but
            // StepMode.None will stop the engine sending Step events at all, meaning we also can't
            // change the StepMode later (except if we hit a Break event).
            // So, instead we simply set a flag and check it in the Step event
            noStepping = true;
        }

        /// <summary>
        /// Stops (pauses) the script (after calling <see cref="Run"/>)
        /// </summary>
        public void Stop()
        {
            noStepping = false;
        }
        /// <summary>
        /// Advances one step in the script - stepping into functions and other callables.
        /// </summary>
        public void StepInto()
        {
            nextStep = StepMode.Into;
            waitForInterface.Set();
        }

        /// <summary>
        /// Advances one step in the script - stepping over functions and other callables.
        /// </summary>
        public void StepOver()
        {
            nextStep = StepMode.Over;
            waitForInterface.Set();
        }

        /// <summary>
        /// Steps out of the current function or other callable, advancing to the statement right after the call.
        /// </summary>
        public void StepOut()
        {
            nextStep = StepMode.Out;
            waitForInterface.Set();
        }

        /// <summary>
        /// Cancels script execution.
        /// </summary>
        public void Cancel()
        {
            cts.Cancel();
        }

        private StepMode Engine_Break(object sender, DebugInformation e)
        {
            return OnPause(e);
        }

        private StepMode Engine_Step(object sender, DebugInformation e)
        {
            return OnPause(e);
        }
    }
}
