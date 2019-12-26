using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Ephemeral.WinAPI;
using Ephemeral.AccessTokenAPI.Domain;
using Ephemeral.AccessTokenAPI.Domain.AccessTokenInfo;

namespace Ephemeral.AccessTokenAPI.Logic
{
    public enum WinAPICreateProcessFunction
    {
        CreateProcessWithToken,
        CreateProcessAsUser
    }
    public class TMProcessBuilder
    {

        /// <summary>
        /// A flag to determine if we attempt to enable all privileges for the token.
        /// </summary>
        public bool EnableAll { get; private set; }
        public String Application { get; private set; }
        public string CommandLine { get; private set; }
        public AccessTokenHandle TokenHandle { get; private set; }
        public bool SameSession { get; private set; }
        public bool Interactive { get; private set; }
        public WinAPICreateProcessFunction WinAPIFunction { get; private set; }
        public bool NoGUI { get; private set; }
        public int SessionId { get; private set; }
        public Pipe ProcessPipe { get; private set; }

        public TMProcessBuilder()
        {
            this.SessionId = -1;
            this.EnableAll = false;
            this.Application = @"C:\Windows\System32\cmd.exe";
            this.CommandLine = null;
            this.TokenHandle = null;
            this.SameSession = false;
            this.Interactive = false;
            this.WinAPIFunction = WinAPICreateProcessFunction.CreateProcessWithToken;
        }

        #region Builder setters

        public TMProcessBuilder UsingSessionId(uint sessionId)
        {
            this.SessionId = (int)sessionId;
            var token = AccessTokenHandle.FromSessionId(sessionId);
            this.TokenHandle = token;
            return this;
        }

        public TMProcessBuilder UseNoGUI()
        {
            this.NoGUI = true;
            return this;
        }

        public TMProcessBuilder UsingCredentials(string domain, string username, string password)
        {
            var token = AccessTokenHandle.FromLogin(username,
                password,
                domain,
                LogonType.LOGON32_LOGON_INTERACTIVE,
                LogonProvider.LOGON32_PROVIDER_DEFAULT);
            this.TokenHandle = token;
            return this;
        }
        public TMProcessBuilder UsingExistingProcessToken(int processId)
        {
            var hProc = TMProcessHandle.FromProcessId(processId);
            var hToken = AccessTokenHandle.FromProcessHandle(hProc, TokenAccess.TOKEN_DUPLICATE, TokenAccess.TOKEN_QUERY);
            var hDuplicate = hToken.DuplicatePrimaryToken();
            this.TokenHandle = hDuplicate;
            return this;
        }
        public TMProcessBuilder SetApplication(string application)
        {
            this.Application = application;
            return this;
        }

        public TMProcessBuilder SetCommandLine(string commandLine)
        {
            this.CommandLine = commandLine;
            return this;
        }

        /// <summary>
        /// Uses the CreateProcessWithTokenW function. This
        /// creates a new process and primary thread using an access token.
        /// This requires the privileges SE_IMPERSONATE_NAME.
        /// </summary>
        /// <returns></returns>
        public TMProcessBuilder UsingCreateProcessWithToken()
        {
            this.WinAPIFunction = WinAPICreateProcessFunction.CreateProcessWithToken;
            return this;
        }

        /// <summary>
        /// Uses the CreateProcessAsUser function. This
        /// creates a new process and primary thread using an access token.
        /// This requires the privileges SE_ASSIGNPRIMARYTOKEN and SE_INCREASE_QUOTA.
        /// </summary>
        /// <returns></returns>
        public TMProcessBuilder UsingCreateProcessAsUser()
        {
            this.WinAPIFunction = WinAPICreateProcessFunction.CreateProcessAsUser;
            return this;
        }

        public TMProcessBuilder EnableAllPrivileges()
        {
            this.EnableAll = true;
            return this;
        }

        public TMProcessBuilder SetupInteractive()
        {
            this.Interactive = true;
            return this;
        }

