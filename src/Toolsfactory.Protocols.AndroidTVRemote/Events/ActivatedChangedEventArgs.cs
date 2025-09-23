namespace Toolsfactory.Protocols.AndroidTVRemote.Events
{
    public class ActivatedChangedEventArgs(ProtoBuf.RemoteFeatures features) : EventArgs
    {
        public ProtoBuf.RemoteFeatures Features { get; init; } = features;
    }
}
