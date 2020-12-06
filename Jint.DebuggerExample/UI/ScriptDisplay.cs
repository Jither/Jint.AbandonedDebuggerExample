using Esprima;
using Jint.DebuggerExample.Debug;
using Jint.DebuggerExample.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Jint.DebuggerExample.UI
{
    public class ScriptDisplay : DisplayArea
    {
        private ScriptData script;
        private Debugger debugger;
        private Location? executingLocation;

        public ScriptData Script
        {
            get => script;
            set
            {
                if (script != value)
                {
                    script = value;
                    Invalidate();
                }
            }
        }

        public Location? ExecutingLocation
        {
            get => executingLocation;
            set
            {
                if (executingLocation != value)
                {
                    executingLocation = value;
                    Invalidate();
                }
            }
        }

        public ScriptDisplay(Display display, Debugger debugger, Bounds bounds) : base(display, bounds)
        {
            this.debugger = debugger;
        }

        public override void Redraw()
        {
            var lines = script?.Lines;
            if (lines == null)
            {
                return;
            }

            int lineNumberWidth = lines.Count.ToString().Length;

            List<string> displayLines = new List<string>();
            displayLines.Add(Colorizer.Foreground($"Script: {script.Id}", Colors.Header));
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];

                // Tabs are "transparent" in the console - i.e., they'll skip a number of cells,
                // without overwriting what's already there. So, we convert them to spaces here
                line = line.TabsToSpaces(4);

                string lineNumber = " " + (i + 1).ToString().PadLeft(lineNumberWidth) + " ";

                if (debugger.HasBreakPoint(script.Id, i + 1))
                {
                    lineNumber = Colorizer.Background(lineNumber, Colors.BreakPoint);
                }

                if (executingLocation != null && executingLocation.Value.Start.Line == i + 1)
                {
                    line = Colorizer.Foreground(line, Colors.ExecutingLine);
                }
                string displayLine = lineNumber + " " + line;
                displayLines.Add(displayLine);
            }

            display.DrawText(displayLines, bounds);
        }
    }
}
