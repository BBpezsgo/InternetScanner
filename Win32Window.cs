using Win32;

namespace InternetScanner
{
    internal class Win32Window : IDisposable
    {
        IntPtr Handle;

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
        }

        public void Dispose()
        {

        }

        static unsafe IntPtr WinProc(IntPtr window, uint message, UIntPtr wParam, IntPtr lParam)
        {
            switch (message)
            {
                case WM.WM_CLOSE:
                    fixed (char* lbText = "Really quit?")
                    {
                        fixed (char* lpCaption = "My application")
                        {
                            if (User32.MessageBox(window, lbText, lpCaption, (uint)MessageBoxButton.MB_OKCANCEL) == MessageBoxResult.IDOK)
                            {
                                if (User32.DestroyWindow(window) == 0)
                                { throw WindowsException.Get(); }
                            }
                        }
                    }
                    return IntPtr.Zero;
                case WM.WM_DESTROY:
                    User32.PostQuitMessage(0);
                    return IntPtr.Zero;
                default:
                    return User32.DefWindowProcW(window, message, wParam, lParam);
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
        }

        public void Close()
        {
            if (User32.PostMessageW(Handle, WM.WM_CLOSE, UIntPtr.Zero, IntPtr.Zero) == 0)
            { throw WindowsException.Get(); }
        }
    }
}
