﻿namespace InternetScanner
{
    public struct Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public struct Point<T> where T : struct
    {
        public T X;
        public T Y;

        public Point(T x, T y)
        {
            X = x;
            Y = y;
        }
    }
}
