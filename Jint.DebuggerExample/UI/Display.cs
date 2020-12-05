using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jint.DebuggerExample.UI
{
    public class Display : ISynchronizationQueue
    {
        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public event Action Resize;
        public event Action Ready;

        private Thread uiThread;

        private List<Action> chores = new List<Action>();
        private int cursorLeft;
        private int cursorTop;
        private bool running;
        private List<DisplayArea> areas = new List<DisplayArea>();

        private ManualResetEventSlim waitForSizePoll = new ManualResetEventSlim(false);
        private ManualResetEventSlim waitForResize = new ManualResetEventSlim(false);
        private bool windowWasResized;


        public void Start()
        {
            uiThread = Thread.CurrentThread;
            Init();
            ResizeScreen();
            Clear();
            RenderLoop();
        }

        public void Add(DisplayArea area)
        {
            areas.Add(area);
        }

        public void Stop()
        {
            running = false;
        }

        public void WriteAt(string message, int left, int top)
        {
            CheckRunningOnUIThread();
            if (top < 0)
            {
                // This will happen when the window is very small - since some areas will draw counting from the bottom
                return;
            }
            Console.SetCursorPosition(left, top);
            Console.Write(message);
            ResetCursor();
        }

        public void ReplaceLine(string message, int top)
        {
            CheckRunningOnUIThread();
            message = message.PadRight(Columns, ' ');
            WriteAt(message, 0, top);
        }

        public void MoveCursor(int left, int top)
        {
            CheckRunningOnUIThread();
            // Area may have calculated negative cursor positions
            // (e.g. when height of window is small and the area draws counting from bottom)
            cursorLeft = Math.Max(0, left);
            cursorTop = Math.Max(0 ,top);
            ResetCursor();
        }

        private void CheckRunningOnUIThread()
        {
            if (Thread.CurrentThread != uiThread)
            {
                throw new InvalidOperationException("UI method called from non-UI thread");
            }
        }

        private void Init()
        {
            Task.Run(MonitorWindowSize);
        }

        private void MonitorWindowSize()
        {
            while (true)
            {
                waitForSizePoll.Wait();
                waitForSizePoll.Reset();
                while (true)
                {
                    if (Console.WindowWidth != Columns || Console.WindowHeight != Rows)
                    {
                        break;
                    }
                }
                windowWasResized = true;
                waitForResize.Set();
            }
        }

        private void ResizeScreen()
        {
            if (Console.WindowHeight == 0)
            {
                // This would cause IOExceptions further down. We'll keep the size from before.
                return;
            }
            Rows = Console.WindowHeight;
            Columns = Console.WindowWidth;
            try
            {
                // Be sure to reset cursor and window position-in-buffer.
                // Resizing the buffer size to smaller than the cursor position will throw exception.
                cursorLeft = 0;
                cursorTop = 0;
                ResetCursor();
                Console.SetWindowPosition(0, 0);

                // We'll get an IOException if we make buffer narrower than 14 columns:
                int columns = Math.Max(Columns, 15);
                Console.SetBufferSize(columns, Rows);

                Redraw();
                Resize?.Invoke();
                windowWasResized = false;
            }
            catch (ArgumentOutOfRangeException)
            {
                // Just in case exception is thrown by SetBufferSize anyway
            }
        }

        private void Redraw()
        {
            Console.CursorVisible = false;
            try
            {
                Clear();
                foreach (var area in areas)
                {
                    area.Redraw();
                }
            }
            finally
            {
                Console.CursorVisible = true;
            }
        }

        private void Clear()
        {
            Console.Clear();
        }

        private void RenderLoop()
        {
            running = true;
            Ready?.Invoke();

            while (running)
            {
                if (EventsPending())
                {
                    HandleEvents();
                }
                lock (chores)
                {
                    if (chores.Count > 0)
                    {
                        RunChores();
                    }
                }
            }
        }

        private bool EventsPending()
        {
            waitForSizePoll.Set();
            waitForResize.Wait(0);
            return windowWasResized;
        }

        private void HandleEvents()
        {
            if (windowWasResized)
            {
                ResizeScreen();
            }
        }

        private void ResetCursor()
        {
            Console.SetCursorPosition(cursorLeft, cursorTop);
        }

        public void Add(Action chore)
        {
            lock (chores)
            {
                chores.Add(chore);
            }
        }

        public void RunChores()
        {
            List<Action> pendingChores;
            lock (chores)
            {
                pendingChores = chores;
                chores = new List<Action>();
            }

            foreach (var chore in pendingChores)
            {
                chore();
            }
        }
    }
}
