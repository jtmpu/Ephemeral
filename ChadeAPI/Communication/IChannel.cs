using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.Chade.Communication
{
    public interface IChannel
    {

        /// <summary>
        /// Locking read operation.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        int Read(byte[] buffer, int offset, int size);

        /// <summary>
        /// Locking write operation.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        int Write(byte[] buffer, int offset, int size);

        /// <summary>
        /// Checks if the channel is connected.
        /// </summary>
        /// <returns></returns>
        bool IsConnected();

        void Flush();

        void Dispose();
    }
}
