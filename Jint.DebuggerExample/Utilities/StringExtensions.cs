using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.DebuggerExample.Utilities
{
    public static class StringExtensions
    {
        public static string Crop(this string str, int maxLength)
        {
            if (maxLength >= str.Length)
            {
                return str;
            }
            return str.Substring(0, maxLength);
        }
    }
}
