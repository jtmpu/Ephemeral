using CommandLine;
using Ephemeral.Chade.Communication;
using Ephemeral.Chade.Communication.NamedPipes;
using Ephemeral.Chade.Communication.Tcp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ChadeCLI
{
    public class CommandLineOptions
    {
        [Option('l', "listen", Default = false, HelpText = "Use listen connection instead of reverse")]
        public bool Listen { get; set; }

        [Option('t', "tcp", Default = false, HelpText = "Use tcp instead of a pipe")]
        public bool Tcp { get; set; }

        [Option('d', "dest", Default = "127.0.0.1", HelpText = "Host to establish connection with")]
        public string Host { get; set; }

        [Option('p', "port", Default = 1234, HelpText = "Port to connect or listen to")]
        public int Port { get; set; }

        [Option('n', "name", Default = "ephemeral.chade", HelpText = "The named pipes filename.")]
        public string Name { get; set; }

        [Option('l', "loop", Default = false, HelpText = "Continously accepts new connects, and never stop. Only relevant when listening")]
        public bool Continous { get; set; }
    }


    public class CommandLine
    {
        private CommandLineOptions _options;
        private IChannel _channel;

        public CommandLine(CommandLineOptions opts)
        {
            _options = opts;
        }

        public void Execute()
        {
            IConnector connector;
            if (_options.Tcp)
            {
                var builder = new TcpConnectorBuilder();
                connector = builder.
                    SetIPAddress(_options.Host).
                    SetPort(_options.Port).
                    SetType(_options.Listen ? TcpConnectorType.Bind : TcpConnectorType.Reverse).
                    Build();
            }
            else
            {
                var builder = new NamedPipeConnectorBuilder();
                connector = builder.
                    SetName(_options.Name).
                    SetRemoteHost(_options.Host).
                    SetNullDACL(true).
                    SetType(_options.Listen ? NamedPipeConnectorType.Bind : NamedPipeConnectorType.Reverse).
                    Build();
            }

            ProcessShell(connector);
        }

        public void ProcessShell(IConnector connector)
        {
            _channel = connector.EstablishOnce();
            connector.Dispose();

            Thread t1 = new Thread(ProcessInput);
            Thread t2 = new Thread(ProcessOutput);

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            _channel.Dispose();
        }

        private void ProcessInput()
        {
            while (true)
            {
                try
                {
                    var line = Console.ReadLine() + "\n";
                    var bytes = Encoding.UTF8.GetBytes(line);
                    _channel.Write(bytes, 0, bytes.Length);
                    _channel.Flush();
                }
                catch (Exception e)
                {
                    Console.WriteLine("[!] ERROR: " + e.Message);
                    break;
                }
            }
        }

        private void ProcessOutput()
        {
            while (true)
            {
                try
                {
                    var bytes = new byte[2048];
                    var readBytes = _channel.Read(bytes, 0, bytes.Length);
                    var output = Encoding.UTF8.GetString(bytes, 0, readBytes);
                    Console.Write(output);
                }
                catch (Exception e)
                {
                    Console.WriteLine("[!] ERROR: " + e.Message);
                    break;
                }
            }
        }
    }
}
