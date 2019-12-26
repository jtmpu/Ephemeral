using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.Ghost.Logging
{
    public enum LogLevel
    {
        DEBUG,
        ERROR,
        INFO
    }

    public interface ILogger
    {
        void Log(LogLevel level, string msg);
        void Error(string msg);
        void Debug(string msg);
        void Info(string msg);
    }
}
