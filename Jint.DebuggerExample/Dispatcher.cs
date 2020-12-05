using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Jint.DebuggerExample
{
    public static class Dispatcher
    {
        private static ISynchronizationQueue _queue;

        public static void Init(ISynchronizationQueue queue)
        {
            _queue = queue;
        }

        public static void Invoke(Action action)
        {
            _queue.Add(action);
        }
    }
}
