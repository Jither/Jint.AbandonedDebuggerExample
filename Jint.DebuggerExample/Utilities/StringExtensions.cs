using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Jint.DebuggerExample.Utilities
{
    public static class StringExtensions
    {
        private static Regex Tab = new Regex("\t");

        private static readonly Regex NewLine = new Regex(@"\r?\n");

        public static IList<string> SplitIntoLines(this string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return Array.Empty<string>();
            }
            return NewLine.Split(str).ToList();
        }

        public static string Crop(this string str, int maxLength)
        {
            if (maxLength >= str.Length)
            {
                return str;
            }
            return str.Substring(0, maxLength);
        }

        public static string TabsToSpaces(this string str, int tabWidth)
        {
            int added = 0;
            return Tab.Replace(str, m =>
            {
                int spaces = tabWidth - (m.Index + added) % tabWidth;
                // Match index is index in the original string, which we're
                // now removing a tab character from, and adding a number of spaces:
                added += spaces - 1;
                return new string(' ', spaces);
            });
        }
    }
}
