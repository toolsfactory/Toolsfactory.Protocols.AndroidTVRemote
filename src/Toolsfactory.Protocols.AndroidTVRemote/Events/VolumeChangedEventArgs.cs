namespace Toolsfactory.Protocols.AndroidTVRemote.Events
{
    public class VolumeChangedEventArgs(uint volume, uint maxVolume, bool muted) : EventArgs
    {
        public bool Muted { get; init; } = muted;
        public uint Volume { get; init; } = volume;
        public uint MaxVolume { get; init; } = maxVolume;
    }
}
