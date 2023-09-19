using static SDL2.SDL;

namespace InternetScanner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /*
            IPv4 a = new(255, 255, 255, 255);
            IPv4 b = new(0, 0, 0, 0);
            IPv4 c = new(0, 0, 0, 1);
            IPv4 d = new(127, 0, 0, 0);
            IPv4 e = new(127, 0, 0, 1);

            a = new(a.Int);
            b = new(b.Int);
            c = new(c.Int);
            d = new(d.Int);
            e = new(e.Int);

            a = new(a.Bytes);
            b = new(b.Bytes);
            c = new(c.Bytes);
            d = new(d.Bytes);
            e = new(e.Bytes);

            a = new(a.Address);
            b = new(b.Address);
            c = new(c.Address);
            d = new(d.Address);
            e = new(e.Address);
            */

            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");

            if (SDL_Init(SDL_INIT_EVERYTHING) != 0) throw SdlException.Get();

            Application application = new(args);
            application.Initialize();
            application.Start();
            while (application.Tick()) ;
            application.Dispose();
        }
    }
}