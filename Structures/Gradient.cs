namespace InternetScanner
{
    struct Gradient
    {
        public Color Left;
        public Color Right;

        public Gradient(Color left, Color right)
        {
            Left = left;
            Right = right;
        }

        public readonly Color Sample(float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            float invertedT = 1f - t;
            return (Left * invertedT) + (Right * t);
        }
    }
}
