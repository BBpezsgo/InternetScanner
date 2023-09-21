﻿using System.Runtime.InteropServices;
using Win32;
using Win32.Utilities;

namespace InternetScanner
{
    class StateInfo
    {

    }

    internal class Win32Window : IDisposable
    {
        static readonly Dictionary<IntPtr, Win32Window> Handlers = new();
        static readonly List<StateInfo> StateInfos = new();

        IntPtr Handle;
        bool IsDestroyed;
        readonly Dictionary<ushort, Control> Controls;

        public Win32Window()
        {
            Controls = new();
        }

        public unsafe void Initialize(string title, int width, int height, uint style = WS.WS_OVERLAPPEDWINDOW | WS.WS_VISIBLE)
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

                StateInfo stateInfo = new();
                StateInfos.Add(stateInfo);

                GCHandle objHandle = GCHandle.Alloc(stateInfo, GCHandleType.WeakTrackResurrection);

                Handle = User32.CreateWindowExW(exStyles,
                    classNamePtr,
                    windowNamePtr,
                    style,
                    0, 0,
                    width, height,
                    IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, (void*)GCHandle.ToIntPtr(objHandle));
                Handlers.Add(Handle, this);
                IsDestroyed = false;
            }

            {
                ushort newControlId = MakeId();
                ComboBox newControl = new(Handle, "Bruh", 10, 10, 50, 20, newControlId);
                Controls.Add(newControlId, newControl);

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
                ushort newControlId = MakeId();
                Button newControl = new(Handle, "Bruh", 60, 10, 50, 20, newControlId);
                Controls.Add(newControlId, newControl);

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
        }

        public void Dispose()
        {
            Handlers.Remove(Handle);
        }

        static IntPtr WinProc(IntPtr hwnd, uint uMsg, UIntPtr wParam, IntPtr lParam)
        {
            if (Handlers.TryGetValue(hwnd, out Win32Window? window) && window.Handle == hwnd)
            {
                return window.HandleEvent(uMsg, wParam, lParam);
            }
            return User32.DefWindowProcW(hwnd, uMsg, wParam, lParam);
        }

        IntPtr HandleEvent(uint uMsg, UIntPtr wParam, IntPtr lParam)
        {
            switch (uMsg)
            {
                case WM.WM_COMMAND:
                    if (lParam != IntPtr.Zero)
                    {
                        ushort controlId = Macros.LOWORD(wParam);
                        if (Controls.TryGetValue(controlId, out Control? control))
                        {
                            control.DispatchEvent(Handle, uMsg, wParam, lParam);
                            return IntPtr.Zero;
                        }
                    }
                    break;
                case WM.WM_CLOSE:
                    if (User32.MessageBox(Handle, "Really quit?", "My application", (uint)MessageBoxButton.MB_OKCANCEL) == MessageBoxResult.IDOK)
                    {
                        if (User32.DestroyWindow(Handle) == 0)
                        { throw WindowsException.Get(); }
                    }
                    return IntPtr.Zero;
                case WM.WM_DESTROY:
                    IsDestroyed = true;
                    User32.PostQuitMessage(0);
                    return IntPtr.Zero;
                default:
                    break;
            }

            return User32.DefWindowProcW(Handle, uMsg, wParam, lParam);
        }

        ushort MakeId()
        {
            ushort result = 1;
            int endlessSafe = ushort.MaxValue - 1;
            while (Controls.ContainsKey(result))
            {
                result++;
                if (--endlessSafe <= 0) throw new Exception($"Failed to generate control id");
            }
            return result;
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

        public unsafe void HandleEventsBlocking()
        {
            Message msg;
            int res;

            while (!IsDestroyed && (res = User32.GetMessageW(&msg, Handle, 0, 0)) != 0)
            {
                if (res == -1)
                { throw WindowsException.Get(); }

                User32.DispatchMessageW(&msg);
            }
        }

        public void Close()
        {
            if (User32.PostMessageW(Handle, WM.WM_CLOSE, UIntPtr.Zero, IntPtr.Zero) == 0)
            { throw WindowsException.Get(); }
        }
    }
}
