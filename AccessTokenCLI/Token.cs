using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;
using Ephemeral.AccessTokenAPI;
using Ephemeral.AccessTokenAPI.Domain;
using Ephemeral.AccessTokenAPI.Domain.AccessTokenInfo;

namespace Ephemeral.AccessTokenCLI
{
    [Verb("token", HelpText = "Get information about an access token.")]
    public class TokenOptions : BaseOptions
    {
        [Option('p', "processid", Required = false, HelpText = "The process ID to show token info from")]
        public int? ProcessID { get; set; }

        [Option('c', "current", Required = false, HelpText = "Show information about current process access token.")]
        public bool Current { get; set; }

        [Option('u', "user", Required = false, HelpText = "Show user info")]
        public bool ShowUser { get; set; }

        [Option('g', "groups", Required = false, HelpText = "Show groups")]
        public bool ShowGroups { get; set; }

        [Option("logonsid", Required = false, HelpText = "Show logon SID")]
        public bool ShowLogonSid { get; set; }

        [Option("sessionid", Required = false, HelpText = "Show session ID")]
        public bool ShowSessionID { get; set; }

        [Option("privileges", Required = false, HelpText = "Show privileges")]
        public bool ShowPrivileges { get; set; }

        [Option("owner", Required = false, HelpText = "Show the user who will become owner of any objects created by the process using this access token.")]
        public bool ShowOwner { get; set; }

        [Option("primarygroup", Required = false, HelpText = "Show the primary group of any objects created by the process using this access token.")]
        public bool ShowPrimaryGroup { get; set; }

        [Option('a', "all", Required = false, HelpText = "Show all available information")]
        public bool ShowAll { get; set; }

        [Option("restrictions", Required = false, HelpText = "Show restriction information")]
        public bool ShowRestrictions { get; set; }

        [Option("elevated", Required = false, HelpText = "Show elevation information")]
        public bool ShowElevations { get; set; }
    }

    public class Token
    {
        private ConsoleOutput console;
        private TokenOptions options;

        public Token(TokenOptions options, ConsoleOutput console)
        {
            this.options = options;
            this.console = console;
        }

        public void Execute()
        {
            TMProcessHandle hProcess;
            if(this.options.ProcessID.HasValue)
            {
                hProcess = TMProcessHandle.FromProcessId(this.options.ProcessID.Value, Ephemeral.WinAPI.ProcessAccessFlags.QueryInformation);
            }
            else
            {
                hProcess = TMProcessHandle.GetCurrentProcessHandle();
            }

            var hToken = AccessTokenHandle.FromProcessHandle(hProcess, TokenAccess.TOKEN_QUERY);

            if (this.options.ShowUser || this.options.ShowAll)
            {
                ShowUser(hToken);
            }
            if (this.options.ShowGroups || this.options.ShowAll)
            {
                ShowGroups(hToken);
            }

            if (this.options.ShowPrivileges || this.options.ShowAll)
            {
                ShowPrivileges(hToken);
            }

            if (this.options.ShowLogonSid || this.options.ShowAll)
            {
                ShowLogonSid(hToken);
            }

            if (this.options.ShowOwner || this.options.ShowAll)
            {
                ShowOwner(hToken);
            }

            if (this.options.ShowPrimaryGroup || this.options.ShowAll)
            {
                ShowPrimaryGroup(hToken);
            }

            if (this.options.ShowSessionID || this.options.ShowAll)
            {
                ShowSessionID(hToken);
            }

            if (this.options.ShowElevations || this.options.ShowAll)
            {
                ShowElevations(hToken);
            }

            if (this.options.ShowRestrictions || this.options.ShowAll)
            {
                ShowRestrictions(hToken);
            }

        }
        private void ShowElevations(AccessTokenHandle hToken)
        {
            var elevations = AccessTokenHasElevation.FromTokenHandle(hToken);
            console.WriteLine("[ELEVATIONS]");
            console.WriteLine("");
            console.WriteLine(elevations.ToOutputString());
            console.WriteLine("");
            var elevationType = AccessTokenElevationType.FromTokenHandle(hToken);
            console.WriteLine(elevationType.ToOutputString());
            console.WriteLine("");
        }

        private void ShowRestrictions(AccessTokenHandle hToken)
        {
            var restrictions = AccessTokenHasRestrictions.FromTokenHandle(hToken);
            console.WriteLine("[RESTRICTIONS]");
            console.WriteLine("");
            console.WriteLine(restrictions.ToOutputString());
            console.WriteLine("");

            if(restrictions.HasRestrictions)
            {
                var restrictedSids = AccessTokenRestrictedSids.FromTokenHandle(hToken);
                console.WriteLine("[RESTRICTIONS - Restricted SIDs]");
                console.WriteLine("");
                console.WriteLine(restrictedSids.ToOutputString());
                console.WriteLine("");
            }
        }

        private void ShowUser(AccessTokenHandle hToken)
        {
            var user = AccessTokenUser.FromTokenHandle(hToken);
            console.WriteLine("[USERNAME]");
            console.WriteLine("");
            console.WriteLine(user.ToOutputString());
            console.WriteLine("");
        }

        private void ShowGroups(AccessTokenHandle hToken)
        {
            var groups = AccessTokenGroups.FromTokenHandle(hToken);
            console.WriteLine("[GROUPS]");
            console.WriteLine("");
            console.WriteLine(groups.ToOutputString());
            console.WriteLine("");
        }

        private void ShowLogonSid(AccessTokenHandle hToken)
        {
            var logonSid = AccessTokenLogonSid.FromTokenHandle(hToken);
            console.WriteLine("[LOGON SID]");
            console.WriteLine("");
            console.WriteLine(logonSid.ToOutputString());
            console.WriteLine("");
        }

        private void ShowSessionID(AccessTokenHandle hToken)
        {
            var sessionId = AccessTokenSessionId.FromTokenHandle(hToken);
            console.WriteLine("[SESSION ID]");
            console.WriteLine("");
            console.WriteLine(sessionId.ToOutputString());
            console.WriteLine("");

        }

        private void ShowPrivileges(AccessTokenHandle hToken)
        {
            var privileges = AccessTokenPrivileges.FromTokenHandle(hToken);
            console.WriteLine("[PRIVILEGES]");
            console.WriteLine("");
            console.WriteLine(privileges.ToOutputString());
            console.WriteLine("");
        }
        
        private void ShowOwner(AccessTokenHandle hToken)
        {
            var owner = AccessTokenOwner.FromTokenHandle(hToken);
            console.WriteLine("[OWNER]");
            console.WriteLine("");
            console.WriteLine(owner.ToOutputString());
            console.WriteLine("");
        }
        private void ShowPrimaryGroup(AccessTokenHandle hToken)
        {
            var group = AccessTokenPrimaryGroup.FromTokenHandle(hToken);
            console.WriteLine("[PRIMARY GROUP]");
            console.WriteLine("");
            console.WriteLine(group.ToOutputString());
            console.WriteLine("");
        }
    }
}
