using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Ephemeral.WinAPI;
using Ephemeral.AccessTokenAPI.Exceptions;

namespace Ephemeral.AccessTokenAPI.Domain.AccessTokenInfo
{
    public class AccessTokenPrivileges
    {
        private List<ATPrivilege> privileges;

        private AccessTokenPrivileges(List<ATPrivilege> privileges)
        {
            this.privileges = privileges;
        }

        public List<ATPrivilege> GetPrivileges()
        {
            return this.privileges;
        }

        public bool IsPrivilegeEnabled(PrivilegeConstants privilege)
        {
            return IsPrivilegeEnabled(privilege.ToString());
        }

        public bool IsPrivilegeEnabled(string privilege)
        {
            foreach(var priv in this.privileges)
            {
                if(priv.Name.ToLower().Equals(privilege.ToLower()))
                {
                    var enabled = priv.Attributes & Constants.SE_PRIVILEGE_ENABLED;
                    return this.IsEnabled(priv.Attributes);
                }
            }
            return false;
        }

        private bool IsEnabled(uint attributes)
        {
            return (attributes & Constants.SE_PRIVILEGE_ENABLED) == Constants.SE_PRIVILEGE_ENABLED;
        }
        private bool IsDisabled(uint attributes)
        {
            return attributes  == Constants.SE_PRIVILEGE_DISABLED;
        }
        private bool IsRemoved(uint attributes)
        {
            return (attributes & Constants.SE_PRIVILEGE_REMOVED) == Constants.SE_PRIVILEGE_REMOVED;
        }

        private bool IsEnabledByDefault(uint attributes)
        {
            return (attributes & Constants.SE_PRIVILEGE_ENABLED_BY_DEFAULT) == Constants.SE_PRIVILEGE_ENABLED_BY_DEFAULT;
        }

        public string ToOutputString()
        {
            StringBuilder sb = new StringBuilder();
            foreach(var priv in privileges)
            {
                var status = "";
                if(IsEnabled(priv.Attributes))
                {
                    status += "Enabled ";
                }
                if (IsDisabled(priv.Attributes))
                {
                    status += "Disabled ";
                }
                if(IsRemoved(priv.Attributes))
                {
                    status += "Removed ";
                }
                if(IsEnabledByDefault(priv.Attributes))
                {
                    status += " (Enabled by default)";
                }

                sb.Append($"{priv.Name}: {status} ({priv.Attributes})\n");
            }
            return sb.ToString();
        }

        public static AccessTokenPrivileges FromTokenHandle(AccessTokenHandle handle)
        {
            uint tokenInfLength = 0;
            bool success;

            IntPtr hToken = handle.GetHandle();

            success = Advapi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenPrivileges, IntPtr.Zero, tokenInfLength, out tokenInfLength);
            IntPtr tokenInfo = Marshal.AllocHGlobal(Convert.ToInt32(tokenInfLength));
            success = Advapi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenPrivileges, tokenInfo, tokenInfLength, out tokenInfLength);

