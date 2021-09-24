using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RevitSocketServer
{
    internal class WinApi
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = false, CallingConvention = CallingConvention.Winapi)]
        public static extern bool AllocConsole();

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = false, CallingConvention = CallingConvention.Winapi)]
        public static extern bool FreeConsole();

        [DllImport("Kernel32.dll", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr GetModuleHandleA(string lpModuleName);

        [DllImport("Kernel32.dll", CharSet = CharSet.Ansi, SetLastError = false, CallingConvention = CallingConvention.Winapi)]
        public static extern bool VirtualProtect(IntPtr lpAddress, int dwSize, int flNewProtect, out int lpflOldProtect);

        private static IntPtr _hWndRevit = IntPtr.Zero;

        private static readonly UInt32 WM_CLOSE = 0x10;

        public static IntPtr GetHandle()
        {
            if (_hWndRevit == IntPtr.Zero)
            {
                var process
                  = System.Diagnostics.Process.
                  GetCurrentProcess();

                if (process != default)
                    _hWndRevit = process.MainWindowHandle;
            }

            return _hWndRevit;
        }

        public static IntPtr CloseCurrentWindow()
            => CloseWindow(GetHandle());

        public static IntPtr CloseWindow(IntPtr hWnd)
            => SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

        private static bool _isConsoleOpen = false;

        public static void ResetStandardOutput()
        {
            var standardOutput = new StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
            Console.OutputEncoding = Encoding.UTF8;
        }

        public static void OpenConsole()
        {
            _isConsoleOpen = AllocConsole();
            if (_isConsoleOpen)
                ResetStandardOutput();
        }


        public static void CloseConsole()
        {
            if (_isConsoleOpen)
            {
                FreeConsole();
            }
        }
    }
}
