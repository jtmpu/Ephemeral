using Ephemeral.Chade.Communication;
using Ephemeral.Chade.Communication.NamedPipes;
using Ephemeral.Chade.Communication.Tcp;
using System;
using System.Text;
using System.Threading;

namespace ChadeCLI
{
    public class Program
    {
        public static IChannel _CHANNEL;

        static void Main(string[] args)
        {
            var builder = new NamedPipeConnectorBuilder();
            var connector = builder.
                UseReverseConnector().
                SetRemoteHost(".").
                SetName("Ephemeral.Chade").
                Build();
            /*var builder = new TcpConnectorBuilder();
            var connector = builder.
                SetIPAddress("127.0.0.1").
                SetPort(1234).
                SetType(TcpConnectorType.Reverse).
                Build();*/
            _CHANNEL = connector.EstablishOnce();
            connector.Dispose();

            Thread t1 = new Thread(ProcessInput);
            Thread t2 = new Thread(ProcessOutput);
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            _CHANNEL.Dispose();

        }

        public static void ProcessInput()
        {
            while(true)
            {
                try
                {
                    var line = Console.ReadLine() + "\n";
                    var bytes = Encoding.UTF8.GetBytes(line);
                    _CHANNEL.Write(bytes, 0, bytes.Length);
                    _CHANNEL.Flush();
                } 
                catch(Exception e)
                {
                    break;
                }
            }
        }

        public static void ProcessOutput()
        {
            while (true)
            {
                try
                {
                    var bytes = new byte[2048];
                    var readBytes = _CHANNEL.Read(bytes, 0, bytes.Length);
                    var output = Encoding.UTF8.GetString(bytes, 0, readBytes);
                    Console.Write(output);
                }
                catch (Exception e)
                {
                    break;
                }
            }
        }
    }
}
