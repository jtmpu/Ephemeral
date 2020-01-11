using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.Chade.Communication
{
    public interface IConnector
    {
        /// <summary>
        /// Sets up a connection.
        /// </summary>
        IChannel EstablishOnce();

        /// <summary>
        /// Removes all resources only connected to the connector. No channels
        /// will get removed.
        /// </summary>
        void Dispose();
    }
}
