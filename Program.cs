using Win32;
using Win32.Utilities;
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
            {
                Form win32Window = new("Bruh", 300, 200);

                {
                    ushort newControlId = win32Window.MakeId();
                    ComboBox newControl = new(win32Window.Handle, "Bruh", 10, 10, 50, 20, newControlId);
                    win32Window.Controls.Add(newControlId, newControl);

                    newControl.AddString("Eh");
                    newControl.SelectedIndex = 0;

                    newControl.OnSelectionChanged += (sender, parent) =>
                    {
                        string selectedItemText = sender.GetString(sender.SelectedIndex);
                        User32.MessageBox(parent, selectedItemText, "Item Selected", (uint)MessageBoxButton.MB_OK);
                        return IntPtr.Zero;
                    };
                }

                {
                    ushort newControlId = win32Window.MakeId();
                    Button newControl = new(win32Window.Handle, "Bruh", 60, 10, 50, 20, newControlId);
                    win32Window.Controls.Add(newControlId, newControl);

                    newControl.OnEvent += (senderHandle, parent, code) =>
                    {
                        ComboBox sender = new(senderHandle);
                        if (code == BN.BN_CLICKED)
                        {
                            User32.MessageBox(parent, "bruh", "Button Clicked", (uint)MessageBoxButton.MB_OK);
                            return IntPtr.Zero;
                        }

                        return null;
                    };
                }

                win32Window.HandleEventsBlocking();
            }

            return;

            if (SDL_Init(SDL_INIT_EVERYTHING) != 0) throw SdlException.Get();

            Application application = new(args);
            application.Initialize();
            application.Start();
            while (application.Tick()) ;
            application.Dispose();
        }
    }
}