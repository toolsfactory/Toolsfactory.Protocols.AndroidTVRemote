namespace Toolsfactory.Protocols.AndroidTVRemote.Events
{
    public class AppLaunchedEventArgs(string appPackage) : EventArgs
    {
        public string AppPackage { get; init; } = appPackage;
    }
}
