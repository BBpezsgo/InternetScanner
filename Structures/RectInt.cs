using System.Diagnostics;
using SDL2;

namespace InternetScanner
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct RectInt : IEquatable<RectInt>
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public int Top
        {
            readonly get => Y;
            set => Y = value;
        }
        public int Left
        {
            readonly get => X;
            set => X = value;
        }
        public int Bottom
        {
            get => Y + Height;
        }
        public int Right
        {
            get => X + Width;
        }

        public readonly Point Position => new(X, Y);
        public readonly Point Size => new(Width, Height);

        public RectInt(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public RectInt(Point position, Point size)
        {
            X = position.X;
            Y = position.Y;
            Width = size.X;
            Height = size.Y;
        }

        public static RectInt Zero => new(0, 0, 0, 0);

        public override readonly string ToString() => $"( {X}, {Y}, {Width}, {Height} )";

        private readonly string GetDebuggerDisplay() => ToString();

        public override readonly bool Equals(object? obj) =>
            obj is RectInt @int &&
            Equals(@int);

        public readonly bool Equals(RectInt other) =>
            X == other.X &&
            Y == other.Y &&
            Width == other.Width &&
            Height == other.Height;

        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

        public static bool operator ==(RectInt left, RectInt right) => left.Equals(right);

        public static bool operator !=(RectInt left, RectInt right) => !(left == right);

        public bool Contains(Point point) =>
            point.X >= X &&
            point.Y >= Y &&
            point.X < Right &&
            point.Y < Bottom;

        public bool Contains(int x, int y) =>
            x >= X &&
            y >= Y &&
            x < Right &&
            y < Bottom;

        internal SDL.SDL_Rect ToSdl() => new()
        {
            x = X,
            y = Y,
            w = Width,
            h = Height,
        };
    }
}
