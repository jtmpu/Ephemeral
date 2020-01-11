using Ephemeral.Chade.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Ephemeral.Chade.Communication.Tcp
{
    public class TcpReverseConnector : IConnector
    {
        public IPAddress IPAddress { get; }
        public int Port { get; }

        public TcpReverseConnector(IPAddress address, int port = 1234)
        {
            this.IPAddress = address;
            this.Port = port;
        }


        public void Dispose()
        {
        }

        public IChannel EstablishOnce()
        {
            try
            {
                var client = new TcpClient();
                client.Connect(this.IPAddress, this.Port);
                Logger.GetInstance().Info("TCP reverse connection succeeded.");
                return new TcpChannel(client);
            }
            catch(Exception e)
            {
                Logger.GetInstance().Info($"TCP reverse connection failed with error: {e.Message}");
                throw e;
            }
}
    }
}
