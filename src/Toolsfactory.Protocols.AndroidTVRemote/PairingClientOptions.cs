using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Toolsfactory.Protocols.AndroidTVRemote
{
    public sealed record PairingClientOptions(
        string ServerAddress,
        X509Certificate2 ClientCertificate,
        ushort Port = PairingClient.DefaultPort,
        SslProtocols Protocol = SslProtocols.Tls12,
        ILoggerFactory? LoggerFactory = null);
}
