namespace Toolsfactory.Protocols.AndroidTVRemote
{
    public class PairingException(string message) : Exception(message);

    public class PairingAttemptsException(string message) : Exception(message);
}
