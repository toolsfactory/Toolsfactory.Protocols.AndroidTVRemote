namespace Toolsfactory.Protocols.AndroidTVRemote.Events
{
    public class StartedChangedEventArgs(bool started) : EventArgs
    {
        public bool Started { get; init; } = started;
    }
}
