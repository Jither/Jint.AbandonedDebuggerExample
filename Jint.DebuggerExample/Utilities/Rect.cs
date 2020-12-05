using Jint.DebuggerExample.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jint.DebuggerExample.Utilities
{
    public enum LengthType
    {
        Absolute,
        Percent
    }

    public struct Length : IEquatable<Length>
    {
        public int Value { get; }
        public LengthType Type { get; }

        public Length(LengthType type, int value)
        {
            if (value > 100 || value < -100)
            {
                throw new ArgumentException($"Percentage length must be between -100 and 100%");
            }
            Type = type;
            Value = value;
        }

        public static Length Percent(int percent)
        {
            return new Length(LengthType.Percent, percent);
        }

        public int ToAbsolute(int totalLength)
        {
            switch (Type)
            {
                case LengthType.Absolute:
                    return Value >= 0 ? Value : totalLength + Value;
                case LengthType.Percent:
                    return Value >= 0 ? (totalLength * Value / 100) : totalLength + (totalLength * Value / 100);
                default:
                    throw new NotImplementedException($"Someone forgot to implement Length type {Type}");
            }
        }

        public override string ToString()
        {
            return Type == LengthType.Absolute ? $"{Value}" : $"{Value}%";
        }

        public override bool Equals(object obj)
        {
            return obj is Length length && Equals(length);
        }

        public bool Equals(Length other)
        {
            return Value == other.Value && Type == other.Type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Type);
        }

        public static bool operator ==(Length left, Length right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Length left, Length right)
        {
            return !(left == right);
        }

        public static implicit operator Length(int value) => new Length(LengthType.Absolute, value);
    }

    public class Bounds
    {
        public Length Left { get; }
        public Length Top { get; }
        public Length Height { get; }
        public Length Width { get; }

        public Bounds(Length left, Length top, Length width, Length height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }

        public Rect ToAbsolute(Display display)
        {
            return new Rect(
                Left.ToAbsolute(display.Columns),
                Top.ToAbsolute(display.Rows),
                Width.ToAbsolute(display.Columns),
                Height.ToAbsolute(display.Rows)
            );
        }
    }

    public class Rect
    {
        public int Left { get; }
        public int Top { get; }
        public int Width { get; }
        public int Height { get; }

        public int Right => Left + Width;
        public int Bottom => Top + Height;

        public Rect(int left, int top, int width, int height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
        }
    }
}
