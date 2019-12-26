using System;
using Ephemeral.AccessTokenAPI;
using CommandLine;
using System.Collections.Generic;

namespace Ephemeral.AccessTokenCLI
{
    class Program
    {

        [STAThread]
        static int Main(string[] args)
        {
            var qwe = new StartProcessOptions();
            qwe.NoGUI = true;
            qwe.Interactive = true;
            qwe.Password = "test";
            qwe.SessionId = -1;
            qwe.Username = "test";
            var co = new ConsoleOutput(qwe);
            //(new StartProcess(qwe, co)).Execute();
            return Parser.Default.ParseArguments<StartProcessOptions, SearchOptions, TokenOptions>(args)
                .MapResult(
                (StartProcessOptions opts) => RunStartProcess(opts),
                (SearchOptions opts) => RunInfo(opts),
                (TokenOptions opts) => RunToken(opts),
                errs => 1);
        }

        public static int RunStartProcess(StartProcessOptions opts)
        {
            var co = new ConsoleOutput(opts);
            Logger.SetGlobalOutput(co);
            var startProcess = new StartProcess(opts, co);
            try
            {
                startProcess.Execute();
                return 0;
            }
            catch(Exception e)
            {
                co.Error(e.Message);
                return 1;
            }
        }

        public static int RunInfo(SearchOptions opts)
        {
            var co = new ConsoleOutput(opts);
            Logger.SetGlobalOutput(co);
            var info = new Search(opts, co);
            try
            {
                info.Execute();
                return 0;
            }
            catch
            {
                return 1;
            }
        }

        public static int RunToken(TokenOptions opts)
        {
            var co = new ConsoleOutput(opts);
            Logger.SetGlobalOutput(co);
            var token = new Token(opts, co);
            try
            {
                token.Execute();
                return 0;
            }
            catch
            {
                return 1;
            }
        }
    }
}
