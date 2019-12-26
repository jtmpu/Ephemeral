using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.WinAPI
{
    public class Constants
    {
        public const int ERROR_NOT_ALL_ASSIGNED = 1300;
        public const int ERROR_INSUFFICIENT_BUFFER = 122;

        public const int SE_PRIVILEGE_ENABLED = 0x00000002;
        public const int SE_PRIVILEGE_DISABLED = 0x00000000;
        public const int SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001;
        public const int SE_PRIVILEGE_REMOVED = 0x00000004;
        public const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        public const UInt32 STANDARD_RIGHTS_READ = 0x00020000;
        public const UInt32 TOKEN_ASSIGN_PRIMARY = 0x0001;
        public const UInt32 TOKEN_DUPLICATE = 0x0002;
        public const UInt32 TOKEN_IMPERSONATE = 0x0004;
        public const UInt32 TOKEN_QUERY = 0x0008;
        public const UInt32 TOKEN_QUERY_SOURCE = 0x0010;
        public const UInt32 TOKEN_ADJUST_PRIVILEGES = 0x0020;
        public const UInt32 TOKEN_ADJUST_GROUPS = 0x0040;
        public const UInt32 TOKEN_ADJUST_DEFAULT = 0x0080;
        public const UInt32 TOKEN_ADJUST_SESSIONID = 0x0100;
        public const UInt32 TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
        public const UInt32 TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
            TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
            TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
            TOKEN_ADJUST_SESSIONID);

        //Constants for dwDesiredAccess:
        public const UInt32 GENERIC_READ = 0x80000000;
        public const UInt32 GENERIC_WRITE = 0x40000000;

        //Constants for return value:
        public const Int32 INVALID_HANDLE_VALUE = -1;

        //Constants for dwFlagsAndAttributes:
        public const UInt32 FILE_FLAG_OVERLAPPED = 0x40000000;
        public const UInt32 FILE_FLAG_NO_BUFFERING = 0x20000000;

        //Constants for dwCreationDisposition:
        public const UInt32 OPEN_EXISTING = 3;

        public const uint PIPE_ACCESS_OUTBOUND = 0x00000002;

        public const uint PIPE_ACCESS_DUPLEX = 0x00000003;

        public const uint PIPE_ACCESS_INBOUND = 0x00000001;
        public const uint PIPE_WAIT = 0x00000000;
        public const uint PIPE_NOWAIT = 0x00000001;
        public const uint PIPE_READMODE_BYTE = 0x00000000;
        public const uint PIPE_READMODE_MESSAGE = 0x00000002;
        public const uint PIPE_TYPE_BYTE = 0x00000000;
        public const uint PIPE_TYPE_MESSAGE = 0x00000004;
        public const uint PIPE_CLIENT_END = 0x00000000;
        public const uint PIPE_SERVER_END = 0x00000001;
        public const uint PIPE_UNLIMITED_INSTANCES = 255;
        public const uint NMPWAIT_WAIT_FOREVER = 0xffffffff;
        public const uint NMPWAIT_NOWAIT = 0x00000001;
        public const uint NMPWAIT_USE_DEFAULT_WAIT = 0x00000000;
        public const ulong ERROR_PIPE_CONNECTED = 535;

        public static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);
        public static UIntPtr HKEY_CURRENT_USER = new UIntPtr(0x80000001u);
        public const uint ERROR_SUCCESS = 0x0;

        public enum PipeMode
        {
            InboundOnly = (int)PIPE_ACCESS_INBOUND,
            OutboundOnly = (int)PIPE_ACCESS_OUTBOUND,
            Bidirectional = (int)(PIPE_ACCESS_INBOUND + PIPE_ACCESS_OUTBOUND)
        };

        public const uint TRUSTEE_IS_NAME = 0x1;
        public const uint TRUSTEE_IS_SID = 0x0;
        public const uint TRUSTEE_IS_USER = 0x1;
        public const uint TRUSTEE_IS_WELL_KNOWN_GROUP = 0x5;
        public const uint TRUSTEE_IS_GROUP = 0x2;
        public const uint GRANT_ACCESS = 0x1;
        public const uint OBJECT_INHERIT_ACE = 0x1;
        public const uint DESKTOP_GENERIC_ALL = 0x000F01FF;
        public const uint WRITE_DAC = 0x00040000;


        public const int STD_INPUT_HANDLE = -10;
        public const int STD_ERROR_HANDLE = -12;
        public const int STD_OUTPUT_HANDLE = -11;
    }
}
