using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace Ephemeral.GhostCLI.UI
{
    public class CommandPrompt
    {
        private string _prompt;
        private string[] _args;

        public CommandPrompt(string[] args, string prompt = null)
        {
            if (prompt == null)
                this._prompt = $"{Environment.UserName}@{Environment.MachineName} > ";
            else
                this._prompt = prompt;

            this._args = args;
        }

        public void Execute()
        {

            this.InteractiveLoop();
        }

        private void InteractiveLoop()
        {
            var client = new NamedPipeClientStream("test");
            client.Connect();
            var reader = new StreamReader(client);
            var writer = new StreamWriter(client);

            while(true)
            {
                Console.Write(this._prompt);
                var line = Console.ReadLine();

                if (line.ToLower().Equals("exit"))
                    break;

                writer.WriteLine(line);
                writer.Flush();
                var resp = reader.ReadLine();
                Console.WriteLine("Server sent: " + resp);
            }

            client.Close();
        }
    }
}
