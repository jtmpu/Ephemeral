using System;
using System.Runtime.Serialization;

namespace Ephemeral.Chade.Exceptions
{
    [Serializable]
    internal class Win32Exception : Exception
    {
        public Win32Exception()
        {
        }

        public Win32Exception(string message) : base(message)
        {
        }

        public Win32Exception(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected Win32Exception(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}