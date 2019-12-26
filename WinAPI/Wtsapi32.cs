using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ephemeral.WinAPI
{
    public class Wtsapi32
    {
        [DllImport("wtsapi32.dll", SetLastError = true)]
        public static extern bool WTSQueryUserToken(
            UInt32 sessionId,
            out IntPtr Token);
    }
}
