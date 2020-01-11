using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.Chade.Logging
{
    public class NullLogger : ILogger
    {
        public void Debug(string msg)
        {
        }

        public void Error(string msg)
        {
        }

        public void Info(string msg)
        {
        }

        public void Log(LogLevel level, string msg)
        {
        }
    }
}
