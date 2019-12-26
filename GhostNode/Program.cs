using Ephemeral.Ghost.Channels;
using Ephemeral.Ghost.Domain;
using System;
using Ephemeral.WinAPI;
using System.Text;
using Ephemeral.Ghost.Logging;

namespace Ephemeral.Ghost
{
    class Program
    {
        static void Main(string[] args)
        {
            //var channel = new NamedPipeChannel("asdqwe123");
            //channel.Start();
            var builder = new NamedPipeBuilder();
            builder.SetName("test").
                SetMode(PipeModeFlags.PIPE_TYPE_BYTE, PipeModeFlags.PIPE_WAIT).
                SetOpenMode(PipeOpenModeFlags.PIPE_ACCESS_DUPLEX).
                SetInBufferSize(1024).
                SetOutBufferSize(1024).
                SetNullDACL(true);
            var pipe = builder.Create();
            Logger.GetInstance().Info($"Created pipe with name '{pipe.Name}");

            try
            {
                NamedPipeBuilder.ConfigureNullLogonSession(pipe.Name);
                Logger.GetInstance().Info("Allowed anonymous access.");
            }
            catch
            {
                Logger.GetInstance().Info("Failed to allow anonymous access, not running as administrator?.");
            }

            Logger.GetInstance().Info("Listening for clients...");
            pipe.Listen();
            Logger.GetInstance().Info("Received connection!");

            for(int i = 0; i < 2; i++)
            {
                var buffer = new byte[1024];
                var read = pipe.Read(buffer, 0, 1024);
                var str = Encoding.UTF8.GetString(buffer, 0, read);
                Console.WriteLine("Received: " + str);

                pipe.Write(buffer, 0, read);
            }

            pipe.Close();

            try
            {
                NamedPipeBuilder.RemoveNullLogonSession(pipe.Name);
                Logger.GetInstance().Info("Removed anonymous access.");
            }
            catch
            {
                Logger.GetInstance().Info("Failed to remove anonymous access, not running as administrator?.");
            }
        }
    }
}
