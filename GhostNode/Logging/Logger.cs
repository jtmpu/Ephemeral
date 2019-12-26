using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.Ghost.Logging
{
    public class Logger
    {
        private static ILogger _logger;
        public static ILogger GetInstance()
        {
            if (_logger == null)
                _logger = new ConsoleLogging();
            return _logger;
        }
    }
}
