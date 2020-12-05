using System;

namespace Jint.DebuggerExample.Debug
{
    public class ScriptLoaderException : Exception
    {
        public ScriptLoaderException(string message) : base(message)
        {
        }

        public ScriptLoaderException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
