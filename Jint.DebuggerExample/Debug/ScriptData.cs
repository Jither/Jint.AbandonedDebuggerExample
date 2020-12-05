using Esprima.Ast;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Jint.DebuggerExample.Debug
{
    public class ScriptData
    {
        private static Regex NewLine = new Regex(@"\r?\n");

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

            Lines = NewLine.Split(script);
        }
    }
}
