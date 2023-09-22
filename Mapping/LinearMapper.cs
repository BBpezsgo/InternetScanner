namespace InternetScanner.Mapping
{
    internal class LinearMapper : Mapper2D
    {
        public LinearMapper(int width, int height) : base(width, height)
        { }

        public override Point Map(long v) => new((int)(v % Width), (int)(v / Width));

        public override long Unmap(int x, int y) => x + (y * Width);
    }
}