            if (success)
            {
                var parsedGroups = new List<ATGroup>();

                TOKEN_PRIVILEGES privileges = (TOKEN_PRIVILEGES)Marshal.PtrToStructure(tokenInfo, typeof(TOKEN_PRIVILEGES));

                var sidAndAttrSize = Marshal.SizeOf(new LUID_AND_ATTRIBUTES());
                var privs = new List<ATPrivilege>();
                for (int i = 0; i < privileges.PrivilegeCount; i++)
                {
                    var laa = (LUID_AND_ATTRIBUTES)Marshal.PtrToStructure(new IntPtr(tokenInfo.ToInt64() + i * sidAndAttrSize + 4), typeof(LUID_AND_ATTRIBUTES));

                    var pname = new StringBuilder();
                    int luidNameLen = 0;
                    IntPtr ptrLuid = Marshal.AllocHGlobal(Marshal.SizeOf(laa.Luid));
                    Marshal.StructureToPtr(laa.Luid, ptrLuid, true);

                    // Get length of name.
                    Advapi32.LookupPrivilegeName(null, ptrLuid, null, ref luidNameLen);
                    pname.EnsureCapacity(luidNameLen);

                    var privilegeName = "";
                    if(!Advapi32.LookupPrivilegeName(null, ptrLuid, pname, ref luidNameLen))
                    {
                        Logger.GetInstance().Error($"Failed to lookup privilege name. LookupPrivilegeName failed with error: {Kernel32.GetLastError()}");
                        privilegeName = "UNKNOWN";
                    }
                    else
                    {
                        privilegeName = pname.ToString();
                    }
                    Marshal.FreeHGlobal(ptrLuid);

                    privs.Add(ATPrivilege.FromValues(privilegeName, laa.Attributes));
                }


                Marshal.FreeHGlobal(tokenInfo);

                return new AccessTokenPrivileges(privs);
            }
            else
            {
                Marshal.FreeHGlobal(tokenInfo);
                Logger.GetInstance().Error($"Failed to retreive session id information for access token. GetTokenInformation failed with error: {Kernel32.GetLastError()}");
                throw new TokenInformationException();
            }
        }

        public static void AdjustTokenPrivileges(AccessTokenHandle hToken, AccessTokenPrivileges privileges)
        {
            AdjustTokenPrivileges(hToken, privileges.GetPrivileges());
        }

        /// <summary>
        /// Attempts to adjust the specified token's privileges. Only a list of the privileges which
        /// should be changed need to be specified.
        /// Throws an exceptions if the access token privilege adjustment fails.
        /// 
        /// NOTE: I currently have a bug here where i can't specify a list of new privileges to add.
        /// This ONLY works when you only have one privilege in the list.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="newPrivileges"></param>
        public static void AdjustTokenPrivileges(AccessTokenHandle hToken, List<ATPrivilege> newPrivileges)
        {
            if (newPrivileges.Count == 0)
                return;
            if (newPrivileges.Count != 1)
                throw new AdjustTokenPrivilegeException("Can only specify ONE privilege in the newPrivileges list.");

            TOKEN_PRIVILEGES tpNew = new TOKEN_PRIVILEGES();
            tpNew.PrivilegeCount = newPrivileges.Count;
            tpNew.Privileges = new LUID_AND_ATTRIBUTES[newPrivileges.Count];
            for(int i = 0; i < newPrivileges.Count; i++)
            {
                LUID luid;
                if (!Advapi32.LookupPrivilegeValue(null, newPrivileges[i].Name, out luid))
                {
                    var msg = $"Failed to lookup LUID for {newPrivileges[i].Name}. LookupPrivilegeValue failed with error: {Kernel32.GetLastError()}";
                    Logger.GetInstance().Error(msg);
                    throw new AdjustTokenPrivilegeException(msg);
                }
                tpNew.Privileges[i] = new LUID_AND_ATTRIBUTES();
                tpNew.Privileges[i].Luid = luid;
                tpNew.Privileges[i].Attributes = newPrivileges[i].Attributes;
            }

            if (!Advapi32.AdjustTokenPrivileges(hToken.GetHandle(), false, ref tpNew, 0, IntPtr.Zero, IntPtr.Zero))
            {
                var msg = $"Failed to adjust token privileges. AdjustTokenPrivileges failed with error: {Kernel32.GetLastError()}";
                Logger.GetInstance().Error(msg);
                throw new AdjustTokenPrivilegeException(msg);
            }

            var err = Kernel32.GetLastError();
            if(err == Constants.ERROR_NOT_ALL_ASSIGNED)
            {
                Logger.GetInstance().Error("Not all privileges or groups referenced are assigned to the caller.");
            }
        }
    }

    public class ATPrivilege
    {
        public string Name { get; }
        public uint Attributes { get; }

        private ATPrivilege(string name, uint attributes)
        {
            this.Name = name;
            this.Attributes = attributes;
        }

        public bool IsEnabled()
        {
            return this.Attributes == Constants.SE_PRIVILEGE_ENABLED;
        }

        public bool IsRemoved()
        {
            return this.Attributes == Constants.SE_PRIVILEGE_REMOVED;
        }

        public bool IsDisabled()
        {
            return this.Attributes == Constants.SE_PRIVILEGE_DISABLED;
        }

        public static ATPrivilege FromValues(string name, uint attributes)
        {
            return new ATPrivilege(name, attributes);
        }

        public static ATPrivilege FromValues(PrivilegeConstants privilege, PrivilegeAttributes attributes)
        {
            return ATPrivilege.FromValues(privilege.ToString(), attributes);
        }

        public static ATPrivilege CreateDisabled(PrivilegeConstants privilege)
        {
            return ATPrivilege.FromValues(privilege, PrivilegeAttributes.DISABLED);
        }
        public static ATPrivilege CreateDisabled(string privilege)
        {
            return ATPrivilege.FromValues(privilege, PrivilegeAttributes.DISABLED);
        }

        public static ATPrivilege CreateEnabled(string privilege)
        {
            return ATPrivilege.FromValues(privilege, PrivilegeAttributes.ENABLED);
        }

        public static ATPrivilege CreateEnabled(PrivilegeConstants privilege)
        {
            return ATPrivilege.FromValues(privilege, PrivilegeAttributes.ENABLED);
        }

        public static ATPrivilege FromValues(string name, PrivilegeAttributes attributes)
        {
            uint attrib = 0;
            switch(attributes)
            {
                case PrivilegeAttributes.REMOVED:
                    attrib = Constants.SE_PRIVILEGE_REMOVED;
                    break;
                case PrivilegeAttributes.ENABLED:
                    attrib = Constants.SE_PRIVILEGE_ENABLED;
                    break;
                case PrivilegeAttributes.DISABLED:
                    attrib = Constants.SE_PRIVILEGE_DISABLED;
                    break;
                default:
                    throw new Exception("Unkwnon privilege attribute");
            }
            return ATPrivilege.FromValues(name, attrib);
        }
    }

    public enum PrivilegeAttributes
    {
        ENABLED,
        DISABLED,
        REMOVED
    }
}
