using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Ephemeral.WinAPI;
using Ephemeral.AccessTokenAPI.Exceptions;

namespace Ephemeral.AccessTokenAPI.Domain.AccessTokenInfo
{
    public class AccessTokenUser
    {
        public string Username { get; }
        public string Domain { get; }

        public SID_NAME_USE Type { get; }

        private AccessTokenUser(string user, string domain, SID_NAME_USE t)
        {
            this.Username = user;
            this.Domain = domain;
            this.Type = t;
        }

        public string ToOutputString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Domain}\\{Username}");
            return sb.ToString();
        }

        public static AccessTokenUser FromTokenHandle(AccessTokenHandle handle)
        {
            uint tokenInfLength = 0;
            bool success;

            IntPtr hToken = handle.GetHandle();

            success = Advapi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenUser, IntPtr.Zero, tokenInfLength, out tokenInfLength);
            IntPtr tokenInfo = Marshal.AllocHGlobal(Convert.ToInt32(tokenInfLength));
            success = Advapi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenUser, tokenInfo, tokenInfLength, out tokenInfLength);

            if (success)
            {
                TOKEN_USER tokenUser = (TOKEN_USER)Marshal.PtrToStructure(tokenInfo, typeof(TOKEN_USER));
                int length = Convert.ToInt32(Advapi32.GetLengthSid(tokenUser.User.Sid));
                byte[] sid = new byte[length];
                Marshal.Copy(tokenUser.User.Sid, sid, 0, length);

                StringBuilder sbUser = new StringBuilder();
                uint cchName = (uint)sbUser.Capacity;
                StringBuilder sbDomain = new StringBuilder();
                uint cchReferencedDomainName = (uint)sbDomain.Capacity;
                SID_NAME_USE peUse;


                var user = "";
                var domain = "";
                if (Advapi32.LookupAccountSid(null, sid, sbUser, ref cchName, sbDomain, ref cchReferencedDomainName, out peUse))
                {
                    user = sbUser.ToString();
                    domain = sbDomain.ToString();
                }
                else
                {
                    var err = Kernel32.GetLastError();
                    if (err == Constants.ERROR_INSUFFICIENT_BUFFER)
                    {
                        sbUser.EnsureCapacity((int)cchName);
                        sbDomain.EnsureCapacity((int)cchReferencedDomainName);
                        if (Advapi32.LookupAccountSid(null, sid, sbUser, ref cchName, sbDomain, ref cchReferencedDomainName, out peUse))
                        {
                            user = sbUser.ToString();
                            domain = sbDomain.ToString();
                        }
                        else
                        {
                            Logger.GetInstance().Error($"Failed to retrieve user for access token. LookupAccountSid failed with error: {err}");
                            user = "UNKNOWN";
                            domain = "UNKNOWN";
                        }
                    }
                    else
                    {
                        Logger.GetInstance().Error($"Failed to retrieve user for access token. LookupAccountSid failed with error: {err}");
                        user = "UNKNOWN";
                        domain = "UNKNOWN";
                    }
                }
                Marshal.FreeHGlobal(tokenInfo);

                return new AccessTokenUser(user, domain, peUse);
            }
            else
            {
                Marshal.FreeHGlobal(tokenInfo);
                Logger.GetInstance().Error($"Failed to retreive token information for access token. GetTokenInformation failed with error: {Kernel32.GetLastError()}");
                throw new TokenInformationException();
            }


        }
    }
}
