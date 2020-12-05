using Esprima.Ast;
using Jint.DebuggerExample.Utilities;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Jint.DebuggerExample.Debug
{
    public class ScriptData
    {
        public string Id { get; }
        public string Path { get; }
        public string Script { get; }
        public Script Ast { get; set; }

        public IList<string> Lines { get; }

        public ScriptData(string id, string path, string script)
        {
            Id = id;
            Path = path;
            Script = script;

            Lines = Script.SplitIntoLines();
        }
    }
}
