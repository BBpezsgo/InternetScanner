namespace InternetScanner.Mapping
{
    internal class CustomMapper : Mapper2D
    {
        const byte SqrtUInt8 = 16;
        const ushort SqrtUInt16 = 256;
        const uint SqrtUInt32 = 65536;
        const ulong SqrtUInt64 = 4294967296;

        public CustomMapper(int width, int height) : base(width, height)
        {

        }

        public override (int X, int Y) Map(long v)
        {
            IPv4 ipv4 = new((uint)v);
            var square4 = MapToSquare(ipv4.Segment4, SqrtUInt8);
            var square3 = MapToSquare(ipv4.Segment3, SqrtUInt8);
            var square2 = MapToSquare(ipv4.Segment2, SqrtUInt8);
            var square1 = MapToSquare(ipv4.Segment1, SqrtUInt8);
            return (
                (int)(square1.X * SqrtUInt32) + (square2.X * SqrtUInt16) + (square3.X * SqrtUInt8) + square4.X,
                (int)(square1.Y * SqrtUInt32) + (square2.Y * SqrtUInt16) + (square3.Y * SqrtUInt8) + square4.Y
            );
        }

        public override long Unmap(int x, int y)
        {
            byte v1, v2, v3, v4;

            {
                int rx = x % SqrtUInt8;
                int ry = y % SqrtUInt8;

                v4 = (byte)MapFromSquare(rx, ry, SqrtUInt8);
            }

            {
                int rx = x / SqrtUInt8 % SqrtUInt8;
                int ry = y / SqrtUInt8 % SqrtUInt8;

                v3 = (byte)MapFromSquare(rx, ry, SqrtUInt8);
            }
            {
                int rx = x / SqrtUInt16 % SqrtUInt8;
                int ry = y / SqrtUInt16 % SqrtUInt8;

                v2 = (byte)MapFromSquare(rx, ry, SqrtUInt8);
            }
            {
                int rx = (int)(x / SqrtUInt32 % SqrtUInt8);
                int ry = (int)(y / SqrtUInt32 % SqrtUInt8);

                v1 = (byte)MapFromSquare(rx, ry, SqrtUInt8);
            }

            return new IPv4(v1, v2, v3, v4).Int;
        }

        public static (ushort X, ushort Y) MapToSquare(ushort v, ushort width) => ((ushort)(v % width), (ushort)(v / width));
        public static ushort MapFromSquare(ushort x, ushort y, ushort width) => (ushort)(x + (y * width));

        public static (int X, int Y) MapToSquare(int v, int width) => (v % width, v / width);
        public static int MapFromSquare(int x, int y, int width) => x + (y * width);

        public static (uint X, uint Y) MapToSquare(uint v, uint width) => (v % width, v / width);
        public static uint MapFromSquare(uint x, uint y, uint width) => x + (y * width);

        public static (ulong X, ulong Y) MapToSquare(ulong v, ulong width) => (v % width, v / width);
        public static ulong MapFromSquare(ulong x, ulong y, ulong width) => x + (y * width);
    }
}
