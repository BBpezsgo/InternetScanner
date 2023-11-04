using System.Diagnostics;
using System.Linq.Expressions;

namespace InternetScanner
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override readonly string ToString() => $"({X}, {Y})";
        readonly string GetDebuggerDisplay() => ToString();

        public static explicit operator SDL2.SDL.SDL_Point(Point v) => new()
        {
            x = v.X,
            y = v.Y,
        };

        #region Point & Point

        public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);

        public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);

        public static Point operator *(Point a, Point b) => new(a.X * b.X, a.Y * b.Y);

        #endregion

        #region Point & Point<float>

        public static Point<float> operator +(Point a, Point<float> b) => new(a.X + b.X, a.Y + b.Y);
        public static Point<float> operator +(Point<float> a, Point b) => new(a.X + b.X, a.Y + b.Y);

        public static Point<float> operator -(Point a, Point<float> b) => new(a.X - b.X, a.Y - b.Y);
        public static Point<float> operator -(Point<float> a, Point b) => new(a.X - b.X, a.Y - b.Y);

        public static Point<float> operator *(Point a, Point<float> b) => new(a.X * b.X, a.Y * b.Y);
        public static Point<float> operator *(Point<float> a, Point b) => new(a.X * b.X, a.Y * b.Y);

        #endregion

        #region Point & int

        public static Point operator *(Point a, int b) => new(a.X * b, a.Y * b);

        public static Point operator /(Point a, int b) => new(a.X / b, a.Y / b);

        #endregion

        #region Point & float

        public static Point<float> operator *(Point a, float b) => new(a.X * b, a.Y * b);

        public static Point<float> operator /(Point a, float b) => new(a.X / b, a.Y / b);

        #endregion
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct Point<T> where T : struct
    {
        public T X;
        public T Y;

        public Point(T x, T y)
        {
            X = x;
            Y = y;
        }

        public override readonly string ToString() => $"({X}, {Y})";
        readonly string GetDebuggerDisplay() => ToString();

        public static Point<T> operator +(Point<T> a, Point<T> b)
            => new(GenericMath.Add(a.X, b.X), GenericMath.Add(a.Y, b.Y));

        public static Point<T> operator *(Point<T> a, Point<T> b)
            => new(GenericMath.Mult(a.X, b.X), GenericMath.Mult(a.Y, b.Y));
    }

    public static class PointExtensions
    {
        public static Point Round(this Point<float> p)
            => new((int)Math.Round(p.X), (int)Math.Round(p.Y));
    }
}
