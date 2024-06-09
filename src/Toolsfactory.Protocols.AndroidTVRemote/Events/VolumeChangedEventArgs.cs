using System;
using System.Linq;

namespace Toolsfactory.Protocols.AndroidTVRemote.Events
{
    public class VolumeChangedEventArgs : EventArgs
    {
        public bool Muted { get; init; }
        public uint Volume { get; init; }
        public uint MaxVolume { get; init; }

        public VolumeChangedEventArgs(uint volume, uint maxVolume, bool muted)
        {
            Volume = volume;
            MaxVolume = maxVolume;
            Muted = muted;
        }
    }
}
