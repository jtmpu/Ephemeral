using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using Ephemeral.Ghost.Channels.Events;
using System.Linq;

namespace Ephemeral.Ghost.Channels
{
    public class NamedPipeChannel
    {
        public string PipeName { get; }
        public event ChannelMessageReceivedEventHandler ChannelMessageReceived;

        public NamedPipeChannel(string pipeName = null)
        {
            this.PipeName = pipeName == null ? "test" : pipeName;
        }

        private void DisallowAnonymous()
        {
            /*
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters", RegistryKeyPermissionCheck.ReadWriteSubTree);
            var values = new List<string>((string[])key.GetValue("NullSessionPipes"));
            if (values.Where(x => x.Equals(this.PipeName)).Count() == 1)
            {
                values.Remove(this.PipeName);
            }
            key.SetValue("NullSessionPipes", values.ToArray());
            */
        }
        private void AllowAnonymous()
        {
            /*
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters", RegistryKeyPermissionCheck.ReadWriteSubTree);
            var values = new List<string>((string[])key.GetValue("NullSessionPipes"));

            if(values.Where(x => x.Equals(this.PipeName)).Count() != 1)
            {
                values.Add(this.PipeName);
            }

            key.SetValue("NullSessionPipes", values.ToArray());
            */
        }

        public void Start()
        {
            //AllowAnonymous();
            /*
            PipeSecurity ps = new PipeSecurity();
            System.Security.Principal.SecurityIdentifier sid = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.WorldSid, null);
            PipeAccessRule a = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
            ps.AddAccessRule(a);
            var server = new NamedPipeServerStream(this.PipeName, PipeDirection.InOut);
            server.SetAccessControl(ps);
            server.WaitForConnection();
            StreamReader reader = new StreamReader(server);
            StreamWriter writer = new StreamWriter(server);
            try
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    OnMessageReceived(line);
                    Console.WriteLine("Received: " + line);
                    writer.WriteLine(line);
                    writer.Flush();
                }
            }
            catch
            {

            }

            server.Disconnect();
            server.Close();
            */
           // DisallowAnonymous();
        }

        protected void OnMessageReceived(string message)
        {
            this.ChannelMessageReceived?.Invoke(this, new ChannelMessageReceivedEventArgs(message));
        }

        public void RegisterMessageReceivedHandler(ChannelMessageReceivedEventHandler handler)
        {
            this.ChannelMessageReceived += handler;
        }

        public void Write(string message)
        {

        }
    }
}
