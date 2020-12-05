using System;

namespace Jint.DebuggerExample
{
    public interface ISynchronizationQueue
    {
        void Add(Action chore);
    }
}
