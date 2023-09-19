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
}
