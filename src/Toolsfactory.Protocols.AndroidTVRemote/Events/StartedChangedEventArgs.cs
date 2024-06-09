using System;
using System.Linq;

namespace Toolsfactory.Protocols.AndroidTVRemote.Events
{
    public class StartedChangedEventArgs : EventArgs
    {
        public bool Started { get; init; }
        public StartedChangedEventArgs(bool started) => Started = started;
    }
}
