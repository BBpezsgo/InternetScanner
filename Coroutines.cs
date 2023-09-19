using System.Collections;

namespace InternetScanner
{
    internal static class Coroutines
    {
        static readonly List<Coroutine> coroutines = new();

        public static void Tick()
        {
            for (int i = coroutines.Count - 1; i >= 0; i--)
            { if (!coroutines[i].Tick()) coroutines.RemoveAt(i); }
        }

        public static void Start(IEnumerator enumerator) => coroutines.Add(new Coroutine(enumerator));
    }

    internal class Coroutine
    {
        readonly IEnumerator Enumerator;

        bool isRunning;

        public bool IsFinished => !isRunning;

        public Coroutine(IEnumerator enumerator)
        {
            Enumerator = enumerator;
        }

        public bool Tick()
        {
            isRunning = Enumerator.MoveNext();
            return isRunning;
        }
    }
}
