namespace Toolsfactory.Protocols.AndroidTVRemote.Events
{
    public class PingRequestedEventArgs(int sequenceId) : EventArgs
    {
        public int SequenceId { get; init; } = sequenceId;
    }
}
