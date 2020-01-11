using Ephemeral.Chade.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Ephemeral.Chade.Communication.Tcp
{
    public class TcpChannel : IChannel
    {
        private TcpClient _client;
        private Stream _stream;

        public TcpChannel(TcpClient client)
        {
            this._client = client;
            this._stream = client.GetStream();
        }

        public int Read(byte[] buffer, int offset, int size)
        {
            return this._stream.Read(buffer, offset, size);
        }

        public int Write(byte[] buffer, int offset, int size)
        {
            this._stream.Write(buffer, offset, size);
            return (size - offset);
        }

        public bool IsConnected()
        {
            if(this._client != null)
                return this._client.Connected;
            return false;
        }

        public void Flush()
        {
            this._stream.Flush();
        }

        public void Dispose()
        {
            Logger.GetInstance().Debug($"Disposed TCP channel object for remote host: {this._client.Client.RemoteEndPoint.ToString()}");
            this._stream.Close();
            this._stream = null;
            this._client = null;
        }
    }
}
