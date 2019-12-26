using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.Ghost.Exceptions
{
    public class Win32Exception : Exception
    {
        public Win32Exception(string message) : base(message)
        {
        }
    }
}
