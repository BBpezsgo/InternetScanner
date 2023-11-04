using System.Linq.Expressions;

namespace InternetScanner
{
    public struct ExceptionUtils
    {
        public static Exception? GetRootException(Exception? exception)
        {
            if (exception == null) return null;

            Exception? result = exception;
            while (result.InnerException != null)
            {
                result = result.InnerException;
            }
            return result;
        }
    }

    public struct ArrayUtils
    {
        public static void Fill<T>(T[,] array, T value)
        {
            int width = array.GetLength(0);
            int height = array.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    array.SetValue(value, x, y);
                }
            }
        }
    }

    public struct GenericMath
    {
        readonly struct BinaryOperator
        {
            public readonly Type Type;
            public readonly Delegate Delegate;

            public BinaryOperator(Type type, Delegate @delegate)
            {
                Type = type;
                Delegate = @delegate;
            }
        }

        static readonly List<BinaryOperator> Adds = new();
        static readonly List<BinaryOperator> Mults = new();

        static Func<T, T, T> GetAdd<T>()
        {
            Type type = typeof(T);
            for (int i = 0; i < Adds.Count; i++)
            {
                if (Adds[i].Type == type)
                {
                    return (Func<T, T, T>)Adds[i].Delegate;
                }
            }

            ParameterExpression pa = Expression.Parameter(type, "a");
            ParameterExpression pb = Expression.Parameter(type, "b");

            BinaryExpression body = Expression.Add(pa, pb);

            Func<T, T, T> add = Expression.Lambda<Func<T, T, T>>(body, pa, pb).Compile();

            Adds.Add(new BinaryOperator(type, add));

            return add;
        }

        static Func<T, T, T> GetMult<T>()
        {
            Type type = typeof(T);
            for (int i = 0; i < Mults.Count; i++)
            {
                if (Mults[i].Type == type)
                {
                    return (Func<T, T, T>)Mults[i].Delegate;
                }
            }

            ParameterExpression pa = Expression.Parameter(type, "a");
            ParameterExpression pb = Expression.Parameter(type, "b");

            BinaryExpression body = Expression.Multiply(pa, pb);

            Func<T, T, T> add = Expression.Lambda<Func<T, T, T>>(body, pa, pb).Compile();

            Mults.Add(new BinaryOperator(type, add));

            return add;
        }

        public static T Add<T>(T a, T b) where T : struct => GetAdd<T>().Invoke(a, b);
        public static T Mult<T>(T a, T b) where T : struct => GetMult<T>().Invoke(a, b);
    }
}
