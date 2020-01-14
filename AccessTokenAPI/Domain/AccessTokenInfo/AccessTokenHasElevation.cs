using Ephemeral.AccessTokenAPI.Domain;
using Ephemeral.AccessTokenAPI.Exceptions;
using Ephemeral.WinAPI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ephemeral.AccessTokenAPI.Domain.AccessTokenInfo
{
    public class AccessTokenHasElevation
    {

        public bool HasElevation { get; }

        public AccessTokenHasElevation(bool hasElevation)
        {
            this.HasElevation = hasElevation;
        }

        public string ToOutputString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"HasElevation: {this.HasElevation}");
            return sb.ToString();
        }

        public static AccessTokenHasElevation FromTokenHandle(AccessTokenHandle handle)
        {
            uint tokenInfLength = 0;
            bool success;

            IntPtr hToken = handle.GetHandle();

            success = Advapi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenElevation, IntPtr.Zero, tokenInfLength, out tokenInfLength);
            IntPtr tokenInfo = Marshal.AllocHGlobal(Convert.ToInt32(tokenInfLength));
            success = Advapi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenElevation, tokenInfo, tokenInfLength, out tokenInfLength);


            if (success)
            {
                TOKEN_ELEVATION elevation = (TOKEN_ELEVATION)Marshal.PtrToStructure(tokenInfo, typeof(TOKEN_ELEVATION));

                bool hasElevation = elevation.IsElevated != 0;
                Marshal.FreeHGlobal(tokenInfo);

                return new AccessTokenHasElevation(hasElevation);
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
