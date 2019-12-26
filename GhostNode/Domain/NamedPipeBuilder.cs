using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Ephemeral.Ghost.Exceptions;
using Ephemeral.Ghost.Logging;
using Ephemeral.WinAPI;

namespace Ephemeral.Ghost.Domain
{
    public class NamedPipeBuilder
    {
        public string Name { get; private set; }

        public uint OpenMode { get; private set; }

        public uint Mode { get; private set; }

        public uint InBufferSize { get; private set; }

        public uint OutBufferSize { get; private set; }

        public bool NullDACL { get; private set; }

        public bool NullLogonSessions { get; private set; }

        public NamedPipeBuilder()
        {
            // setting some sane defaults.
            this.Name = "Ephemeral.Ghost";
            this.OpenMode = (uint)PipeOpenModeFlags.PIPE_ACCESS_DUPLEX;
            this.Mode = (uint)PipeModeFlags.PIPE_TYPE_BYTE | (uint)PipeModeFlags.PIPE_WAIT;
            this.InBufferSize = 1024;
            this.OutBufferSize = 1024;
            this.NullDACL = false;
            this.NullLogonSessions = false;
        }

        #region Setters

        /// <summary>
        /// Setting the DACL of the named pipe to null will allow anyone
        /// to access this pipe.
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public NamedPipeBuilder SetNullDACL(bool enabled)
        {
            this.NullDACL = enabled;
            return this;
        }

        /// <summary>
        /// Configures the computer to explicitly allow anonymous remote connections to 
        /// this named pipe.
        /// This configurations changes registry settings on the computer, something which
        /// could be commonly looked at during best practice reviews or automatic alarms.
        /// Consider performing a NamedPipeBuilder.Cleanup() if this is used.
        /// </summary>
        /// <returns></returns>
        public NamedPipeBuilder SetNullLogonSessions(bool enabled)
        {
            this.NullLogonSessions = enabled;
            return this;
        }

        public NamedPipeBuilder SetName(string name)
        {
            this.Name = name;
            return this;
        }

        public NamedPipeBuilder SetOpenMode(params PipeOpenModeFlags[] flags)
        {
            this.OpenMode = 0;
            foreach(var flag in flags)
            {
                this.OpenMode |= (uint)flag;
            }
            return this;
        }

        public NamedPipeBuilder SetMode(params PipeModeFlags[] flags)
        {
            this.Mode = 0;
            foreach(var flag in flags)
            {
                this.Mode |= (uint)flag;
            }
            return this;
        }

        public NamedPipeBuilder SetInBufferSize(uint size)
        {
            this.InBufferSize = size;
            return this;
        }

        public NamedPipeBuilder SetOutBufferSize(uint size)
        {
            this.OutBufferSize = size;
            return this;
        }

        #endregion

        public NamedPipe Create()
        {
            var pipeName = $"\\\\.\\pipe\\{this.Name}";

            IntPtr pSecAttr = IntPtr.Zero;
            IntPtr pSecDesc = IntPtr.Zero;

            if(this.NullDACL)
            {
                this.InnerCreateNullDACL(out pSecAttr, out pSecDesc);
            }

            IntPtr pipeHandle = Kernel32.CreateNamedPipe(pipeName, 
                this.OpenMode, 
                this.Mode, 
                Constants.PIPE_UNLIMITED_INSTANCES, 
                this.OutBufferSize,
                this.InBufferSize, 
                Constants.NMPWAIT_WAIT_FOREVER, 
                pSecAttr);

            if(pipeHandle.ToInt32() == Constants.INVALID_HANDLE_VALUE)
            {
                var msg = $"Failed to create pipe. CreateNamedPipe failed with error code: {Kernel32.GetLastError()}";
                Logger.GetInstance().Error(msg);

                if (this.NullDACL)
                {
                    Marshal.FreeHGlobal(pSecDesc);
                    Marshal.FreeHGlobal(pSecAttr);
                }

                throw new Win32Exception(msg);
            }

            if(this.NullDACL)
            {
                Marshal.FreeHGlobal(pSecDesc);
                Marshal.FreeHGlobal(pSecAttr);
            }

            if(this.NullLogonSessions)
            {
                NamedPipeBuilder.ConfigureNullLogonSession(this.Name);
            }


            return new NamedPipe(pipeHandle, this.Name, this.InBufferSize, this.OutBufferSize, this.OpenMode, this.Mode);
        }

