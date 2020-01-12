using Ephemeral.Chade.Exceptions;
using Ephemeral.Chade.Logging;
using Ephemeral.WinAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Ephemeral.Chade.Communication.NamedPipes
{
    public enum ChannelType
    {
        Client,
        Server
    }

    public class NamedPipeChannel : IChannel
    {

        public ChannelType Type { get; }
        public string Name { get; }

        public IntPtr Handle { get; }

        public uint InBufferSize { get; }

        public uint OutBufferSize { get; }

        public uint OpenMode { get; }

        public uint Mode { get; }

        internal NamedPipeChannel(IntPtr handle, string name, uint inBufSize, uint outBufSize, uint openMode, uint mode, ChannelType type)
        {
            this.Handle = handle;
            this.Name = name;
            this.OutBufferSize = outBufSize;
            this.InBufferSize = inBufSize;
            this.OpenMode = openMode;
            this.Mode = mode;
            this.Type = type;
        }

        /// <summary>
        /// Sends the buffer using the named pipe.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public int Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer", "The buffer cannot be read into null.");
            if (buffer.Length < (offset + count))
                throw new ArgumentException("Buffer is not large enough");
            if (offset < 0)
                throw new ArgumentException("Offset cannot be negative");
            if (count < 0)
                throw new ArgumentException("Count cannot be negative");

            if (offset != 0)
            {
                var buf = new byte[count];
                for (int x = 0; x < count; x++)
                    buf[x] = buffer[offset + x];

                buffer = buf;
            }

            uint written = 0;
            bool result = Kernel32.WriteFile(this.Handle, buffer, (uint)count, ref written, IntPtr.Zero);
            if (!result)
            {
                var msg = $"Failed to write to pipe '{this.Name}'. WriteFile failed with error code: {Kernel32.GetLastError()}";
                Logger.GetInstance().Error(msg);
                throw new Win32Exception(msg);
            }
            if (written < count)
                throw new IOException($"Failed to write entire buffer to pipe '{this.Name}'.");

            return (int)written;
        }

        /// <summary>
        /// Checks if data is available to be read.
        /// </summary>
        /// <returns></returns>
        public bool IsDataAvailable()
        {
            uint bytesRead = 0, available = 0, thismsg = 0;
            bool result = Kernel32.PeekNamedPipe(this.Handle, null, 0, ref bytesRead, ref available, ref thismsg);
            return (result && available > 0);
        }

        public void Flush()
        {
            Kernel32.FlushFileBuffers(this.Handle);
        }


        /// <summary>
        /// Reads data from the named pipe into the buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer", "The buffer cannot be read into null.");
            if (buffer.Length < (offset + count))
                throw new ArgumentException("Buffer is not large enough");
            if (offset < 0)
                throw new ArgumentException("Offset cannot be negative");
            if (count < 0)
                throw new ArgumentException("Count cannot be negative");


            uint read = 0;
            byte[] buf = buffer;
            if (offset != 0)
            {
                buf = new byte[count];
            }

            // Due to a lockup in the named pipe read functionality, we manually wait here until data is available.
            while (!IsDataAvailable())
                Thread.Sleep(10);

            bool f = Kernel32.ReadFile(this.Handle, buf, (uint)count, ref read, IntPtr.Zero);
            if (!f)
            {
                var msg = $"Failed to read data from pipe '{this.Name}'. ReadFile failed with error code: {Kernel32.GetLastError()}";
                Logger.GetInstance().Error(msg);
                throw new Win32Exception(msg);
            }

            if (offset != 0)
            {
                for (int x = 0; x < read; x++)
                {
                    buffer[offset + x] = buf[x];
                }
            }
            return (int)read;
        }

        public bool IsConnected()
        {
            return true;
        }

        public void Dispose()
        {
        }
    }
}
