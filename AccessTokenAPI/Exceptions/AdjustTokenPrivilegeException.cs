using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.AccessTokenAPI.Exceptions
{
    class AdjustTokenPrivilegeException : Exception
    {
        public AdjustTokenPrivilegeException(string message) : base(message)
        {
        }
    }
}
