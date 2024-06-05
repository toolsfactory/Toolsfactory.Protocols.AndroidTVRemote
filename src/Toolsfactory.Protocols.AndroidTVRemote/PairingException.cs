using System;
using System.Linq;

namespace Toolsfactory.Protocols.AndroidTVRemote
{
    public class PairingException : Exception
    {
        public PairingException(string message) : base(message) { }
    }
}
