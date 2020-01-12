using System;
using CommandLine;
using Ephemeral.Chade.Communication.NamedPipes;
using Ephemeral.Chade.Communication.Tcp;
using Ephemeral.Chade.Handlers;

namespace Ephemeral.Chade
{
    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<CommandLineOptions>(args)
                .MapResult(
                (CommandLineOptions opts) => Start(opts),
                errs => 0);
        }

        public static int Start(CommandLineOptions opts)
        {
            var cli = new CommandLine(opts);
            cli.Execute();
            return 0;
        }
    }
}
