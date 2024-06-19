using System;
using System.Linq;

namespace Toolsfactory.Protocols.AndroidTVRemote.Events
{
    public class AppLaunchedEventArgs : EventArgs
    {
        public string AppPackage { get; init; }
        public AppLaunchedEventArgs(string appPackage) => AppPackage = appPackage;
    }
}
