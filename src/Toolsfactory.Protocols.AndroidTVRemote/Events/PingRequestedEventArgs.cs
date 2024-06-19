using System;
using System.Linq;

namespace Toolsfactory.Protocols.AndroidTVRemote.Events
{
    public class PingRequestedEventArgs : EventArgs
    {
        public int SequenceId { get; init; }
        public PingRequestedEventArgs(int sequenceId) => SequenceId = sequenceId;
    }
}
