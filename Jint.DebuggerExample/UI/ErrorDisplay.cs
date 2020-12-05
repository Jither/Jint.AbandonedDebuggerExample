using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.DebuggerExample.UI
{
    public class ErrorDisplay : DisplayArea
    {
        private Display display;
        private string error = null;

        public string Error
        {
            get => error;
            set
            {
                if (error != value)
                {
                    error = value;
                    Redraw();
                }
            }
        }

        public ErrorDisplay(Display display)
        {
            this.display = display;
        }

        public override void Redraw()
        {
            display.ReplaceLine(error ?? string.Empty, display.Rows - 1);
        }
    }
}
