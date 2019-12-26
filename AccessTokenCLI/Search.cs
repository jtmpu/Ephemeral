using CommandLine;
using Ephemeral.AccessTokenAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Ephemeral.AccessTokenAPI.Domain;
using Ephemeral.AccessTokenAPI.Domain.AccessTokenInfo;
using Ephemeral.WinAPI;

namespace Ephemeral.AccessTokenCLI
{
    [Verb("search", HelpText = "Retrieve information about access tokens")]
    public class SearchOptions : BaseOptions
    {

        [Option('a', "all", Required = false, HelpText = "List all available access tokens")]
        public bool ListTokens { get; set; }

        [Option('p', "privilege", Required = false, HelpText = "List processes with this privilege enabled")]
        public string Privilege { get; set; }

        [Option('t', "term", Required = false, HelpText = "Search for processes with this name.")]
        public string Term { get; set; }

        [Option("disabled", Default = false, Required = false, HelpText = "List processes with the privilege disabled" )]
        public bool Disabled { get; set;  }

        [Option('u', "user", Default = "", Required = false, HelpText = "The username or parts of it to search for processes for.")]
        public string User { get; set; }

    }
    public class Search
    {
        private SearchOptions options;
        private ConsoleOutput console;

        public Search(SearchOptions options, ConsoleOutput console)
        {
            this.options = options;
            this.console = console;
        }

        public void Execute()
        {
            if (options.ListTokens)
            {
                var processes = TMProcess.GetAllProcesses();
                this.InnerPrintProcesses(processes);
            }
            if(this.options.Privilege != null)
            {
                var processes = TMProcess.GetAllProcesses();

                var found = new List<TMProcess>();
                foreach(var proc in processes)
                {
                    try
                    {
                        var hProc = TMProcessHandle.FromProcess(proc, ProcessAccessFlags.QueryInformation);
                        var hToken = AccessTokenHandle.FromProcessHandle(hProc, TokenAccess.TOKEN_QUERY);
                        var privileges = AccessTokenPrivileges.FromTokenHandle(hToken);
                        foreach(var priv in privileges.GetPrivileges())
                        {
                            if(priv.Name.ToLower().Contains(this.options.Privilege.ToLower()))
                            {
                                if(this.options.Disabled)
                                {
                                    if (priv.IsDisabled())
                                        found.Add(proc);
                                }
                                else
                                {
                                    if (priv.IsEnabled())
                                        found.Add(proc);
                                }
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        console.Error("Failed to retrieve privilege information: " + e.Message);
                    }
                }
                this.InnerPrintProcesses(found);
            }
            if(this.options.Term != null && this.options.Term != "")
            {
                var processes = TMProcess.GetProcessByName(this.options.Term);
                this.InnerPrintProcesses(processes);
            }
            if(this.options.User != null && this.options.User != "")
            {
                var processes = TMProcess.GetAllProcesses();
                var found = new List<TMProcess>();
                foreach(var proc in processes)
                {
                    try
                    {
                        var hProc = TMProcessHandle.FromProcess(proc, ProcessAccessFlags.QueryInformation);
                        var hToken = AccessTokenHandle.FromProcessHandle(hProc, TokenAccess.TOKEN_QUERY);
                        var user = AccessTokenUser.FromTokenHandle(hToken);
                        if (user.Username.ToLower().Contains(this.options.User.ToLower()))
                        {
                            found.Add(proc);
                        }
                    }
                    catch
                    {

                    }
                }
                this.InnerPrintProcesses(found);
            }
        }

        private void InnerPrintProcesses(List<TMProcess> processes)
        {
            List<Tuple<string, string, string, string>> processesInfo = new List<Tuple<string, string, string, string>>();
            foreach (var p in processes)
            {
                var sessionId = "";
                string username = "";
                try
                {
                    var pHandle = TMProcessHandle.FromProcess(p, ProcessAccessFlags.QueryInformation);
                    var tHandle = AccessTokenHandle.FromProcessHandle(pHandle, TokenAccess.TOKEN_QUERY);
                    var userInfo = AccessTokenUser.FromTokenHandle(tHandle);
                    var sessionInfo = AccessTokenSessionId.FromTokenHandle(tHandle);
                    username = userInfo.Domain + "\\" + userInfo.Username;
                    sessionId = sessionInfo.SessionId.ToString();
                }
                catch (Exception)
                {
                }
                processesInfo.Add(new Tuple<string, string, string, string>(p.ProcessId.ToString(), p.ProcessName, username, sessionId));
            }

            StringBuilder output = new StringBuilder();
            int padding = 2;
            int maxName = 0;
            int maxPid = 0;
            int maxUser = 0;
            int maxSession = 0;

            foreach (var p in processesInfo)
            {
                maxPid = Math.Max(maxPid, p.Item1.Length);
                maxName = Math.Max(maxName, p.Item2.Length);
                maxUser = Math.Max(maxUser, p.Item3.Length);
                maxSession = Math.Max(maxSession, p.Item4.Length);
            }

            string name = "PROCESS";
            string pid = "PID";
            string user = "USER";
            string session = "SESSION";

            output.Append(pid + "," + generateSpaces(maxPid + padding - pid.Length));
            output.Append(name + "," + generateSpaces(maxName + padding - name.Length));
            output.Append(user + generateSpaces(maxUser + padding - user.Length));
            output.Append(session + "\n");

            var sorted = processesInfo.OrderBy(x => x.Item1).ToList();
            foreach (var p in sorted)
            {
                string line = "";
                line += p.Item1 + ",";
                line += generateSpaces(maxPid + padding - p.Item1.Length);
                line += p.Item2 + ",";
                line += generateSpaces(maxName + padding - p.Item2.Length);
                line += p.Item3;
                line += generateSpaces(maxUser + padding - p.Item3.Length);
                line += p.Item4;
                output.Append(line + "\n");
            }

            console.Write(output.ToString());
        }

        private string generateSpaces(int number)
        {
            string ret = "";
            for (int i = 0; i < number; i++)
                ret += " ";
            return ret;
        }
    }
}
