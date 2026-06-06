using System;
using System.Runtime.InteropServices;

namespace Win32.Interop
{
    public static class User32
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
    }
}
