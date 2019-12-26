using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Ephemeral.AccessTokenAPI.Domain.AccessTokenInfo;
using Ephemeral.WinAPI;

namespace Ephemeral.AccessTokenAPI.Domain
{
    /// <summary>
    /// A representation of all possible available information
    /// for an access token. If it's not possible to retrieve
    /// the specified information for a token, this just ignores it.
    /// </summary>
    public class AccessTokenInformation
    {

        private AccessTokenGroups _groups;
        private AccessTokenLogonSid _logonSid;
        private AccessTokenOwner _owner;
        private AccessTokenPrimaryGroup _primaryGroup;
        private AccessTokenPrivileges _privileges;
        private AccessTokenSessionId _sessionId;
        private AccessTokenUser _user;

        public AccessTokenInformation(AccessTokenHandle handle)
        {
            try
            {
                this._groups = AccessTokenGroups.FromTokenHandle(handle);
            }
            catch { }
            try
            {
                this._logonSid = AccessTokenLogonSid.FromTokenHandle(handle);
            }
            catch { }
            try
            {
                this._owner = AccessTokenOwner.FromTokenHandle(handle);
            }
            catch { }
            try
            {
                this._primaryGroup = AccessTokenPrimaryGroup.FromTokenHandle(handle);
            }
            catch { }
            try
            {
                this._primaryGroup = AccessTokenPrimaryGroup.FromTokenHandle(handle);
            }
            catch { }
            try
            {
                this._privileges = AccessTokenPrivileges.FromTokenHandle(handle);
            }
            catch { }
            try
            {
                this._sessionId = AccessTokenSessionId.FromTokenHandle(handle);
            }
            catch { }
            try
            {
                this._user = AccessTokenUser.FromTokenHandle(handle);
            }
            catch { }
        }
    

        public string ToOutputString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[USER]\n");
            sb.Append(this._user?.ToOutputString());
            sb.Append("\n");
            sb.Append("\n");
            sb.Append("[GROUPS]\n");
            sb.Append(this._groups?.ToOutputString());
            sb.Append("\n");
            sb.Append("\n");
            sb.Append("[PRIVILEGES]\n");
            sb.Append(this._privileges?.ToOutputString());
            sb.Append("\n");
            sb.Append("\n");
            sb.Append("[LOGON SID]\n");
            sb.Append(this._logonSid?.ToOutputString());
            sb.Append("\n");
            sb.Append("\n");
            sb.Append("[SESSION ID]\n");
            sb.Append(this._sessionId?.ToOutputString());
            sb.Append("\n");
            sb.Append("\n");
            sb.Append("[OWNER]\n");
            sb.Append(this._owner?.ToOutputString());
            sb.Append("\n");
            sb.Append("\n");
            sb.Append("[PRIMARY GROUP]\n");
            sb.Append(this._primaryGroup?.ToOutputString());
            sb.Append("\n");
            sb.Append("\n");
            return sb.ToString();
        }
    }
}
