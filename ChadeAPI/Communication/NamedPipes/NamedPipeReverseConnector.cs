using Ephemeral.Chade.Exceptions;
using Ephemeral.Chade.Logging;
using Ephemeral.WinAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.Chade.Communication.NamedPipes
{
    public class NamedPipeReverseConnector : IConnector
    {
        public string RemoteHost { get; }
        public string Name { get; }
        public string PipeName { get; }

        public IntPtr Handle { get; private set; }

        public uint InBufferSize { get; }

        public uint OutBufferSize { get; }

        public uint OpenMode { get; }

        public uint Mode { get; }

        public NamedPipeReverseConnector(string name, string remoteHost, uint inBufferSize, uint outBufferSize, uint openMode, uint mode)
        {
            this.RemoteHost = remoteHost;
            this.Name = name;
            this.PipeName = $"\\\\{remoteHost}\\pipe\\{name}";
            this.InBufferSize = inBufferSize;
            this.OutBufferSize = outBufferSize;
            this.OpenMode = openMode;
            this.Mode = mode;
        }

        public void Dispose()
        {

        }

        public void Close()
        {
            if (!Kernel32.CloseHandle(this.Handle))
            {
                Logger.GetInstance().Error($"Failed to close handle to pipe '{this.PipeName}'. CloseHandle failed with error: {Kernel32.GetLastError()}.");
            }
        }

        public IChannel EstablishOnce()
        {
            var pipeMode = Constants.GENERIC_READ | Constants.GENERIC_WRITE;
            this.Handle = Kernel32.CreateFile(this.PipeName, pipeMode, 0, IntPtr.Zero, Constants.OPEN_EXISTING, 0, IntPtr.Zero);

            if(this.Handle.ToInt32() == Constants.INVALID_HANDLE_VALUE)
            {
                var err = $"Failed to open named pipe on '{this.PipeName}'. CreateFile failed with error: {Kernel32.GetLastError()}";
                Logger.GetInstance().Error(err);
                throw new Win32Exception(err);
            }

            return new NamedPipeChannel(this.Handle, this.Name, this.InBufferSize, this.OutBufferSize, this.OpenMode, this.Mode, ChannelType.Client);
        }
    }
}
