using Ephemeral.GhostCLI.UI;
using System;

namespace Ephemeral.GhostCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var cli = new CommandPrompt(args);
            cli.Execute();
        }
    }
}
