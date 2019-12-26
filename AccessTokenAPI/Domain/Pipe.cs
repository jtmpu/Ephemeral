using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using Ephemeral.WinAPI;

namespace Ephemeral.AccessTokenAPI.Domain
{
    public class Pipe
    {
        public IntPtr ReadHandle { get; }
        public IntPtr WriteHandle { get; }

        private Pipe(IntPtr readHandle, IntPtr writeHandle)
        {
            
            this.ReadHandle = readHandle;
            this.WriteHandle = writeHandle;
        }

        ~Pipe()
        {
            Kernel32.CloseHandle(this.ReadHandle);
            Kernel32.CloseHandle(this.WriteHandle);
        }

        public void Write(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            uint written = 0;
            if (!Kernel32.WriteFile(this.WriteHandle, bytes, (uint)bytes.Length, ref written, IntPtr.Zero))
            {
                Logger.GetInstance().Error($"Failed to write bytes to pipe. WriteFile failed with error code: {Kernel32.GetLastError()}");
            }
        }

        public string ReadAll()
        {
            var ret = "";
            var buffer = new byte[1024];
            uint lpBytesRead = 0;
            do
            {
                Logger.GetInstance().Debug($"Read data.. {lpBytesRead}");
                if (!Kernel32.ReadFile(this.ReadHandle, buffer, 1024, ref lpBytesRead, IntPtr.Zero))
                {
                    Logger.GetInstance().Error($"Failed to read from input pipe. ReadFile failed with error code: {Kernel32.GetLastError()}");
                }
                ret += Encoding.UTF8.GetString(buffer, 0, (int)lpBytesRead);
                Logger.GetInstance().Debug(ret);
            } while (lpBytesRead > 0);
            return ret;
        }

        public static Pipe Create()
        {
            IntPtr pRead;
            IntPtr pWrite;

            Logger.GetInstance().Debug("Creating a security descriptor for the pipe which allows everyone access.");
            SECURITY_ATTRIBUTES secAttr = new SECURITY_ATTRIBUTES();
            SECURITY_DESCRIPTOR secDesc = new SECURITY_DESCRIPTOR();

            if(!Advapi32.InitializeSecurityDescriptor(out secDesc, 1))
            {
                Logger.GetInstance().Error($"Failed to create a security descriptor for the pipe. InitializeSecurityDescriptor failed with error code: {Kernel32.GetLastError()}");
                throw new Exception();
            }

            // IntPtr.Zero means a null DACL, which allows everyone access to this pipe.
            if(!Advapi32.SetSecurityDescriptorDacl(ref secDesc, true, IntPtr.Zero, false))
            {
                Logger.GetInstance().Error($"Failed to set a null DACL in the security descriptor for the pipe. SetSecurityDescriptorDacl failed with error code: {Kernel32.GetLastError()}");
                throw new Exception();
            }

            secAttr.bInheritHandle = 1;
            IntPtr pSecDesc = Marshal.AllocHGlobal(Marshal.SizeOf(secDesc));
            Marshal.StructureToPtr(secDesc, pSecDesc, false);
            secAttr.lpSecurityDescriptor = pSecDesc;
            secAttr.nLength = Marshal.SizeOf(secAttr);

            Logger.GetInstance().Debug("Successfully created security descriptor for pipe.");

            if(!Kernel32.CreatePipe(out pRead, out pWrite, ref secAttr, 0))
            {
                Logger.GetInstance().Error($"Failed to create pipe. CreatePipe failed with error code: {Kernel32.GetLastError()}");
                throw new Exception();
            }

            return new Pipe(pRead, pWrite);
        }
    }
}
