using static SDL2.SDL;

namespace InternetScanner
{
    internal class SdlException : Exception
    {
        public SdlException(string? message) : base(message)
        {

        }

        public static SdlException Get() => new(SDL_GetError());
    }
}
