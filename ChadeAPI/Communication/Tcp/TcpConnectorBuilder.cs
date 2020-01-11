using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Ephemeral.Chade.Communication.Tcp
{
    public enum TcpConnectorType
    {
        Bind,
        Reverse
    }
    
    public class TcpConnectorBuilder
    {
        public IPAddress IPAddress { get; private set; }
        public int Port { get; private set; }
        public TcpConnectorType Type { get; private set; }


        public TcpConnectorBuilder()
        {
            // Set some sane defaults.
            this.IPAddress = IPAddress.Any;
            this.Port = 1234;
            this.Type = TcpConnectorType.Bind;
        }

        public TcpConnectorBuilder SetIPAddress(string ip)
        {
            this.IPAddress = IPAddress.Parse(ip);
            return this;
        }

        public TcpConnectorBuilder SetIPAddress(IPAddress address)
        {
            this.IPAddress = address;
            return this;
        }

        public TcpConnectorBuilder UseIPAddressAny()
        {
            this.IPAddress = IPAddress.Any;
            return this;
        }

        public TcpConnectorBuilder SetPort(int port)
        {
            this.Port = port;
            return this;
        }

        public TcpConnectorBuilder SetType(TcpConnectorType type)
        {
            this.Type = type;
            return this;
        }

        public TcpConnectorBuilder UseBindConnection()
        {
            this.Type = TcpConnectorType.Bind;
            return this;
        }

        public TcpConnectorBuilder UseReverseConnection()
        {
            this.Type = TcpConnectorType.Reverse;
            return this;
        }

        public IConnector Build()
        {
            switch(this.Type)
            {
                case TcpConnectorType.Bind:
                    return new TcpBindConnector(this.IPAddress, this.Port);
                case TcpConnectorType.Reverse:
                    return new TcpReverseConnector(this.IPAddress, this.Port);
                default:
                    return new TcpBindConnector(this.IPAddress, this.Port);
            }
        }
    }
}
