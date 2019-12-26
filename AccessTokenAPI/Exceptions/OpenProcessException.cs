using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.AccessTokenAPI.Exceptions
{
    public class OpenProcessException : Exception
    {
        public OpenProcessException(string message) : base(message)
        {
        }
    }
}
