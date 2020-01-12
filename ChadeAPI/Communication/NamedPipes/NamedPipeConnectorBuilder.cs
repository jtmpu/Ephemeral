using Ephemeral.Chade.Exceptions;
using Ephemeral.Chade.Logging;
using Ephemeral.Chade.Registry;
using Ephemeral.WinAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Ephemeral.Chade.Communication.NamedPipes
{
    public enum NamedPipeConnectorType
    {
        Bind,
        Reverse
    }

    public class NamedPipeConnectorBuilder
    {
        public string Name { get; private set; }

        public uint OpenMode { get; private set; }
        
        public uint Mode { get; private set; }

        public uint InBufferSize { get; private set; }

        public uint OutBufferSize { get; private set; }

        public bool NullDACL { get; private set; }

        public bool NullLogonSessions { get; private set; }

        public NamedPipeConnectorType Type { get; private set; }

        public string RemoteHost { get; private set; }

        public NamedPipeConnectorBuilder()
        {
            this.Name = "Ephemeral.Chade";
            this.RemoteHost = ".";
            this.OpenMode = (uint)PipeOpenModeFlags.PIPE_ACCESS_DUPLEX;
            this.Mode = (uint)PipeModeFlags.PIPE_TYPE_BYTE | (uint)PipeModeFlags.PIPE_WAIT;
            this.InBufferSize = 1024;
            this.OutBufferSize = 1024;
            this.NullDACL = false;
            this.NullLogonSessions = false;
            this.Type = NamedPipeConnectorType.Bind;
        }

        #region Setters

        /// <summary>
        /// Setting the DACL of the named pipe to null will allow anyone
        /// to access this pipe.
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public NamedPipeConnectorBuilder SetNullDACL(bool enabled)
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
        public NamedPipeConnectorBuilder SetNullLogonSessions(bool enabled)
        {
            this.NullLogonSessions = enabled;
            return this;
        }

        public NamedPipeConnectorBuilder SetName(string name)
        {
            this.Name = name;
            return this;
        }

        public NamedPipeConnectorBuilder SetOpenMode(params PipeOpenModeFlags[] flags)
        {
            this.OpenMode = 0;
            foreach (var flag in flags)
            {
                this.OpenMode |= (uint)flag;
            }
            return this;
        }

        public NamedPipeConnectorBuilder SetMode(params PipeModeFlags[] flags)
        {
            this.Mode = 0;
            foreach (var flag in flags)
            {
                this.Mode |= (uint)flag;
            }
            return this;
        }

        public NamedPipeConnectorBuilder SetInBufferSize(uint size)
        {
            this.InBufferSize = size;
            return this;
        }

        public NamedPipeConnectorBuilder SetOutBufferSize(uint size)
        {
            this.OutBufferSize = size;
            return this;
        }

        public NamedPipeConnectorBuilder SetType(NamedPipeConnectorType type)
        {
            this.Type = type;
            return this;
        }

        public NamedPipeConnectorBuilder UseReverseConnector()
        {
            return this.SetType(NamedPipeConnectorType.Reverse);
        }
        public NamedPipeConnectorBuilder UseBindConnector()
        {
            return this.SetType(NamedPipeConnectorType.Bind);
        }

        public NamedPipeConnectorBuilder SetRemoteHost(string host)
        {
            this.RemoteHost = host;
            return this;
        }

        #endregion


        public IConnector Build()
        {

            if(this.Type == NamedPipeConnectorType.Reverse)
            {
                return new NamedPipeReverseConnector(this.Name, this.RemoteHost, this.InBufferSize, this.OutBufferSize, this.OpenMode, this.Mode);
            }

            var pipeName = $"\\\\.\\pipe\\{this.Name}";

            IntPtr pSecAttr = IntPtr.Zero;
            IntPtr pSecDesc = IntPtr.Zero;

            if (this.NullDACL)
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

            if (pipeHandle.ToInt32() == Constants.INVALID_HANDLE_VALUE)
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

            if (this.NullDACL)
            {
                Marshal.FreeHGlobal(pSecDesc);
                Marshal.FreeHGlobal(pSecAttr);
            }

            if (this.NullLogonSessions)
            {
                Utils.ConfigureNullLogonSession(this.Name);
            }


            return new NamedPipeBindConnector(pipeHandle, this.Name, this.InBufferSize, this.OutBufferSize, this.OpenMode, this.Mode);
        }

        /// <summary>
        /// Sets up special permissions for the pipe, allowing certain users access e.t.c.
        /// Returns two allocated structure which needs to be deallocated.
        /// </summary>
        private void InnerCreateNullDACL(out IntPtr pSecAttr, out IntPtr pSecDesc)
        {
            SECURITY_ATTRIBUTES secAttr = new SECURITY_ATTRIBUTES();
            SECURITY_DESCRIPTOR secDesc = new SECURITY_DESCRIPTOR();

            if (!Advapi32.InitializeSecurityDescriptor(out secDesc, 1))
            {
                var msg = $"Failed to initialize new security descriptor when creating pipe. InitializeSecurityDescriptor failed with error code: {Kernel32.GetLastError()}";
                Logger.GetInstance().Error(msg);
                throw new Win32Exception(msg);
            }

            if (!Advapi32.SetSecurityDescriptorDacl(ref secDesc, true, IntPtr.Zero, false))
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
    }
}
