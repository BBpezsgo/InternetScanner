using static SDL2.SDL;

namespace InternetScanner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Main invoked");
            /*
            Tests.Start();
            return;
            */
            /*
            unsafe
            {
                int nButtonPressed = 0;
                HResult result;
                fixed (char* text1 = "Text1")
                fixed (char* text2 = "Text2")
                {
                    result = Comctl32.TaskDialog(
                        IntPtr.Zero,
                        IntPtr.Zero,
                        null,
                        text1,
                        text2,
                        TDCBF.OK_BUTTON,
                        TD.WARNING_ICON,
                        &nButtonPressed);
                }

                if (DBCI.IDOK == nButtonPressed)
                {
                    // OK button pressed
                }
                else if (DBCI.IDCANCEL == nButtonPressed)
                {
                    // Cancel pressed
                }
                return;
            }
            */
            /*
            {
                Menu menu = Menu.Create();

                menu.AppendMenu(1, "Exit");
                menu.AppendMenu(2, "What");

                menu.MenuItems[1].SetState(MenuItemState.Grayed);

                using Form form = new("Bruh", 300, 150, menu);

                PopupMenu popupMenu = PopupMenu.CreatePopup();
                popupMenu.AppendMenu(3, "Bruh");
                popupMenu.AppendSeparator();
                popupMenu.AppendMenu(4, "heh");

                form.OnMenuItem += (Form sender, ushort menuItemId) =>
                {
                    switch (menuItemId)
                    {
                        case 1:
                            sender.Close();
                            break;
                        default:
                            break;
                    }
                };

                form.OnContextMenu += (Form sender, Window context, Win32.Point position) =>
                {
                    popupMenu.MenuItems[0].Text = context.Win32ClassName;
                    popupMenu.Show(context.Handle, position.X, position.Y);
                };

                StaticControl staticControl = new(form.Handle, "Type something:", new Win32.Rect(5, 5, 100, 20), 1);
                form.Controls.Add(1, staticControl);

                EditControl editControl = new(form.Handle, "eh", new Win32.Rect(115, 5, 100, 20), 2);
                form.Controls.Add(2, editControl);

                using Bitmap bitmap = Bitmap.LoadFromFile(".\\img.bmp", 48, 48);

                StaticControl image = new(form.Handle, null, new Win32.Rect(5, 30, 48, 48), 3, SS.BITMAP);
                form.Controls.Add(3, image);

                image.SetImage(bitmap, IMAGE.BITMAP);
                image.SendMessage(STM.SETIMAGE, (UIntPtr)0, bitmap);

                Window[] controls = form.Children;

                Form.HandleEventsBlocking();
                return;
            }
            */
            /*
            {
                var windowHandles = Window.GetWindows();
                for (int i = 0; i < windowHandles.Length; i++)
                {
                    Window window = (Window)windowHandles[i];
                    StringBuilder builder = new();
                    builder.Append($"0x{Convert.ToString(window.Handle.ToInt64(), 16).PadLeft(8, '0')} {{ ");

                    builder.Append($"\"{window.Text}\"");

                    builder.Append($" {window.ThreadProcessId}");

                    builder.Append($" \"{window.Win32ClassName}\"");

                    if (window.IsMinimized)
                    { builder.Append(" Minimized"); }

                    if (window.IsMaximized)
                    { builder.Append(" Maximized"); }

                    if (window.IsVisible)
                    { builder.Append(" Visible"); }

                    builder.Append(" }");
                    Console.WriteLine(builder);

                    var root = window.Root;
                    var children = ((Window)root).Children;
                    foreach (var childH in children)
                    {
                        Window child = (Window)childH;
                    }
                }
                return;
            }
            */
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
            /*
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
            */

            if (SDL_Init(SDL_INIT_EVERYTHING) != 0) throw SdlException.Get();

            Application application = new(args);
            application.Initialize();
            application.Start();
            while (application.Tick()) ;
            application.Dispose();
        }
    }
}