        public TMProcessBuilder EnsureSameSesssionId()
        {
            this.SameSession = true;
            return this;
        }

        #endregion

        public TMProcess Create()
        {
            if(this.EnableAll)
                this.InnerEnablePrivileges();

            if(this.SameSession)
                this.InnerSetSameSessionId();

            if (this.NoGUI == false)
                this.InnerSetDesktopACL();

            switch(this.WinAPIFunction)
            {
                case WinAPICreateProcessFunction.CreateProcessAsUser:
                    return InnerCreateProcessAsUser();
                case WinAPICreateProcessFunction.CreateProcessWithToken:
                    return InnerCreateProcessWithToken();
                default:
                    throw new Exception("No WinAPI process creation function chosen.");
            }
        }

        #region Internal logic

        /// <summary>
        /// Create a processes with an access token of another user requires
        /// us to modify the desktop ACL to allow everybody to have access to it.
        /// 
        /// This requires the SeSecurityPrivilege.
        /// This code has been heavily inspired/taken from Invoke-TokenManipulation.
        /// </summary>
        private void InnerSetDesktopACL()
        {
            this.InnerElevateProcess(PrivilegeConstants.SeSecurityPrivilege);

            var winAccess = (uint)ACCESS_MASK.ACCESS_SYSTEM_SECURITY;
            winAccess |= (uint)ACCESS_MASK.WRITE_DAC;
            winAccess |= (uint)ACCESS_MASK.READ_CONTROL;

            IntPtr hWinSta = User32.OpenWindowStation("WinSta0", false, winAccess);

            if(hWinSta == IntPtr.Zero)
            {
                Logger.GetInstance().Error($"Failed to open handle to window station. OpenWindowStation failed with error code: {Kernel32.GetLastError()}");
                throw new Exception();
            }

            Logger.GetInstance().Debug("Configuring the current Window Station ACL to allow everyone all access.");
            this.InnerSetACLAllowEveryone(hWinSta);

            if (!User32.CloseWindowStation(hWinSta))
            {
                Logger.GetInstance().Error($"Failed to release handle to window station. CloseWindowStation failed with error code: {Kernel32.GetLastError()}");
                throw new Exception();
            }

            var desktopAccess = Constants.DESKTOP_GENERIC_ALL | (uint)ACCESS_MASK.WRITE_DAC;

            IntPtr hDesktop = User32.OpenDesktop("default", 0, false, desktopAccess);

            if(hDesktop == IntPtr.Zero)
            {
                Logger.GetInstance().Error($"Failed to open handle to the default desktop. OpenDesktop failed with error code: {Kernel32.GetLastError()}");
                throw new Exception();
            }

            Logger.GetInstance().Debug("Configuring the current desktop ACL to allow everyone all access.");
            this.InnerSetACLAllowEveryone(hDesktop);

            if(!User32.CloseDesktop(hDesktop))
            {
                Logger.GetInstance().Error($"Failed to close handle to the default desktop. CloseDesktop failed with error code: {Kernel32.GetLastError()}");
            }
        }

