using Ephemeral.AccessTokenAPI.Exceptions;
using Ephemeral.WinAPI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ephemeral.AccessTokenAPI.Domain.AccessTokenInfo
{
    public class AccessTokenHasRestrictions
    {
        public bool HasRestrictions { get; }

        private AccessTokenHasRestrictions(bool hasRestrictions)
        {
            this.HasRestrictions = hasRestrictions;
        }

        public string ToOutputString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"HasRestrictions: {this.HasRestrictions}");
            return sb.ToString();
        }

        public static AccessTokenHasRestrictions FromTokenHandle(AccessTokenHandle handle)
        {
            uint tokenInfLength = 0;
            bool success;

            IntPtr hToken = handle.GetHandle();

            success = Advapi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenHasRestrictions, IntPtr.Zero, tokenInfLength, out tokenInfLength);
            IntPtr tokenInfo = Marshal.AllocHGlobal(Convert.ToInt32(tokenInfLength));
            success = Advapi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenHasRestrictions, tokenInfo, tokenInfLength, out tokenInfLength);

            UInt32 hasRestrictions;
            if (success)
            {
                hasRestrictions = (UInt32)Marshal.ReadInt32(tokenInfo);

                Marshal.FreeHGlobal(tokenInfo);

                return new AccessTokenHasRestrictions(hasRestrictions == 0 ? false : true);
            }
            else
            {
                Marshal.FreeHGlobal(tokenInfo);
                Logger.GetInstance().Error($"Failed to retreive HasRestrictions information for access token. GetTokenInformation failed with error: {Kernel32.GetLastError()}");
                throw new TokenInformationException();
            }
        }
    }
}
