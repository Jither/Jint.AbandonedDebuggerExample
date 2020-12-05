using Jint.DebuggerExample.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.DebuggerExample.UI
{
    public class ErrorDisplay : DisplayArea
    {
        private string error = null;

        public string Error
        {
            get => error;
            set
            {
                if (error != value)
                {
                    error = value;
                    Invalidate();
                }
            }
        }

        public ErrorDisplay(Display display) : base(display, new Bounds(0, -1, Length.Percent(100), 1))
        {
            
        }

        public override void Redraw()
        {
            display.DrawText(Colorizer.Foreground(error ?? string.Empty, Colors.Error), bounds);
        }
    }
}
