using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.Ghost.Logging
{
    public class NullLogging : ILogger
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
