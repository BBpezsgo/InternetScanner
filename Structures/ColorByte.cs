using System.Diagnostics;
using System.Globalization;
using static SDL2.SDL;

namespace InternetScanner
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct ColorByte
    {
        public byte R;
        public byte G;
        public byte B;

        public ColorByte(byte v)
        {
            R = v;
            G = v;
            B = v;
        }

        public ColorByte(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public readonly SDL_Color ToSdl => new()
        {
            r = R,
            g = G,
            b = B,
            a = byte.MaxValue,
        };

        public static bool operator ==(ColorByte left, ColorByte right) => left.Equals(right);
        public static bool operator !=(ColorByte left, ColorByte right) => !(left == right);

        public ColorByte Clamp(byte min, byte max)
        {
            R = Math.Clamp(R, min, max);
            G = Math.Clamp(G, min, max);
            B = Math.Clamp(B, min, max);
            return this;
        }

        public static ColorByte Clamp(ColorByte c, byte min, byte max)
            => c.Clamp(min, max);

        /// <exception cref="ArgumentException"/>
        /// <exception cref="FormatException"/>
        public static ColorByte FromHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
            { throw new ArgumentException($"'{nameof(hex)}' cannot be null or whitespace.", nameof(hex)); }

            hex = hex.TrimStart('#');

            if (hex.Length == 6)
            {
                return new ColorByte(
                            byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber),
                            byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber),
                            byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber));
            }

            if (hex.Length == 3)
            {
                return new ColorByte(
                            byte.Parse(hex.Substring(0, 1), NumberStyles.HexNumber),
                            byte.Parse(hex.Substring(1, 1), NumberStyles.HexNumber),
                            byte.Parse(hex.Substring(2, 1), NumberStyles.HexNumber));
            }

            throw new FormatException();
        }

        public static ColorByte Red => new(255, 0, 0);
        public static ColorByte Yellow => new(255, 255, 0);
        public static ColorByte Green => new(0, 255, 0);
        public static ColorByte Cyan => new(0, 255, 255);
        public static ColorByte Blue => new(3, 132, 252);
        public static ColorByte Magenta => new(255, 0, 255);
        public static ColorByte Black => new(0, 0, 0);
        public static ColorByte Gray => new(127, 127, 127);
        public static ColorByte White => new(255, 255, 255);

        public override readonly string ToString() => $"( {R}, {G}, {B} )";
        readonly string GetDebuggerDisplay() => ToString();

        public override readonly bool Equals(object? obj)
            => obj is ColorByte color && Equals(color);

        public readonly bool Equals(ColorByte other) =>
            R == other.R &&
            G == other.G &&
            B == other.B;

        public override readonly int GetHashCode()
            => HashCode.Combine(R, G, B);
    }
}
