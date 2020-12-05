using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.DebuggerExample.Utilities
{
    public class RgbColor
    {
        public byte Red { get; }
        public byte Green { get; }
        public byte Blue { get; }

        public RgbColor(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }
    }
    public static class Colorizer
    {
        public static string Foreground(string text, RgbColor color)
        {
            return $"\u001b[38;2;{color.Red};{color.Green};{color.Blue}m{text}\u001b[0m";
        }
    }
}
