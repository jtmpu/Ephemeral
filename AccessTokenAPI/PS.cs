using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Ephemeral.AccessTokenAPI.Domain;
using Ephemeral.AccessTokenAPI.Domain.AccessTokenInfo;
using Ephemeral.WinAPI;

namespace Ephemeral.AccessTokenAPI
{
    public class PS
    {

        /// <summary>
        /// Return access token information regarding current process.
        /// </summary>
        /// <returns></returns>
        public static String WhoisProcess()
        {
            var hProc = TMProcessHandle.GetCurrentProcessHandle();
            var hToken = AccessTokenHandle.FromProcessHandle(hProc);
            return new AccessTokenInformation(hToken).ToOutputString();
        }

        /// <summary>
        /// Return access token information regarding current thread.
        /// </summary>
        /// <returns></returns>
        public static String WhoisThread()
        {
            var hThread = TMThreadHandle.GetCurrentThreadHandle();
            var hToken = AccessTokenHandle.FromThreadHandle(hThread);
            return new AccessTokenInformation(hToken).ToOutputString();
        }

        private static string GetAccessTokenInfo(AccessTokenHandle hToken)
        {
            StringBuilder info = new StringBuilder();
            var user = AccessTokenUser.FromTokenHandle(hToken);
            var groups = AccessTokenGroups.FromTokenHandle(hToken);
            var privileges = AccessTokenPrivileges.FromTokenHandle(hToken);
            info.Append("[USERNAME]\n");
            info.Append("\n");
            info.Append($"{user.Domain}\\{user.Username}\n");
            info.Append("\n");
            info.Append("[GROUPS]");
            info.Append("\n");
            foreach (var group in groups.GetGroupEnumerator())
                info.Append($"{group.Domain}\\{group.Name}\n");
            info.Append("\n");
            info.Append("[PRIVILEGES]");
            info.Append("\n");
            info.Append(privileges.ToOutputString());
            info.Append("\n");
            return info.ToString();
        }

        #region Privileges

        public static void EnableProcessPrivilege(string privilege)
        {
            SetProcessPrivilege(privilege, true);
        }

        public static void DisableProcessPrivilege(string privilege)
        {
            SetProcessPrivilege(privilege, false);
        }

        public static void SetProcessPrivilege(string privilege, bool enabled)
        {
            var hProc = TMProcessHandle.GetCurrentProcessHandle();
            var hToken = AccessTokenHandle.FromProcessHandle(hProc, TokenAccess.TOKEN_ADJUST_PRIVILEGES);
            SetPrivilege(hToken, privilege, enabled);
        }

        public static void EnabledThreadPrivilege(string privilege)
        {
            SetThreadPrivilege(privilege, true);
        }
        public static void DisableThreadPrivilege(string privilege)
        {
            SetThreadPrivilege(privilege, false);
        }

        public static void SetThreadPrivilege(string privilege, bool enabled)
        {
            var hThread = TMThreadHandle.GetCurrentThreadHandle();
            var hToken = AccessTokenHandle.FromThreadHandle(hThread);
            SetPrivilege(hToken, privilege, enabled);
        }

        public static void SetPrivilege(AccessTokenHandle handle, string privilege, bool enabled)
        {
            var newPrivs = new List<ATPrivilege>();
            var attributes = (uint)(enabled ? Constants.SE_PRIVILEGE_ENABLED : Constants.SE_PRIVILEGE_DISABLED);
            newPrivs.Add(ATPrivilege.FromValues(privilege, attributes));

            AccessTokenPrivileges.AdjustTokenPrivileges(handle, newPrivs);
        }

        public static void EnableAllThreadPrivileges()
        {
            var hThread = TMThreadHandle.GetCurrentThreadHandle();
            var hToken = AccessTokenHandle.FromThreadHandle(hThread);
            SetAllPrivileges(hToken, true);
        }
        public static void DisableAllThreadPrivileges()
        {
            var hThread = TMThreadHandle.GetCurrentThreadHandle();
            var hToken = AccessTokenHandle.FromThreadHandle(hThread);
            SetAllPrivileges(hToken, false);
        }

        public static void DisableAllProcessPrivileges()
        {
            var hProc = TMProcessHandle.GetCurrentProcessHandle();
            var hToken = AccessTokenHandle.FromProcessHandle(hProc);
            SetAllPrivileges(hToken, false);
        }
        public static void EnableAllProcessPrivileges()
        {
            var hProc = TMProcessHandle.GetCurrentProcessHandle();
            var hToken = AccessTokenHandle.FromProcessHandle(hProc);
            SetAllPrivileges(hToken, true);
        }

        /// <summary>
        /// Retrieves all current thread privileges, and enables the
        /// ones that are possible.
        /// </summary>
        public static void SetAllPrivileges(AccessTokenHandle hToken, bool enabled)
        {
            foreach(var priv in Enum.GetNames(typeof(PrivilegeConstants)))
            {
                var attributes = enabled ? Constants.SE_PRIVILEGE_ENABLED : Constants.SE_PRIVILEGE_DISABLED;
                var newPriv = new List<ATPrivilege>();
                newPriv.Add(ATPrivilege.FromValues(priv, (uint)attributes));
                try
                {
                    AccessTokenPrivileges.AdjustTokenPrivileges(hToken, newPriv);
                }
                catch
                {
                    continue;
                }
            }
        }

        #endregion

        /// <summary>
        /// Duplicates and impersonates the process token of the specified PID.
        /// This replaces the current thread token. Call RevertToSelf() to get back
        /// previous access token.
        /// </summary>
        /// <param name="pid"></param>
        public static void ImpersonateProcessToken(int pid)
        {
            var hProc = TMProcessHandle.FromProcessId(pid, ProcessAccessFlags.QueryInformation);
            var hToken = AccessTokenHandle.FromProcessHandle(hProc, TokenAccess.TOKEN_IMPERSONATE, TokenAccess.TOKEN_DUPLICATE);

            var hDuplicate = hToken.DuplicateImpersonationToken(TokenAccess.TOKEN_ALL_ACCESS);

            if(!Advapi32.SetThreadToken(IntPtr.Zero, hDuplicate.GetHandle()))
            {
                Console.WriteLine($"{Kernel32.GetLastError()}");
            }
        }

        public static void RevertToSelf()
        {
            Advapi32.RevertToSelf();
        }

        public static void ListProcesses()
        {
            var processes = TMProcess.GetAllProcesses();
            foreach(var p in processes)
            {
                try
                {
                    var pHandle = TMProcessHandle.FromProcess(p, ProcessAccessFlags.QueryInformation);
                    var hToken = AccessTokenHandle.FromProcessHandle(pHandle, TokenAccess.TOKEN_QUERY);
                    var userInfo = AccessTokenUser.FromTokenHandle(hToken);
                    Console.WriteLine($"{p.ProcessId}, {p.ProcessName}, {userInfo.Username}");

                } catch(Exception)
                {
                    continue;
                }
            }

        }
    }
}
