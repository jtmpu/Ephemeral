using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.AccessTokenAPI
{
    /// <summary>
    /// An ugly singleton implementation of output logging. 
    /// The CLI application will override and provide the proper output 
    /// object implementation so that the output can be logged to the commandline.
    /// </summary>
    public class Logger : IOutput
    {
        private static IOutput _logger;
        public static IOutput GetInstance()
        {
            if (_logger == null)
                _logger = new Logger();
            return _logger;
        }

        public static void SetGlobalOutput(IOutput output)
        {
            _logger = output;
        }

        public void Log(LogLevel level, string msg)
        {
        }

        public void Error(string msg)
        {
        }

        public void Debug(string msg)
        {
        }

        public void Info(string msg)
        {
        }
    }
}
