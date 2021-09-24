using System;
using System.Runtime.InteropServices;

namespace RevitSocketServer.Util
{
    internal static class RevitPatcher
    {
        /// <summary>
        /// Производит удаление стандартного вызова TaskDialog при ошибках связанных с Third Party updater
        /// </summary>
        public static void ThirdPartyUpdaterDialog()
        {
            // revitui.dll + 0x5442B
            // orig: 
            //      call [@TaskDialog...]
            //
            // need patch by:
            //      _asm{
            //          mov eax, 0x3E9
            //          nop
            //      }
            //      bytes: [B8 E9 03 00 00 90]
            int off = 0x5442B + 0x1000;
            byte[] arr = new byte[] { 0xB8, 0xE9, 0x03, 0x00, 0x00, 0x90 };
            var ptr = WinApi.GetModuleHandleA("revitui.dll");
            if (ptr != IntPtr.Zero && WinApi.VirtualProtect(ptr + off, 8, 0x40, out int _))
            {
                var adr = (ptr + off).ToString("X4");
                Marshal.Copy(arr, 0, ptr + off, arr.Length);
            }
        }
    }
}
