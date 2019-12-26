using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.AccessTokenAPI.Exceptions
{
    public class AuthenticationFailedException : Exception
    {
        public AuthenticationFailedException(string message) : base(message)
        {
        }
    }
}
