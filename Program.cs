using Win32;
using Win32.Utilities;
using static Constants;
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

                ComboBox? comboBox1 = null;
                ProgressBar? progressBar1 = null;

                unsafe
                {
                    win32Window.OnResize += (sender, rect) =>
                    {
                        sender.UpdateWindow();
                    };
                }

                {
                    ushort newControlId = win32Window.MakeId();
                    comboBox1 = new(win32Window.Handle, "Bruh", 10, 10, 50, 200, newControlId);
                    win32Window.Controls.Add(newControlId, comboBox1);

                    comboBox1.AddString("Eh");
                    comboBox1.SelectedIndex = 0;

                    comboBox1.OnSelectionChanged += (sender, parent) =>
                    {
                        // string selectedItemText = sender.GetString(sender.SelectedIndex);
                        // User32.MessageBox(parent, selectedItemText, "Item Selected", (uint)MessageBoxButton.MB_OK);
                        return IntPtr.Zero;
                    };
                }

                {
                    ushort newControlId = win32Window.MakeId();
                    progressBar1 = new(win32Window.Handle, "Bruh", 10, 40, 70, 20, newControlId);
                    win32Window.Controls.Add(newControlId, progressBar1);
                    progressBar1.Position = 33;
                }

                {
                    ushort newControlId = win32Window.MakeId();
                    Button? button1 = new(win32Window.Handle, "Bruh", 60, 10, 50, 20, newControlId);
                    win32Window.Controls.Add(newControlId, button1);

                    button1.OnClick += (sender) =>
                    {
                        _ = User32.MoveWindow(win32Window.Handle, 0, 0, 300, 200, TRUE);

                        IntPtr[] children = win32Window.Childs;
                        RECT rect = win32Window.ClientRect;
                        // User32.MessageBox(parent, "bruh", "Button Clicked", (uint)MessageBoxButton.MB_OK);

                        progressBar1.Add(10);
                        progressBar1.State = ProgressBarState.Normal;
                    };
                }

                {
                    ushort newControlId = win32Window.MakeId();
                    IpAddress ipAddress1 = new(win32Window.Handle, 10, 70, 120, 20, newControlId);
                    win32Window.Controls.Add(newControlId, ipAddress1);
                }

                Form.HandleEventsBlocking();
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