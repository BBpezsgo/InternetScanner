using System.Diagnostics;
using System.Globalization;
using static SDL2.SDL;

namespace InternetScanner
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct Color : IEquatable<Color>
    {
        public float R;
        public float G;
        public float B;

        public Color(float v)
        {
            R = v;
            G = v;
            B = v;
        }

        public Color(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        public readonly ColorByte To255
        {
            get
            {
                Color clamped = Color.Clamp(this, 0f, 1f);
                return new ColorByte((byte)Math.Round(clamped.R * byte.MaxValue), (byte)Math.Round(clamped.G * byte.MaxValue), (byte)Math.Round(clamped.B * byte.MaxValue));
            }
        }

        public readonly SDL_Color ToSdl
        {
            get
            {
                Color clamped = Color.Clamp(this, 0f, 1f);
                return new SDL_Color()
                {
                    r = (byte)Math.Round(clamped.R * byte.MaxValue),
                    g = (byte)Math.Round(clamped.G * byte.MaxValue),
                    b = (byte)Math.Round(clamped.B * byte.MaxValue),
                    a = byte.MaxValue,
                };
            }
        }

        public static Color operator +(Color a, Color b)
            => new(a.R + b.R, a.G + b.G, a.B + b.B);

        public static Color operator -(Color a, Color b)
            => new(a.R - b.R, a.G - b.G, a.B - b.B);

        public static Color operator *(Color a, Color b)
            => new(a.R * b.R, a.G * b.G, a.B * b.B);

        public static Color operator *(Color a, float b)
            => new(a.R * b, a.G * b, a.B * b);

        public static Color operator *(float a, Color b) => b * a;

        public static Color operator /(Color a, float b)
            => new(a.R / b, a.G / b, a.B / b);

        public static bool operator ==(Color left, Color right) => left.Equals(right);
        public static bool operator !=(Color left, Color right) => !(left == right);

        public Color Clamp(float min, float max)
        {
            R = Math.Clamp(R, min, max);
            G = Math.Clamp(G, min, max);
            B = Math.Clamp(B, min, max);
            return this;
        }

        public static Color Clamp(Color c, float min, float max)
            => c.Clamp(min, max);

        public static Color FromRgb(byte r, byte g, byte b) => new(r / 255f, g / 255f, b / 255f);

        /// <exception cref="ArgumentException"/>
        /// <exception cref="FormatException"/>
        public static Color FromHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
            { throw new ArgumentException($"'{nameof(hex)}' cannot be null or whitespace.", nameof(hex)); }

            hex = hex.TrimStart('#');

            if (hex.Length == 6)
            {
                return Color.FromRgb(
                            byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber),
                            byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber),
                            byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber));
            }

            if (hex.Length == 3)
            {
                return Color.FromRgb(
                            byte.Parse(hex.Substring(0, 1), NumberStyles.HexNumber),
                            byte.Parse(hex.Substring(1, 1), NumberStyles.HexNumber),
                            byte.Parse(hex.Substring(2, 1), NumberStyles.HexNumber));
            }

            throw new FormatException();
        }

        public static Color Red => new(1f, 0f, 0f);
        public static Color Yellow => new(1f, 1f, 0f);
        public static Color Green => new(0f, 1f, 0f);
        public static Color Cyan => new(0f, 1f, 1f);
        public static Color Blue => Color.FromRgb(3, 132, 252);
        public static Color Magenta => new(1f, 0f, 1f);
        public static Color Black => new(0f, 0f, 0f);
        public static Color Gray => new(.5f, .5f, .5f);
        public static Color White => new(1f, 1f, 1f);

        public override readonly string ToString() => $"( {Math.Round(R, 2)}, {Math.Round(G, 2)}, {Math.Round(B, 2)} )";
        readonly string GetDebuggerDisplay() => ToString();

        public override readonly bool Equals(object? obj)
            => obj is Color color && Equals(color);

        public readonly bool Equals(Color other) =>
            R == other.R &&
            G == other.G &&
            B == other.B;

        public override readonly int GetHashCode()
            => HashCode.Combine(R, G, B);
    }

}
