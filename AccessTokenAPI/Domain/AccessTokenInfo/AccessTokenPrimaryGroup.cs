using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Ephemeral.WinAPI;
using Ephemeral.AccessTokenAPI.Exceptions;

namespace Ephemeral.AccessTokenAPI.Domain.AccessTokenInfo
{
    public class AccessTokenPrimaryGroup
    {

        public string Name { get; }
        public string Domain { get; }
        public IntPtr SidPtr { get; }
        public SID_NAME_USE Type { get; }

        private AccessTokenPrimaryGroup(string name, string domain, IntPtr sidPtr, SID_NAME_USE peUse)
        {
            this.Name = name;
            this.Domain = domain;
            this.SidPtr = sidPtr;
            this.Type = peUse;
        }

        public string ToOutputString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Domain}\\{Name}");
            return sb.ToString();

        }

        public static AccessTokenPrimaryGroup FromTokenHandle(AccessTokenHandle handle)
        {
            uint tokenInfLength = 0;
            bool success;

            IntPtr hToken = handle.GetHandle();

            success = Advapi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenPrimaryGroup, IntPtr.Zero, tokenInfLength, out tokenInfLength);
            IntPtr tokenInfo = Marshal.AllocHGlobal(Convert.ToInt32(tokenInfLength));
            success = Advapi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenPrimaryGroup, tokenInfo, tokenInfLength, out tokenInfLength);

            if (success)
            {
                // Same struct as the token owner, so lets just reuse it.
                TOKEN_OWNER tokenOwner = (TOKEN_OWNER)Marshal.PtrToStructure(tokenInfo, typeof(TOKEN_OWNER));
                IntPtr sidPtr = tokenOwner.Owner;
                int sidLength = Convert.ToInt32(Advapi32.GetLengthSid(tokenOwner.Owner));
                byte[] sid = new byte[sidLength];
                Marshal.Copy(tokenOwner.Owner, sid, 0, sidLength);
                StringBuilder lpname = new StringBuilder();
                uint cchname = (uint)lpname.Capacity;
                StringBuilder lpdomain = new StringBuilder();
                uint cchdomain = (uint)lpdomain.Capacity;
                SID_NAME_USE peUse;
                var name = "";
                var domain = "";
                if (!Advapi32.LookupAccountSid(null, sid, lpname, ref cchname, lpdomain, ref cchdomain, out peUse))
                {
                    var err = Kernel32.GetLastError();
                    if (err == Constants.ERROR_INSUFFICIENT_BUFFER)
                    {
                        lpname.EnsureCapacity((int)cchname);
                        lpdomain.EnsureCapacity((int)cchdomain);
                        if (!Advapi32.LookupAccountSid(null, sid, lpname, ref cchname, lpdomain, ref cchdomain, out peUse))
                        {
                            Logger.GetInstance().Error($"Failed to lookup owner SID. LookupAccountSid failed with error: {Kernel32.GetLastError()}");
                            throw new TokenInformationException();
                        }
                        else
                        {
                            name = lpname.ToString();
                            domain = lpdomain.ToString();
                        }
                    }
                    else
                    {
                        Logger.GetInstance().Error($"Failed to lookup owner SID. LookupAccountSid failed with error: {err}");
                        throw new TokenInformationException();
                    }
                }
                else
                {
                    name = lpname.ToString();
                    domain = lpdomain.ToString();
                }

                Marshal.FreeHGlobal(tokenInfo);
                return new AccessTokenPrimaryGroup(name, domain, sidPtr, peUse);
            }
            else
            {
                Marshal.FreeHGlobal(tokenInfo);
                Logger.GetInstance().Error($"Failed to retreive session id information for access token. GetTokenInformation failed with error: {Kernel32.GetLastError()}");
                throw new TokenInformationException();
            }
        }
    }
}
