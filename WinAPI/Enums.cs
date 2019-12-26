using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.WinAPI
{

    [Flags]
    public enum PipeOpenModeFlags : uint
    {
        PIPE_ACCESS_DUPLEX = 0x00000003,
        PIPE_ACCESS_INBOUND = 0x00000001,
        PIPE_ACCESS_OUTBOUND = 0x00000002,
        FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000,
        FILE_FLAG_WRITE_THROUGH = 0x80000000,
        FILE_FLAG_OVERLAPPED = 0x40000000,
        WRITE_DAC = 0x00040000,
        WRITE_OWNER = 0x00080000,
        ACCESS_SYSTEM_SECURITY = 0x01000000
    }

    [Flags]
    public enum PipeModeFlags : uint
    {
        //One of the following type modes can be specified. The same type mode must be specified for each instance of the pipe.
        PIPE_TYPE_BYTE = 0x00000000,
        PIPE_TYPE_MESSAGE = 0x00000004,
        //One of the following read modes can be specified. Different instances of the same pipe can specify different read modes
        PIPE_READMODE_BYTE = 0x00000000,
        PIPE_READMODE_MESSAGE = 0x00000002,
        //One of the following wait modes can be specified. Different instances of the same pipe can specify different wait modes.
        PIPE_WAIT = 0x00000000,
        PIPE_NOWAIT = 0x00000001,
        //One of the following remote-client modes can be specified. Different instances of the same pipe can specify different remote-client modes.
        PIPE_ACCEPT_REMOTE_CLIENTS = 0x00000000,
        PIPE_REJECT_REMOTE_CLIENTS = 0x00000008
    }

    enum RegWow64Options
    {
        None = 0,
        KEY_WOW64_64KEY = 0x0100,
        KEY_WOW64_32KEY = 0x0200
    }

    public enum RegistryRights : int
    {
        KEY_ALL_ACCESS = 0xf003f,
        KEY_CREATE_LINK = 0x0020,
        KEY_CREATE_SUB_KEY = 0x0004,
        KEY_ENUMERATE_SUB_KEYS = 0x0008,
        KEY_EXECUTE = 0x20019,
        KEY_NOTIFY = 0x0010,
        KEY_QUERY_VALUE = 0x0001,
        KEY_READ = 0x20019,
        KEY_SET_VALUE = 0x0002,
        KEY_WOW64_32KEY = 0x0200,
        KEY_WOW64_64KEY = 0x0100,
        KEY_WRITE = 0x20006
    }

    public enum RegistryValueKind : uint
    {
        REG_NONE = 0,
        REG_SZ = 1,
        REG_EXPAND_SZ = 2,
        REG_BINARY = 3,
        REG_DWORD = 4,
        REG_DWORD_LITTLE_ENDIAN = 4,
        REG_DWORD_BIG_ENDIAN = 5,
        REG_LINK = 6,
        REG_MULTI_SZ = 7
    }
}
