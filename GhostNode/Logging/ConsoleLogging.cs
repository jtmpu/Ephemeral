using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.Ghost.Logging
{
    public class ConsoleLogging : ILogger
    {

        public void Debug(string msg)
        {
            Console.WriteLine("[?] " + msg);
        }

        public void Error(string msg)
        {
            Console.WriteLine("[!] " + msg);
        }

        public void Info(string msg)
        {
            Console.WriteLine("[+] " + msg);
        }

        public void Log(LogLevel level, string msg)
        {
            switch(level)
            {
                case LogLevel.DEBUG:
                    this.Debug(msg);
                    break;
                case LogLevel.ERROR:
                    this.Error(msg);
                    break;
                case LogLevel.INFO:
                    this.Info(msg);
                    break;
            }
        }
    }
}