        private void InnerSetACLAllowEveryone(IntPtr hObject)
        {
            IntPtr pSidOwner, pSidGroup, pDacl, pSacl, pSecurityDescriptor;
            uint ret = Advapi32.GetSecurityInfo(
                hObject, SE_OBJECT_TYPE.SE_WINDOW_OBJECT,
                SECURITY_INFORMATION.DACL_SECURITY_INFORMATION,
                out pSidOwner,
                out pSidGroup,
                out pDacl,
                out pSacl,
                out pSecurityDescriptor);

            if (ret != 0)
            {
                Logger.GetInstance().Error($"Failed to retrieve security info for window station. GetSecurityInfo failed with error code: {Kernel32.GetLastError()}");
                throw new Exception();
            }

            if (pDacl == IntPtr.Zero)
            {
                Logger.GetInstance().Error($"DACL pointer for window station is a null pointer.");
                throw new Exception();
            }

            ACL aclObj = (ACL)Marshal.PtrToStructure(pDacl, typeof(ACL));
            uint realSize = 2000;
            IntPtr pAllUsersSid = Marshal.AllocHGlobal((int)realSize);

            if (!Advapi32.CreateWellKnownSid(WELL_KNOWN_SID_TYPE.WinWorldSid, IntPtr.Zero, pAllUsersSid, ref realSize))
            {
                Logger.GetInstance().Error($"Failed to lookup SID for Everyone. CreateWellKnownSid failed with error code: {Kernel32.GetLastError()}");
                Marshal.FreeHGlobal(pAllUsersSid);
                throw new Exception();
            }

            var trusteeSize = Marshal.SizeOf(typeof(TRUSTEE));
            var pTrustee = Marshal.AllocHGlobal(trusteeSize);
            var trustee = (TRUSTEE)Marshal.PtrToStructure(pTrustee, typeof(TRUSTEE));
            Marshal.FreeHGlobal(pTrustee);

            trustee.pMultipleTrustee = IntPtr.Zero;
            trustee.MultipleTrusteeOperation = 0;
            trustee.TrusteeForm = Constants.TRUSTEE_IS_SID;
            trustee.TrusteeType = Constants.TRUSTEE_IS_WELL_KNOWN_GROUP;
            trustee.ptstrName = pAllUsersSid;

            var explicitAccessSize = Marshal.SizeOf(typeof(EXPLICIT_ACCESS));
            var pExplicitAccess = Marshal.AllocHGlobal(explicitAccessSize);
            var explicitAccess = (EXPLICIT_ACCESS)Marshal.PtrToStructure(pExplicitAccess, typeof(EXPLICIT_ACCESS));
            Marshal.FreeHGlobal(pExplicitAccess);
            explicitAccess.grfAccessPermissions = 0xf03ff; // What even is this??
            explicitAccess.grfAccessMode = Constants.GRANT_ACCESS;
            explicitAccess.grfInheritance = Constants.OBJECT_INHERIT_ACE;
            explicitAccess.Trustee = trustee;

            IntPtr pNewDacl = IntPtr.Zero;
            var result = Advapi32.SetEntriesInAcl(1, ref explicitAccess, pDacl, out pNewDacl);
            Marshal.FreeHGlobal(pAllUsersSid);

            if (result != 0)
            {
                Logger.GetInstance().Error($"Failed to set ACE entry in Window Station DACL. SetEntriesInAcl failed with error code: {Kernel32.GetLastError()}");
                throw new Exception();
            }

            if (pNewDacl == IntPtr.Zero)
            {
                Logger.GetInstance().Error("The new DACL is null. Something went wrong.");
                throw new Exception();
            }

            ret = Advapi32.SetSecurityInfo(
                hObject,
                SE_OBJECT_TYPE.SE_WINDOW_OBJECT,
                SECURITY_INFORMATION.DACL_SECURITY_INFORMATION,
                pSidOwner,
                pSidGroup,
                pNewDacl,
                pSacl);

            if (ret != 0)
            {
                Logger.GetInstance().Error($"Failed to set security info on window station. SetSecurityInfo failed with error code: {Kernel32.GetLastError()}");
            }

            Kernel32.LocalFree(pSecurityDescriptor);
        }

