using Ephemeral.AccessTokenAPI.Exceptions;
using Ephemeral.WinAPI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ephemeral.AccessTokenAPI.Domain.AccessTokenInfo
{ 
    public enum ElevationType
    {
        Default,
        Full,
        Limited
    }

    public class AccessTokenElevationType
    {
        public TOKEN_ELEVATION_TYPE Type { get; }

        public AccessTokenElevationType(TOKEN_ELEVATION_TYPE type)
        {
            this.Type = type;
        }

        public string ToOutputString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Elevation: {this.Type.ToString()}");
            return sb.ToString();
        }

        public static AccessTokenElevationType FromTokenHandle(AccessTokenHandle handle)
        {
            uint tokenInfLength = 0;
            bool success;

            IntPtr hToken = handle.GetHandle();

            success = Advapi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenElevationType, IntPtr.Zero, tokenInfLength, out tokenInfLength);
            IntPtr tokenInfo = Marshal.AllocHGlobal(Convert.ToInt32(tokenInfLength));
            success = Advapi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenElevationType, tokenInfo, tokenInfLength, out tokenInfLength);

            if (success)
            {
                TOKEN_ELEVATION_TYPE elevation = (TOKEN_ELEVATION_TYPE)Marshal.ReadInt32(tokenInfo);
                Marshal.FreeHGlobal(tokenInfo);
                return new AccessTokenElevationType(elevation);
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
