using Win32;

namespace InternetScanner
{
    internal class Win32Window : IDisposable
    {
        IntPtr Handle;
        private IntPtr BrugButton;

        public Win32Window()
        {

        }

        public unsafe void Initialize(string title, int width, int height, uint style = WS.OVERLAPPEDWINDOW | WS.VISIBLE)
        {
            fixed (char* classNamePtr = "windowClass")
            {
                WNDCLASSEXW windowClass = new()
                {
                    cbSize = (uint)sizeof(WNDCLASSEXW),
                    hbrBackground = IntPtr.Zero,
                    hCursor = IntPtr.Zero,
                    hIcon = IntPtr.Zero,
                    hIconSm = IntPtr.Zero,
                    hInstance = IntPtr.Zero,
                    lpszClassName = classNamePtr,
                    lpszMenuName = null,
                    style = 0,
                    lpfnWndProc = &WinProc,
                };

                ushort classId = User32.RegisterClassExW(&windowClass);
            }

            fixed (char* windowNamePtr = title)
            fixed (char* classNamePtr = "windowClass")
            {
                uint exStyles = 0;

                Handle = User32.CreateWindowExW(exStyles,
                    classNamePtr,
                    windowNamePtr,
                    style,
                    0, 0,
                    width, height,
                    IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, null);
            }

            this.BrugButton = CreateButton(Handle, "Bruh", 10, 10, 50, 20);
        }

        public void Dispose()
        {

        }

        static unsafe IntPtr WinProc(IntPtr window, uint message, UIntPtr wParam, IntPtr lParam)
        {
            switch (message)
            {
                case WM.WM_COMMAND:
                    var low = BitConverter.ToInt16(BitConverter.GetBytes(wParam.ToUInt32()), 0);
                    var high = BitConverter.ToInt16(BitConverter.GetBytes(wParam.ToUInt32()), 2);
                    if ((high == 0) && (lParam != IntPtr.Zero))
                    {
                        switch (low)
                        {
                            default:
                                break;
                        }
                    }
                    return User32.DefWindowProcW(window, message, wParam, lParam);
                    switch (wParam.ToUInt32())
                    {
                        case 11:
                            User32.MessageBox(IntPtr.Zero, "hello windows", "title", 0);
                            break;
                    }
                    return User32.DefWindowProcW(window, message, wParam, lParam);
                case WM.WM_CLOSE:
                    if (User32.MessageBox(window, "Really quit?", "My application", (uint)MessageBoxButton.MB_OKCANCEL) == MessageBoxResult.IDOK)
                    {
                        if (User32.DestroyWindow(window) == 0)
                        { throw WindowsException.Get(); }
                    }
                    return IntPtr.Zero;
                case WM.WM_DESTROY:
                    User32.PostQuitMessage(0);
                    return IntPtr.Zero;
                default:
                    return User32.DefWindowProcW(window, message, wParam, lParam);
            }
        }

        static unsafe IntPtr CreateButton(IntPtr window, string label, int x, int y, int width, int height)
        {
            fixed (char* windowNamePtr = label)
            fixed (char* classNamePtr = "BUTTON")
            {
                uint exStyles = 0;

                return User32.CreateWindowExW(
                    exStyles,
                    classNamePtr,  // Predefined class; Unicode assumed 
                    windowNamePtr,      // Button text 
                    WS.TABSTOP | WS.VISIBLE | WS.CHILD,  // Styles 
                    x,         // x position 
                    y,         // y position 
                    width,        // Button width
                    height,        // Button height
                    window,     // Parent window
                    IntPtr.Zero,       // No menu.
                    User32.GetWindowLongPtrW(window, -6)
                    );      // Pointer not needed.
            }
        }

        public unsafe void HandleEvents()
        {
            Message msg;
            int res;

            if ((res = User32.PeekMessageW(&msg, Handle, 0, 0, PM.PM_REMOVE)) != 0)
            {
                if (res == -1)
                { throw WindowsException.Get(); }
                else
                { User32.DispatchMessageW(&msg); }
            }

            if ((res = User32.PeekMessageW(&msg, BrugButton, 0, 0, PM.PM_REMOVE)) != 0)
            {
                if (res == -1)
                { throw WindowsException.Get(); }
                else
                { User32.DispatchMessageW(&msg); }
            }
        }

        public void Close()
        {
            if (User32.PostMessageW(Handle, WM.WM_CLOSE, UIntPtr.Zero, IntPtr.Zero) == 0)
            { throw WindowsException.Get(); }
        }
    }
}
