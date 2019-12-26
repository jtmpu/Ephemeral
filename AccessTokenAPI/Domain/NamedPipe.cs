using System;
using Ephemeral.WinAPI;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Ephemeral.AccessTokenAPI.Domain
{
    public enum PipeAccess
    {
        Read,
        Write
    }

    public class NamedPipe
    {
        public IntPtr Handle { get; }

        private NamedPipe(IntPtr pipeHandle)
        {
            this.Handle = pipeHandle;
        }

        public void Write(string msg)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(msg);
            uint written = 0;
            bool result = Kernel32.WriteFile(this.Handle, buffer, (uint)msg.Length, ref written, IntPtr.Zero);

            if (!result)
                Logger.GetInstance().Error($"Failed to write bytes to named pipe. WriteFile failed with error code: {Kernel32.GetLastError()}");
            if (written < msg.Length)
                Logger.GetInstance().Error($"Unable to write entire buffer to named pipe.");
        }

        public static NamedPipe Create(string name, Constants.PipeMode mode)
        {
            string pName = @"\\.\pipe\" + name;
            IntPtr handle = Kernel32.CreateNamedPipe(
                pName,
                (uint)mode,
                Constants.PIPE_TYPE_BYTE | Constants.PIPE_WAIT,
                Constants.PIPE_UNLIMITED_INSTANCES,
                0,
                1024,
                Constants.NMPWAIT_WAIT_FOREVER,
                IntPtr.Zero);

            if(handle.ToInt32() == Constants.INVALID_HANDLE_VALUE)
            {
                var msg = $"Error creating named pipe '{pName}'. CreateNamedPipe failed with error code: {Kernel32.GetLastError()}";
                Logger.GetInstance().Error(msg);
                throw new Exception(msg);
            }


            return new NamedPipe(handle);
        }

        public static NamedPipe Open(string name, PipeAccess access)
        {
            uint mode = 0;
            if (access == PipeAccess.Read)
                mode |= Constants.GENERIC_READ;
            if (access == PipeAccess.Write)
                mode |= Constants.GENERIC_WRITE;

            var pName = @"\\.\pipe\" + name;
            IntPtr handle = Kernel32.CreateFile(pName, mode, 0, IntPtr.Zero, Constants.OPEN_EXISTING, 0, IntPtr.Zero);

            if(handle.ToInt32() == Constants.INVALID_HANDLE_VALUE)
            {
                var msg = $"Error opening named pipe '{pName}. CreateFile failed with error code: {Kernel32.GetLastError()}";
                Logger.GetInstance().Error(msg);
                throw new Exception(msg);
            }

            return new NamedPipe(handle);
        }

    }
}
