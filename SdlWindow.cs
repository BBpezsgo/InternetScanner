using static SDL2.SDL;

namespace InternetScanner
{
    public delegate void SdlEventListener(SDL_Event e);

    public class SdlWindow : IDisposable
    {
        public readonly int Width;
        public readonly int Height;

        public readonly IntPtr Renderer;
        public readonly IntPtr Window;

        public event SdlEventListener? OnEvent;

        public SdlWindow(string title, ushort width, ushort height)
        {
            Width = width;
            Height = height;

            Window = SDL_CreateWindow(title, SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, Width, Height, 0);

            if (Window == IntPtr.Zero) throw SdlException.Get();

            Renderer = SDL_CreateRenderer(Window, -1, 0);

            if (Renderer == IntPtr.Zero) throw SdlException.Get();
        }

        public void Initialize()
        {
            if (SDL_SetRenderDrawColor(Renderer, 100, 100, 100, 255) != 0)
            { throw SdlException.Get(); }

            if (SDL_RenderClear(Renderer) != 0)
            { throw SdlException.Get(); }
        }

        public void Render()
        {
            SDL_RenderPresent(Renderer);
        }

        public void HandleEvents()
        {
            while (SDL_PollEvent(out SDL_Event @event) != 0)
            { OnEvent?.Invoke(@event); }
        }

        public void Dispose()
        {
            SDL_DestroyRenderer(Renderer);
            SDL_DestroyWindow(Window);
            SDL_Quit();
        }

        public ColorByte RenderColor
        {
            get
            {
                if (SDL_GetRenderDrawColor(Renderer, out byte r, out byte g, out byte b, out _) != 0)
                { throw SdlException.Get(); }
                return new ColorByte(r, g, b);
            }
            set
            {
                if (SDL_SetRenderDrawColor(Renderer, value.R, value.G, value.B, 255) != 0)
                { throw SdlException.Get(); }
            }
        }

        public void DrawPixel(int x, int y)
        {
            if (SDL_RenderDrawPoint(Renderer, x, y) != 0)
            { throw SdlException.Get(); }
        }
    }
}
