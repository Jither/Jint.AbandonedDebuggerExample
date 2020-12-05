using Jint.DebuggerExample.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Jint.DebuggerExample.UI
{
    public class TextDisplay : DisplayArea
    {
        private string content = null;

        public string Content
        {
            get => content;
            set
            {
                if (content != value)
                {
                    content = value;
                    Invalidate();
                }
            }
        }

        public TextDisplay(Display display) : base(display, new Bounds(0, 0, Length.Percent(100), -2))
        {
        }

        public override void Redraw()
        {
            display.DrawText(content, bounds);
        }
    }
}
