namespace InternetScanner.Mapping
{
    internal abstract class Mapper2D
    {
        public readonly int Width;
        public readonly int Height;

        public Mapper2D(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public abstract (int X, int Y) Map(long v);

        public abstract long Unmap(int x, int y);

        public long Unmap((int X, int Y) point) => Unmap(point.X, point.Y);
    }
}
