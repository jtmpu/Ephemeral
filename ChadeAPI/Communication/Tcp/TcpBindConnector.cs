using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Ephemeral.Chade.Logging;
using System.Text;

namespace Ephemeral.Chade.Communication.Tcp
{
    public class TcpBindConnector : IConnector
    {
        public IPAddress IPAddress { get; }
        public int Port { get; }

        private TcpListener _server;

        public TcpBindConnector(IPAddress address = null, int port = 1234)
        {
            this.IPAddress = address == null ? IPAddress.Any : address;
            this.Port = port;
        }

        public IChannel EstablishOnce()
        {
            try
            {
                this._server = new TcpListener(this.IPAddress, this.Port);
                this._server.Start();
                Logger.GetInstance().Info($"Started TCP listener on {this.IPAddress.ToString()}:{this.Port}"); 
                var client = this._server.AcceptTcpClient();
                Logger.GetInstance().Info($"Received connection from {client.Client.RemoteEndPoint.ToString()}");
                var channel = new TcpChannel(client);
                return channel;
            }
            catch(Exception e)
            {
                Logger.GetInstance().Info($"TCP bind listener failed with error: {e.Message}");
                throw e;
            }
        }

        public void Dispose()
        {
            this._server.Stop();
            this._server = null;
            Logger.GetInstance().Debug($"Disposed TCP Bind listener.");
        }
    }
}