        /// <summary>
        /// Sets up special permissions for the pipe, allowing certain users access e.t.c.
        /// Returns two allocated structure which needs to be deallocated.
        /// </summary>
        private void InnerCreateNullDACL(out IntPtr pSecAttr, out IntPtr pSecDesc)
        {
            SECURITY_ATTRIBUTES secAttr = new SECURITY_ATTRIBUTES();
            SECURITY_DESCRIPTOR secDesc = new SECURITY_DESCRIPTOR();

            if(!Advapi32.InitializeSecurityDescriptor(out secDesc, 1))
            {
                var msg = $"Failed to initialize new security descriptor when creating pipe. InitializeSecurityDescriptor failed with error code: {Kernel32.GetLastError()}";
                Logger.GetInstance().Error(msg);
                throw new Win32Exception(msg);
            }

            if(!Advapi32.SetSecurityDescriptorDacl(ref secDesc, true, IntPtr.Zero, false))
            {
                var msg = $"Failed to set null DACL for security descriptor when creating pipe. SetSecurityDescriptorDacl failed with error code: {Kernel32.GetLastError()}";
                Logger.GetInstance().Error(msg);
                throw new Win32Exception(msg);
            }

            secAttr.bInheritHandle = 1;
            pSecDesc = Marshal.AllocHGlobal(Marshal.SizeOf(secDesc));
            Marshal.StructureToPtr(secDesc, pSecDesc, false);
            secAttr.lpSecurityDescriptor = pSecDesc;
            secAttr.nLength = Marshal.SizeOf(secAttr);
            pSecAttr = Marshal.AllocHGlobal(Marshal.SizeOf(secAttr));
            Marshal.StructureToPtr(secAttr, pSecAttr, false);
        }

        public static void ConfigureNullLogonSession(string name)
        {
            var pipes = GetNullLogonSessionRegistry();

            if(!pipes.Contains(name))
            {
                pipes.Add(name);
                SetNullLogonSessionRegistry(pipes);
            }
        }

        public static void RemoveNullLogonSession(string name)
        {
            var pipes = GetNullLogonSessionRegistry();
            if (pipes.Contains(name))
            {
                pipes.Remove(name);
                SetNullLogonSessionRegistry(pipes);
            }
        }

        private static List<string> GetNullLogonSessionRegistry()
        {
            UIntPtr hKey;
            var access = (int)RegistryRights.KEY_READ;
            var result = Kernel32.RegOpenKeyEx(Constants.HKEY_LOCAL_MACHINE, @"SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters", 0, access, out hKey);
            if (result != Constants.ERROR_SUCCESS)
            {
                var msg = $"Failed to open registry lanman server's registry key with read write access. RegOpenKeyEx failed with error code: {Kernel32.GetLastError()}";
                Logger.GetInstance().Error(msg);
                throw new Win32Exception(msg);
            }

            var kind = RegistryValueKind.REG_MULTI_SZ;
            int size = 0;
            result = Kernel32.RegQueryValueEx(hKey, "NullSessionPipes", 0, ref kind, IntPtr.Zero, ref size);

            if (result != Constants.ERROR_SUCCESS)
            {
                var msg = $"Failed to open NullSessionPipes key with read write access. RegQueryValueEx failed with error code: {Kernel32.GetLastError()}";
                Kernel32.RegCloseKey(hKey);
                Logger.GetInstance().Error(msg);
                throw new Win32Exception(msg);
            }

            IntPtr ptr = Marshal.AllocHGlobal(size);
            result = Kernel32.RegQueryValueEx(hKey, "NullSessionPipes", 0, ref kind, ptr, ref size);
            var bytes = new byte[size];
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);

            // The multi_sz type is an array of string separated by the null byte.
            var str = Encoding.Unicode.GetString(bytes);
            var splits = str.Split('\0');


            Kernel32.RegCloseKey(hKey);

            return splits.Where(x => x != "").ToList();
        }

        private static void SetNullLogonSessionRegistry(List<string> pipes)
        {
            UIntPtr hKey;
            var access = (int)RegistryRights.KEY_READ | (int)RegistryRights.KEY_WRITE;
            var result = Kernel32.RegOpenKeyEx(Constants.HKEY_LOCAL_MACHINE, @"SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters", 0, access, out hKey);
            if (result != Constants.ERROR_SUCCESS)
            {
                var msg = $"Failed to open registry lanman server's registry key with read write access. RegOpenKeyEx failed with error code: {result}";
                Logger.GetInstance().Error(msg);
                throw new Win32Exception(msg);
            }

            var value = string.Join("\0", pipes) + "\0";

            //Ensure we use UTF-8 encoding, otherwise something goes wrong i think.
            var bytes = Encoding.UTF8.GetBytes(value);

            IntPtr dst = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, dst, bytes.Length);

            var kind = RegistryValueKind.REG_MULTI_SZ;
            result = Kernel32.RegSetValueEx(hKey, "NullSessionPipes", 0, kind, dst, bytes.Length);

            if (result != Constants.ERROR_SUCCESS)
            {
                var msg = $"Failed to set NullSessionPipes key. RegSetValueEx failed with error code: {result}";
                Logger.GetInstance().Error(msg);
                Marshal.FreeHGlobal(dst);
                Kernel32.RegCloseKey(hKey);
                throw new Win32Exception(msg);
            }

            Kernel32.RegCloseKey(hKey);
            Marshal.FreeHGlobal(dst);
        }

    }
}
