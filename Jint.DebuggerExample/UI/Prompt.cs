using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jint.DebuggerExample.UI
{
    public class Prompt : DisplayArea
    {
        private bool running;
        private Display display;
        private string prompt = "debug>";

        public event Action<string> Command;

        public Prompt(Display display)
        {
            this.display = display;
        }

        public void Start()
        {
            Task.Run(Input);
        }

        public void Stop()
        {
            running = false;
        }

        public override void Redraw()
        {
            Dispatcher.Invoke(() =>
            {
                display.ReplaceLine(prompt + " ", display.Rows - 2);
                display.MoveCursor(prompt.Length + 1, display.Rows - 2);
            });
        }

        private void Input()
        {
            running = true;
            while (running)
            {
                Redraw();
                // Yeah, ReadLine is blocking, so can't cancel this thread if it's pending.
                // But we get a bit lucky here - when we cancel the thread due to an "exit" command,
                // we've just left the ReadLine call.
                string commandLine = Console.ReadLine();
                if (Command != null)
                {
                    Dispatcher.Invoke(() => Command(commandLine));
                }
            }
        }
    }
}