using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Jint.DebuggerExample.Debug
{
    public class ScriptLoader
    {
        private readonly Debugger debugger;

        private Dictionary<string, ScriptData> scripts = new Dictionary<string, ScriptData>(StringComparer.OrdinalIgnoreCase);

        public ScriptLoader(Debugger debugger)
        {
            this.debugger = debugger;
        }

        public ScriptData GetScript(string id)
        {
            if (scripts.TryGetValue(id, out ScriptData result))
            {
                return result;
            }
            return null;
        }

        public void Load(string path)
        {
            try
            {
                string source = File.ReadAllText(path);
                var script = new ScriptData(GetId(path), path, source);
                scripts.Add(script.Id, script);
                debugger.AddScript(script);
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is PathTooLongException || ex is DirectoryNotFoundException || ex is IOException || ex is UnauthorizedAccessException)
            {
                string fileName = Path.GetFileName(path);
                throw new ScriptLoaderException($"Could not load '{fileName}': {ex.Message}", ex);
            }
        }

        private string GetId(string path)
        {
            // Just to address most common use case, we only include filename in ID.
            // If we already have a script with the same file name, append an index
            string fileName = Path.GetFileName(path);
            int index = 1;
            string id = fileName;
            do
            {
                id = index > 1 ? $"{fileName} ({index})" : fileName;
                index++;
            }
            while (scripts.ContainsKey(id));
            return id;
        }
    }
}
