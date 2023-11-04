using System.Diagnostics;
using SDL2;

namespace InternetScanner
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct Rect : IEquatable<Rect>
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public float Top
        {
            readonly get => Y;
            set => Y = value;
        }
        public float Left
        {
            readonly get => X;
            set => X = value;
        }
        public float Bottom
        {
            get => Y + Height;
        }
        public float Right
        {
            get => X + Width;
        }

        public readonly Point<float> Position => new(X, Y);
        public readonly Point<float> Size => new(Width, Height);

        public Rect(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rect(Point<float> position, Point<float> size)
        {
            X = position.X;
            Y = position.Y;
            Width = size.X;
            Height = size.Y;
        }

        public static Rect Zero => new(0f, 0f, 0f, 0f);

        public override readonly string ToString() => $"( {X}, {Y}, {Width}, {Height} )";

        private readonly string GetDebuggerDisplay() => ToString();

        public override readonly bool Equals(object? obj) =>
            obj is Rect @int &&
            Equals(@int);

        public readonly bool Equals(Rect other) =>
            X == other.X &&
            Y == other.Y &&
            Width == other.Width &&
            Height == other.Height;

        public override readonly int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

        public static bool operator ==(Rect left, Rect right) => left.Equals(right);

        public static bool operator !=(Rect left, Rect right) => !(left == right);

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

        public readonly RectInt Round() => new((int)Math.Round(X), (int)Math.Round(Y), (int)Math.Round(Width), (int)Math.Round(Height));

        public readonly SDL.SDL_Rect ToSdl() => Round().ToSdl();
    }
}