        private TMProcess InnerCreateProcessWithToken()
        {
            this.InnerElevateProcess(PrivilegeConstants.SeImpersonatePrivilege);

            STARTUPINFO si = new STARTUPINFO();
            if (this.Interactive)
                si = this.InnerSetupInteractive();

            PROCESS_INFORMATION pi;
            if (!Advapi32.CreateProcessWithTokenW(this.TokenHandle.GetHandle(), LogonFlags.NetCredentialsOnly,
                this.Application, this.CommandLine, CreationFlags.NewConsole, IntPtr.Zero, @"C:\", ref si, out pi))
            {
                Logger.GetInstance().Error($"Failed to create shell. CreateProcessWithTokenW failed with error code: {Kernel32.GetLastError()}");
                throw new Exception();
            }

            return TMProcess.FromValues("", pi.dwProcessId, this.ProcessPipe);
        }

        private TMProcess InnerCreateProcessAsUser()
        {
            this.InnerElevateProcess(PrivilegeConstants.SeAssignPrimaryTokenPrivilege, PrivilegeConstants.SeIncreaseQuotaPrivilege);

            STARTUPINFO si = new STARTUPINFO();
            PROCESS_INFORMATION pi;
            SECURITY_ATTRIBUTES saProcessAttributes = new SECURITY_ATTRIBUTES();
            SECURITY_ATTRIBUTES saThreadAttributes = new SECURITY_ATTRIBUTES();
            if (!Advapi32.CreateProcessAsUser(this.TokenHandle.GetHandle(), this.Application, this.CommandLine, ref saProcessAttributes,
                ref saThreadAttributes, false, 0, IntPtr.Zero, null, ref si, out pi))
            {
                Logger.GetInstance().Error($"Failed to create shell. CreateProcessAsUser failed with error code: {Kernel32.GetLastError()}");
                throw new Exception();
            }

            return TMProcess.GetProcessById(pi.dwProcessId);
        }

        private void InnerEnablePrivileges()
        {
            foreach (var privName in Enum.GetNames(typeof(PrivilegeConstants)))
            {
                var privs = new List<ATPrivilege>();
                privs.Add(ATPrivilege.CreateEnabled(privName));
                try
                {
                    AccessTokenPrivileges.AdjustTokenPrivileges(this.TokenHandle, privs);
                }
                catch
                {
                }
            }
        }

        private void InnerSetSameSessionId()
        {
            var hCurrent = AccessTokenHandle.GetCurrentProcessTokenHandle();
            var currentSession = AccessTokenSessionId.FromTokenHandle(hCurrent);
            var targetSession = AccessTokenSessionId.FromTokenHandle(this.TokenHandle);
            if (currentSession.SessionId != targetSession.SessionId)
                AccessTokenSessionId.SetTokenSessionId(currentSession, this.TokenHandle);

            var tmp = AccessTokenSessionId.FromTokenHandle(TokenHandle);
            if (tmp.SessionId != currentSession.SessionId)
                Logger.GetInstance().Error($"Failed to set session id for token. {currentSession.SessionId} vs {tmp.SessionId}");
        }

        private void InnerElevateProcess(params PrivilegeConstants[] privs)
        {
            var hToken = AccessTokenHandle.GetCurrentProcessTokenHandle();
            var privileges = AccessTokenPrivileges.FromTokenHandle(hToken);

            foreach (var priv in privs)
            {
                if (!privileges.IsPrivilegeEnabled(priv))
                {
                    //Due to current bug, i can only adjust one privilege at a time.
                    var newPriv = new List<ATPrivilege>();
                    newPriv.Add(ATPrivilege.CreateEnabled(priv));
                    AccessTokenPrivileges.AdjustTokenPrivileges(hToken, newPriv);
                }
            }
        }

        private STARTUPINFO InnerSetupInteractive()
        {
            this.ProcessPipe = Pipe.Create();
            STARTUPINFO si = new STARTUPINFO();
            // Setup pipes in the other order, so as to logically "read" from the processes output
            //si.hStdInput = this.ProcessPipe.ReadHandle;
            //si.hStdOutput = this.ProcessPipe.WriteHandle;
            //si.hStdError = this.ProcessPipe.WriteHandle;
            si.dwFlags = (int)STARTF.STARTF_USESTDHANDLES;
            si.cb = Marshal.SizeOf(si);
            return si;
        }

        #endregion

    }
}
