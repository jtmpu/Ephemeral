using Ephemeral.AccessTokenAPI.Domain;
using Ephemeral.AccessTokenAPI.Exceptions;
using Ephemeral.WinAPI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Ephemeral.AccessTokenAPI.Domain.AccessTokenInfo
{
    public class AccessTokenRestrictedSids
    {
        private List<ATGroup> _restrictedSids;

        private AccessTokenRestrictedSids(List<ATGroup> restrictedSids)
        {
            this._restrictedSids = restrictedSids;
        }

        public IEnumerable<ATGroup> GetGroupEnumerator()
        {
            foreach (var group in _restrictedSids)
            {
                yield return group;
            }
        }

        public string ToOutputString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var group in _restrictedSids)
            {
                sb.Append($"Group: {group.Domain}\\{group.Name}\nSID: {group.SIDString}\n\n");
            }
            return sb.ToString();
        }


        public static AccessTokenRestrictedSids FromTokenHandle(AccessTokenHandle handle)
        {
            uint tokenInfLength = 0;
            bool success;

            IntPtr hToken = handle.GetHandle();

            success = Advapi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenRestrictedSids, IntPtr.Zero, tokenInfLength, out tokenInfLength);
            IntPtr tokenInfo = Marshal.AllocHGlobal(Convert.ToInt32(tokenInfLength));
            success = Advapi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenRestrictedSids, tokenInfo, tokenInfLength, out tokenInfLength);

            if (success)
            {
                var parsedGroups = new List<ATGroup>();

                TOKEN_GROUPS groups = (TOKEN_GROUPS)Marshal.PtrToStructure(tokenInfo, typeof(TOKEN_GROUPS));

                var sidAndAttrSize = Marshal.SizeOf(new SID_AND_ATTRIBUTES());

                for (int i = 0; i < groups.GroupCount; i++)
                {
                    var saa = (SID_AND_ATTRIBUTES)Marshal.PtrToStructure(new IntPtr(tokenInfo.ToInt64() + i * sidAndAttrSize + IntPtr.Size), typeof(SID_AND_ATTRIBUTES));
                    var sid = saa.Sid;
                    var attributes = saa.Attributes;

                    IntPtr strPtr;
                    var sidString = "";
                    if (Advapi32.ConvertSidToStringSid(sid, out strPtr))
                    {
                        sidString = Marshal.PtrToStringAuto(strPtr);
                    }
                    else
                    {
                        Logger.GetInstance().Error($"Failed to convert SID to string. ConvertSidToStringSid failed with error: {Kernel32.GetLastError()}");
                        sidString = "UNKNOWN";
                    }

                    int length = Convert.ToInt32(Advapi32.GetLengthSid(sid));
                    byte[] sidBytes = new byte[length];
                    Marshal.Copy(sid, sidBytes, 0, length);
                    StringBuilder lpName = new StringBuilder();
                    uint cchname = (uint)lpName.Capacity;
                    StringBuilder lpdomain = new StringBuilder();
                    uint cchdomain = (uint)lpdomain.Capacity;
                    SID_NAME_USE peUse;

                    var name = "";
                    var domain = "";
                    if (!Advapi32.LookupAccountSid(null, sidBytes, lpName, ref cchname, lpdomain, ref cchdomain, out peUse))
                    {
                        var err = Kernel32.GetLastError();
                        if (err == Constants.ERROR_INSUFFICIENT_BUFFER)
                        {
                            lpName.EnsureCapacity((int)cchname);
                            lpdomain.EnsureCapacity((int)cchdomain);


                            if (!Advapi32.LookupAccountSid(null, sidBytes, lpName, ref cchname, lpdomain, ref cchdomain, out peUse))
                            {
                                Logger.GetInstance().Error($"Failed to lookup name and domain from SID. LookupAccountSid failed with error: {err}");
                                name = "UNKNOWN";
                                domain = "UNKNOWN";
                            }
                            else
                            {
                                name = lpName.ToString();
                                domain = lpdomain.ToString();
                            }
                        }
                    }
                    else
                    {
                        name = lpName.ToString();
                        domain = lpdomain.ToString();
                    }

                    parsedGroups.Add(new ATGroup(sidString, sid, attributes, name, domain, peUse));
                }

                Marshal.FreeHGlobal(tokenInfo);
                return new AccessTokenRestrictedSids(parsedGroups);
            }
            else
            {
                Marshal.FreeHGlobal(tokenInfo);
                Logger.GetInstance().Error($"Failed to retreive restricted sids for access token. GetTokenInformation failed with error: {Kernel32.GetLastError()}");
                throw new TokenInformationException();
            }
        }
    }
}
