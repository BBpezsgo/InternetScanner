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

        public abstract Point Map(long v);

        public abstract long Unmap(int x, int y);

        public long Unmap(Point point) => Unmap(point.X, point.Y);
    }
}
