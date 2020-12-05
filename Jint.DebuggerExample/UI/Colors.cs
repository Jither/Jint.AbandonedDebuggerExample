using Jint.DebuggerExample.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.DebuggerExample.UI
{
    public static class Colors
    {
        public static RgbColor Header = new RgbColor(0x60, 0xa0, 0xff);
        public static RgbColor Error = new RgbColor(0xff, 0x30, 0x10);
        public static RgbColor ExecutingLine = new RgbColor(0xff, 0xc0, 0x00);
        public static RgbColor BreakPoint = new RgbColor(0x80, 0, 0);
    }
}
