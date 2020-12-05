using Jint.DebuggerExample.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Jint.DebuggerExample.UI
{
    public class MainDisplay : DisplayArea
    {
        private Regex NewLine = new Regex("\r?\n");

        private Display display;
        private IEnumerable<string> contentLines = Enumerable.Empty<string>();
        private string content = null;

        public string Content
        {
            get => content;
            set
            {
                if (content != value)
                {
                    content = value;
                    contentLines = NewLine.Split(content);
                    Redraw();
                }
            }
        }

        public MainDisplay(Display display)
        {
            this.display = display;
        }

        public override void Redraw()
        {
            Dispatcher.Invoke(() =>
            {
                int row = 0;
                foreach (string line in contentLines)
                {
                    if (row >= display.Rows - 2)
                    {
                        break;
                    }
                    string displayLine = line.Crop(display.Columns);
                    display.ReplaceLine(displayLine, row);
                    row++;
                }
            });
        }
    }
}
