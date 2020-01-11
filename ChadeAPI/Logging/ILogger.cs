using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.Chade.Logging
{
    public interface ILogger
    {
        void Log(LogLevel level, string msg);
        void Error(string msg);
        void Debug(string msg);
        void Info(string msg);
    }

    public class Logger
    {
        private Logger()
        {

        }

        private static ILogger _loggingInstance;
        public static ILogger GetInstance()
        {
            if(_loggingInstance == null)
            {
                _loggingInstance = new ConsoleLogger();
            }
            return _loggingInstance;
        }
    }
}
