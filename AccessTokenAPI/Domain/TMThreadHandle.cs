using System;
using System.Collections.Generic;
using System.Text;
using Ephemeral.WinAPI;

namespace Ephemeral.AccessTokenAPI.Domain
{
    public class TMThreadHandle
    {
        public IntPtr Handle { get; }


        private TMThreadHandle(IntPtr handle)
        {
            this.Handle = handle;
        }

        public static TMThreadHandle GetCurrentThreadHandle()
        {
            return new TMThreadHandle(Kernel32.GetCurrentThread());
        }
    }
}
