using System;
using System.Linq;

namespace Toolsfactory.Protocols.AndroidTVRemote.Events
{
    public class ActivatedChangedEventArgs : EventArgs
    {
        public ProtoBuf.RemoteFeatures Features { get; init; }
        public ActivatedChangedEventArgs(ProtoBuf.RemoteFeatures features) => Features = features;
    }
}
