using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.AccessTokenAPI
{
    public enum LogLevel
    {
        INFO,
        DEBUG,
        ERROR
    }
    public interface IOutput
    {
        void Log(LogLevel level, string msg);
        void Error(string msg);
        void Debug(string msg);
        void Info(string msg);
    }
}
