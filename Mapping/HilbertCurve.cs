namespace InternetScanner.Mapping
{
    internal class HilbertCurve : Mapper2D
    {
        public HilbertCurve(int width, int height) : base(width, height)
        { }

        //convert (x,y) to d
        static long Unmap(long n, long x, long y)
        {
            long rx, ry, s, d = 0;
            for (s = n / 2; s > 0; s /= 2)
            {
                rx = Convert.ToInt64(((x & s) > 0));
                ry = Convert.ToInt64((y & s) > 0);
                d += s * s * ((3 * rx) ^ ry);
                RotateQuadrant(s, ref x, ref y, rx, ry);
            }
            return d;
        }

        //convert d to (x,y)
        static void Map(long n, long d, ref long x, ref long y)
        {
            long rx, ry, s, t = d;

            x = y = 0;
            for (s = 1; s < n; s *= 2)
            {
                rx = (1 & (t / 2));
                ry = (1 & (t ^ rx));
                RotateQuadrant(s, ref x, ref y, rx, ry);
                x += s * rx;
                y += s * ry;
                t /= 4;
            }
        }

        // rotate/flip a quadrant appropriately
        static void RotateQuadrant(long n, ref long x, ref long y, long rx, long ry)
        {
            if (ry == 0)
            {
                if (rx == 1)
                {
                    x = n - 1 - x;
                    y = n - 1 - y;
                }

                long t = x;
                x = y;
                y = t;
            }
        }

        public override (int X, int Y) Map(long v)
        {
            long x = 0, y = 0;
            Map(uint.MaxValue, v, ref x, ref y);
            return ((int)x, (int)y);
        }

        public override long Unmap(int x, int y) => Unmap(uint.MaxValue, x, y);
    }
}
