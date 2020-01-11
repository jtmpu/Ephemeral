using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.Chade.Logging
{
    public class ConsoleLogger : ILogger
    {
        private readonly object loglock = new object();

        public void Debug(string msg)
        {
            this.Log(LogLevel.DEBUG, msg);
        }

        public void Error(string msg)
        {
            this.Log(LogLevel.ERROR, msg);
        }

        public void Info(string msg)
        {
            this.Log(LogLevel.INFO, msg);
        }

        public void Log(LogLevel level, string msg)
        {
            lock(loglock)
            {
                switch (level)
                {
                    case LogLevel.DEBUG:
                        Console.WriteLine($"[?] {DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}: {msg}");
                        break;
                    case LogLevel.ERROR:
                        Console.WriteLine($"[!] {DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}: {msg}");
                        break;
                    case LogLevel.INFO:
                        Console.WriteLine($"[+] {DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}: {msg}");
                        break;
                }
            }
        }
    }
}
