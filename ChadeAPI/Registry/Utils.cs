using Ephemeral.Chade.Exceptions;
using Ephemeral.Chade.Logging;
using Ephemeral.WinAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Ephemeral.Chade.Registry
{
    public class Utils
    {
        public static void ConfigureNullLogonSession(string name)
        {
            var pipes = GetNullLogonSessionRegistry();

            if (!pipes.Contains(name))
            {
                pipes.Add(name);
                SetNullLogonSessionRegistry(pipes);
            }
        }

        public static void RemoveNullLogonSession(string name)
        {
            var pipes = GetNullLogonSessionRegistry();
            if (pipes.Contains(name))
            {
                pipes.Remove(name);
                SetNullLogonSessionRegistry(pipes);
            }
        }

        private static List<string> GetNullLogonSessionRegistry()
        {
            UIntPtr hKey;
            var access = (int)RegistryRights.KEY_READ;
            var result = Kernel32.RegOpenKeyEx(Constants.HKEY_LOCAL_MACHINE, @"SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters", 0, access, out hKey);
            if (result != Constants.ERROR_SUCCESS)
            {
                var msg = $"Failed to open registry lanman server's registry key with read write access. RegOpenKeyEx failed with error code: {Kernel32.GetLastError()}";
                Logger.GetInstance().Error(msg);
                throw new Win32Exception(msg);
            }

            var kind = RegistryValueKind.REG_MULTI_SZ;
            int size = 0;
            result = Kernel32.RegQueryValueEx(hKey, "NullSessionPipes", 0, ref kind, IntPtr.Zero, ref size);

            if (result != Constants.ERROR_SUCCESS)
            {
                var msg = $"Failed to open NullSessionPipes key with read write access. RegQueryValueEx failed with error code: {Kernel32.GetLastError()}";
                Kernel32.RegCloseKey(hKey);
                Logger.GetInstance().Error(msg);
                throw new Win32Exception(msg);
            }

            IntPtr ptr = Marshal.AllocHGlobal(size);
            result = Kernel32.RegQueryValueEx(hKey, "NullSessionPipes", 0, ref kind, ptr, ref size);
            var bytes = new byte[size];
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);

            // The multi_sz type is an array of string separated by the null byte.
            var str = Encoding.Unicode.GetString(bytes);
            var splits = str.Split('\0');


            Kernel32.RegCloseKey(hKey);

            return splits.Where(x => x != "").ToList();
        }

        private static void SetNullLogonSessionRegistry(List<string> pipes)
        {
            UIntPtr hKey;
            var access = (int)RegistryRights.KEY_READ | (int)RegistryRights.KEY_WRITE;
            var result = Kernel32.RegOpenKeyEx(Constants.HKEY_LOCAL_MACHINE, @"SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters", 0, access, out hKey);
            if (result != Constants.ERROR_SUCCESS)
            {
                var msg = $"Failed to open registry lanman server's registry key with read write access. RegOpenKeyEx failed with error code: {result}";
                Logger.GetInstance().Error(msg);
                throw new Win32Exception(msg);
            }

            var value = string.Join("\0", pipes) + "\0";

            //Ensure we use UTF-8 encoding, otherwise something goes wrong i think.
            var bytes = Encoding.UTF8.GetBytes(value);

            IntPtr dst = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, dst, bytes.Length);

            var kind = RegistryValueKind.REG_MULTI_SZ;
            result = Kernel32.RegSetValueEx(hKey, "NullSessionPipes", 0, kind, dst, bytes.Length);

            if (result != Constants.ERROR_SUCCESS)
            {
                var msg = $"Failed to set NullSessionPipes key. RegSetValueEx failed with error code: {result}";
                Logger.GetInstance().Error(msg);
                Marshal.FreeHGlobal(dst);
                Kernel32.RegCloseKey(hKey);
                throw new Win32Exception(msg);
            }

            Kernel32.RegCloseKey(hKey);
            Marshal.FreeHGlobal(dst);
        }
    }
}
