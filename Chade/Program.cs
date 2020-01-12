using System;
using Ephemeral.Chade.Communication.NamedPipes;
using Ephemeral.Chade.Communication.Tcp;
using Ephemeral.Chade.Handlers;

namespace Ephemeral.Chade
{
    class Program
    {
        static void Main(string[] args)
        {
            //var connector = new TcpBindConnector();
            /*var connectorBuilder = new TcpConnectorBuilder();
            var connector = connectorBuilder.
                SetIPAddress("127.0.0.1").
                SetPort(1234).
                UseBindConnection().
                Build();*/
            var builder = new NamedPipeConnectorBuilder();
            var connector = builder.
                UseBindConnector().
                SetRemoteHost(".").
                SetName("Ephemeral.Chade").
                SetNullDACL(true).
                Build();
            /*var connector = builder.
                SetNullDACL(true).
                SetName("Ephemeral.Chade").
                Build();*/
            var channel = connector.EstablishOnce();
            connector.Dispose();

            var shell = new SimpleShell(channel);
            shell.Process();

            while (true)
            {
                Console.Read();
                break;
            }

            shell.Dispose();
            channel.Dispose();
        }
    }
}
