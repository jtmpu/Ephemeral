using System;
using System.Collections.Generic;
using System.Text;

namespace Ephemeral.Ghost.Channels.Events
{
    public delegate void ChannelMessageReceivedEventHandler(object sender, ChannelMessageReceivedEventArgs e);

    public class ChannelMessageReceivedEventArgs : EventArgs
    {
        public string Line;

        public ChannelMessageReceivedEventArgs(string line)
        {
            this.Line = line;
        }
    }
}
