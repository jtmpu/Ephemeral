﻿using System;
using System.Collections.Generic;
using System.Text;
using Ephemeral.Chade.Exceptions;
using Ephemeral.Chade.Logging;
using Ephemeral.WinAPI;

namespace Ephemeral.Chade.Communication.NamedPipes
{
    public class NamedPipeBindConnector : IConnector
    {
        public string Name { get; }
        public string PipeName { get; }

        public IntPtr Handle { get; }

        public uint InBufferSize { get; }

        public uint OutBufferSize { get; }

        public uint OpenMode { get; }

        public uint Mode { get; }

        public NamedPipeBindConnector(IntPtr pipeHandle, string name, uint inBufferSize, uint outBufferSize, uint openMode, uint mode)
        {
            this.Handle = pipeHandle;
            this.Name = name;
            this.PipeName = $"\\\\.\\pipe\\{this.Name}";
            this.InBufferSize = inBufferSize;
            this.OutBufferSize = outBufferSize;
            this.OpenMode = openMode;
            this.Mode = mode;
        }

        public void Dispose()
        {
            // Specific pipe handles are managed by individual pipe channels
        }

        public void Close()
        {
            if (!Kernel32.CloseHandle(this.Handle))
            {
                Logger.GetInstance().Error($"Failed to close handle to pipe '{this.Name}'. CloseHandle failed with error: {Kernel32.GetLastError()}.");
            }
        }

        public IChannel EstablishOnce()
        {
            Logger.GetInstance().Info($"Listening for connections over pipe '{this.Name}'");
            Kernel32.DisconnectNamedPipe(this.Handle);
            if (!Kernel32.ConnectNamedPipe(this.Handle, IntPtr.Zero))
            {
                var error = Kernel32.GetLastError();
                if (error == Constants.ERROR_PIPE_CONNECTED)
                {
                    Logger.GetInstance().Info($"Received connection on pipe '{this.Name}'");
                    return new NamedPipeChannel(this.Handle, this.Name, this.InBufferSize, this.OutBufferSize, this.OpenMode, this.Mode, ChannelType.Server);
                }
                var err = $"Failed to listen on pipe '{this.Name}'. ConnetNamedPipe failed with error: {error}";
                Logger.GetInstance().Error(err);
                throw new Win32Exception(err);
            }

            Logger.GetInstance().Info($"Received connection on pipe '{this.Name}'");
            return new NamedPipeChannel(this.Handle, this.Name, this.InBufferSize, this.OutBufferSize, this.OpenMode, this.Mode, ChannelType.Server);
        }
    }
}
