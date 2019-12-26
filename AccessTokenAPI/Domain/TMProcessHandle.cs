using System;
using System.Collections.Generic;
using System.Text;
using Ephemeral.AccessTokenAPI.Exceptions;
using Ephemeral.WinAPI;

namespace Ephemeral.AccessTokenAPI.Domain
{
    public class TMProcessHandle
    {

        public IntPtr Handle { get; }
        private bool selfPseudoHandle;
        private ProcessAccessFlags processAccess;

        private TMProcessHandle(IntPtr handle, ProcessAccessFlags processAccess, bool selfHandle = false)
        {
            this.Handle = handle;
            this.processAccess = processAccess;
            this.selfPseudoHandle = selfHandle;
        }

        public ProcessAccessFlags GetProcessAccessFlag()
        {
            return this.processAccess;
        }

        public bool IsOpenedWithAccess(ProcessAccessFlags access)
        {
            return this.processAccess == access;
        }

        ~TMProcessHandle()
        {
            if (!this.selfPseudoHandle)
                Kernel32.CloseHandle(this.Handle);
        }

        public static TMProcessHandle GetCurrentProcessHandle()
        {
            Logger.GetInstance().Debug($"Retrieve handle to the current process.");
            return new TMProcessHandle(Kernel32.GetCurrentProcess(), ProcessAccessFlags.All, true);
        }
        
        public static TMProcessHandle FromProcess(TMProcess process, ProcessAccessFlags desiredAccess = ProcessAccessFlags.All, bool inheritHandle = true)
        {
            return FromProcessId(process.ProcessId, desiredAccess, inheritHandle);
        }

        public static TMProcessHandle FromProcessId(int pid, ProcessAccessFlags desiredAccess = ProcessAccessFlags.All, bool inheritHandle = true)
        {
            IntPtr hProcess = Kernel32.OpenProcess(desiredAccess, inheritHandle, pid);
            if(hProcess == IntPtr.Zero)
            {
                string errMsg = $"Failed to open handle to process '{pid}' with the access flag '{desiredAccess.ToString()}'. OpenProcess failed with error code: {Kernel32.GetLastError()}.";
                Logger.GetInstance().Error(errMsg);
                throw new OpenProcessException(errMsg);
            }

            return new TMProcessHandle(hProcess, desiredAccess, false);
        }
    }
}
