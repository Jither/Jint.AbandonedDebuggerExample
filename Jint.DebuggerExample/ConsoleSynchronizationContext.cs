using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Jint.DebuggerExample
{
    public interface ISynchronizationQueue
    {
        void Add(Action chore);
    }

    public class ConsoleSynchronizationContext : SynchronizationContext
    {
        private ISynchronizationQueue queue;

        public ConsoleSynchronizationContext(ISynchronizationQueue queue)
        {
            this.queue = queue;
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            queue.Add(() => d(state));
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            queue.Add(() => d(state));
        }
    }
}
