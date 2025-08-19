using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Toolsfactory.Protocols.AndroidTVRemote.Extensions;

namespace Toolsfactory.Protocols.AndroidTVRemote
{
    public abstract class ClientBase : IDisposable
    {
        #region Fields

        protected CancellationTokenSource? Cts;
        protected readonly bool Logging;
        protected SslStream? Stream;
        protected TcpClient? Client;
        protected ILogger? Logger;
        private bool _disposedValue;
        private bool _connected;
        #endregion

        #region Properties
        protected string ServerAddress { get; init; }
        protected ushort Port { get; init; }
        protected SslProtocols Protocol { get; init; }
        protected X509Certificate2 ClientCertificate { get; }
        protected bool ValidateServerCertificate { get; init; }
        protected string? PinnedServerCertificateThumbprint { get; init; }
        
        /// <summary>
        /// Indicates whether the client is connected to the server on TCP level
        /// </summary>
        protected bool Connected 
        { 
            get => _connected;
            set
            {
                if (value == _connected) return; 
                
                _connected = value; 
                OnConnectionChanged(this, EventArgs.Empty);
            } 
        }
        #endregion

        #region Events
        // ReSharper disable once EventNeverSubscribedTo.Global
        public event EventHandler? ConnectionChanged;
        private void OnConnectionChanged(object? sender, EventArgs e) => ConnectionChanged?.Invoke(sender, e);
        #endregion

        #region Constructors
        protected ClientBase(string serverAddress, X509Certificate2 clientCertificate, ILoggerFactory? loggerFactory)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(serverAddress, nameof(serverAddress));
            ServerAddress = serverAddress;
            Protocol = SslProtocols.Tls13;
            ClientCertificate = clientCertificate;
            Logging = loggerFactory != null;
            Logger = loggerFactory?.CreateLogger<ClientBase>();
            _connected = false;
        }

        private void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
            {
                Close();
                Stream?.Dispose();
                Stream = null;
                Client?.Dispose();
                Client = null;
                Cts!.Dispose();
            }
            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Connection management
        protected async Task InitiateConnectionAsync()
        {
            if (Connected) return;

            if (Logging) Logger!.LogDebug("Connecting to {S}:{Port1}", ServerAddress, Port);
            await SetupStreamAsync();

            Cts?.Dispose();
            Cts = new CancellationTokenSource();

            var token = Cts.Token;
            _ = Task.Run(async () => await HandleClientAsync(), token);

            Connected = true;
            if (Logging) Logger!.LogInformation("Connected to {S}:{Port1}", ServerAddress, Port);
        }

        protected virtual void Close()
        {
            if (Logging) Logger!.LogDebug("Actively closing connection to {S}:{Port1}", ServerAddress, Port);
            
            try
            {
                Cts?.Cancel();
            }
            catch { /* ignore */ }

            try
            {
                Stream?.Dispose();
                Client?.Dispose();
            }
            catch { /* ignore */ }

            try
            {
                Cts?.Dispose();
                Cts = null;
            }
            catch { /* ignore */ }

            Connected = false;
        }

        private async Task SetupStreamAsync()
        {
            if (Stream != null) return;

            if (ClientCertificate == null)
                throw new PairingException("Set a client certificate before requesting a secure stream!");

            RemoteCertificateValidationCallback callback;

            if (!ValidateServerCertificate)
                callback = static (sender, certificate, chain, error) => true;
            else
            {
                callback = (sender, certificate, chain, errors) =>
                {
                    if (errors != SslPolicyErrors.None)
                        return false;

                    if (!string.IsNullOrWhiteSpace(PinnedServerCertificateThumbprint) && certificate is not null)
                    {
                        using var serverCert2 = new X509Certificate2(certificate);
                        string thumbActual = serverCert2.Thumbprint.Replace(" ", "").ToUpperInvariant();
                        string thumbPinned = PinnedServerCertificateThumbprint.Replace(" ", "").ToUpperInvariant();
                        return thumbActual == thumbPinned;
                    }

                    return true;
                };
            }
            Client = new TcpClient();
            await Client.ConnectAsync(ServerAddress, Port);
            Stream = new SslStream(Client.GetStream(), false, callback, null);
            await Stream.AuthenticateAsClientAsync(ServerAddress, new X509CertificateCollection { ClientCertificate }, Protocol, false);
            
            Cts?.Dispose();
            Cts = new CancellationTokenSource();

            var token = Cts.Token;
            _ = Task.Run(async () => await HandleClientAsync(), token);
        }

        private async Task HandleClientAsync()
        {
            while (!Cts!.Token.IsCancellationRequested)
            {
                var read = await ReceiveResponseMessageAsync(Cts.Token);
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (read == null) continue;
                try
                {
                    if (Logging) Logger!.LogDebug($"Received raw: {read.ToHex()}");
                    ProcessMessage(read);
                }
                catch (Exception ex)
                {
                    if (Logging) Logger!.LogError($"Error processing message: {ex.Message}");
                    if (Logging) Logger!.LogDebug($"Raw Message: {read.ToHex()}");
                }
            }
        }

        private async Task<byte[]> ReceiveResponseMessageAsync(CancellationToken token)
        {
            var length = await Stream!.ReadVarIntAsync(token);
            var bytes = await Stream!.ReadBytesAsync((int)length, token);
            return bytes;
        }
        #endregion

        protected abstract void ProcessMessage(byte[] data);
    }
}
