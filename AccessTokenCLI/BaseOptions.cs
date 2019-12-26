using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.AccessTokenCLI
{
    public class BaseOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Increase verbosity")]
        public bool Verbose { get; set; }
        [Option('d', "debug", Required = false, HelpText = "Remove all output")]
        public bool Debug { get; set; }
        [Option('q', "quiet", Required = false, HelpText = "Remove all output")]
        public bool Quiet { get; set; }
    }
}
