using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Toolsfactory.Protocols.AndroidTVRemote.ProtoBuf;

namespace Toolsfactory.Protocols.AndroidTVRemote
{
    public abstract class ClientBase : IDisposable
    {
        #region private fields
        protected CancellationTokenSource _Cts = new CancellationTokenSource();
        protected SslStream? _Stream = null;
        protected TcpClient? _Client = null;
        protected ILogger? _Logger = null;
        protected bool _Logging = false;
        protected ILoggerFactory? _LoggerFactory;
        private bool _disposedValue;
        bool _Connected = false;
        #endregion

        #region Properties
        public string ServerAddress { get; init; }
        public UInt16 Port { get; init; }
        public SslProtocols Protocol { get; init; } = SslProtocols.Tls12;
        /// <summary>
        /// Indicates whether the client is connected to the server on TCP level
        /// </summary>
        public bool Connected 
        { 
            get => _Connected; 
            protected set { if (value == _Connected) return; _Connected = value; OnConnectionChanged(this, new EventArgs()); } 
        }
        public X509Certificate2 ClientCertificate { get; init; }
        

        #endregion

        #region Events
        public event EventHandler? ConnectionChanged;
        protected virtual void OnConnectionChanged(object? sender, EventArgs e) => ConnectionChanged?.Invoke(sender, e);
        #endregion


        #region Constructors
       protected ClientBase(string serverAddress, X509Certificate2 clientCertificate, ILoggerFactory? loggerFactory)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(serverAddress, nameof(serverAddress));
            ServerAddress = serverAddress;
            Protocol = SslProtocols.Tls12;
            ClientCertificate = clientCertificate;
            _LoggerFactory = loggerFactory;
            _Logging = loggerFactory != null;
            _Logger = _LoggerFactory?.CreateLogger<ClientBase>();
            _Connected = false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Close();
                    _Stream?.Dispose();
                    _Stream = null;
                    _Client?.Dispose();
                    _Client = null;
                }
                _disposedValue = true;
            }
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
            if (Connected)
                return;

            if (_Logging) _Logger!.LogDebug($"Connecting to {ServerAddress}:{Port}");
            await SetupStreamAsync();

            // Handle incoing Data in a background thread
            _ = Task.Run(() => HandleClientAsync());

            Connected = true;
            if (_Logging) _Logger!.LogInformation($"Connected to {ServerAddress}:{Port}");
        }

        public virtual void Close()
        {
            if (!Connected)
                return;
            if (_Logging) _Logger!.LogDebug($"Actively closing connection to {ServerAddress}:{Port}");
            _Cts.Cancel();
            _Stream?.Close();
            _Client?.Close();
            Connected = false;
        }

        protected async Task SetupStreamAsync()
        {
            if (_Stream != null)
                return;

            if (ClientCertificate == null)
                throw new PairingException($"Set a client certificate before requesting a secure stream!");

            var callback = new RemoteCertificateValidationCallback((sender, certificate, chain, error) => { return true; });
            _Client = new TcpClient();
            await _Client.ConnectAsync(ServerAddress, Port);
            _Stream = new SslStream(_Client.GetStream(), false, callback, null);
            _Stream.AuthenticateAsClient(ServerAddress, new X509CertificateCollection() { ClientCertificate }, Protocol, false);
        }

        private async Task HandleClientAsync()
        {
            while (!_Cts.Token.IsCancellationRequested)
            {
                var read = await ReceiveResponseMessageAsync(_Cts.Token);
                if (read != null)
                {
                    try
                    {
                        if (_Logging) _Logger!.LogDebug($"Received raw: {read.ToHex()}");
                        ProcessMessage(read);
                    }
                    catch (Exception ex)
                    {
                        if (_Logging) _Logger!.LogError($"Error processing message: {ex.Message}");
                        if (_Logging) _Logger!.LogDebug($"Raw Message: {read.ToHex()}");
                    }
                }
            }
        }

        async Task<byte[]> ReceiveResponseMessageAsync(CancellationToken token)
        {
            var length = await _Stream!.ReadVarIntAsync(token);
            var bytes = await _Stream!.ReadBytesAsync((int)length, token);
            return bytes;
        }

        #endregion

        protected abstract void ProcessMessage(byte[] data);

    }
}
