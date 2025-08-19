using Microsoft.Extensions.Logging;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Toolsfactory.Protocols.AndroidTVRemote
{
    public sealed record RemoteControlClientOptions(
        string ServerAddress,
        X509Certificate2 ClientCertificate,
        ushort Port = RemoteControlClient.DefaultPort,
        SslProtocols Protocol = SslProtocols.Tls13,
        ILoggerFactory? LoggerFactory = null,
        bool ValidateServerCertificate = false,
        string? PinnedServerCertificateThumbprint = null);
}
