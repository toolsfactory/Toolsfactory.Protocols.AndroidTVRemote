using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Tls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Toolsfactory.Protocols.AndroidTVRemote
{
    public sealed class PairingClient : ClientBase
    {
        public const UInt16 DefaultPort = 6467;
        private ManualResetEventSlim _PairingEvent = new ManualResetEventSlim(false);

        #region Properties
        public string PrivateKeyPEM { get; private set; } = string.Empty;
        public string ClientCertificatePEM { get; private set; } = string.Empty;
        public X509Certificate2? ServerCertificate { get; private set; }
        public bool IsPairing { get; private set; } = false;

        #endregion

        public PairingClient(string serverAddress, X509Certificate2 clientCertificate)
            : this(new PairingClientOptions(serverAddress, clientCertificate)) { }

        public PairingClient(PairingClientOptions options) 
            : base(options.ServerAddress, options.ClientCertificate, options.LoggerFactory)
        {
            Port = options.Port;
            Protocol = options.Protocol;
            _Logger = options.LoggerFactory?.CreateLogger<PairingClient>();

            GenerateClientCertificate();
            ClientCertificate = CertificateBuilder.LoadCertificateFromPEM(ClientCertificatePEM, PrivateKeyPEM);
        }

        public async Task PreparePairingAsync()
        {
            if (IsPairing)
                throw new PairingException("Already pairing");

            if (ServerCertificate == null)
                await GetServerCertificateAsync();
        }

        public async Task<bool> StartPairingAsync()
        {
            if (IsPairing)
                return false;

            try
            {
                await InitiateConnectionAsync();
                await SendPairingMessageAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<PairingResult> FinishPairingAsync(string code)
        {
            if (code.Length != 6 || !code.IsHexString())
                return PairingResult.InvalidCode;

            _PairingEvent.Reset();
            var hex = code.HexToByteArray();
            var nonce = hex[1..];
            var check = hex[0..1];
            var hash = CalculatePairingCodeHash(nonce);

            await SendPairingSecretMessageAsync(hash);
            var set = _PairingEvent.Wait(15000, _Cts.Token);
            _Client!.Close();
            return set == true ? PairingResult.Success : PairingResult.Timeout;
        }

        public void CancelPairing() 
        {
            _Cts.Cancel();
        }

        #region Certificate
        private void GenerateClientCertificate()
        {
            var certNameOptions = new CertificateNameOptions("atvremote", "US", "California", "Mountain View", "Google Inc.", "Android", "example@google.com");
            var certOptions = new CertificateOptions(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddYears(10));
            var certBuilder = new CertificateBuilder(certNameOptions, certOptions);

            ClientCertificatePEM = certBuilder.CertificateAsPEM();
            PrivateKeyPEM = certBuilder.PrivateKeyAsPEM();
        }

        #endregion


        private async Task GetServerCertificateAsync()
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect(ServerAddress, Port);

                SslStream ssl = new SslStream(
                    client.GetStream(),
                    false,
                    new RemoteCertificateValidationCallback((sender, certificate, chain, error) =>
                    {
                        if (certificate != null)
                        {
                            ServerCertificate = new X509Certificate2(certificate);
                        }
                        return true;
                    }), null);

                try
                {
                    await ssl.AuthenticateAsClientAsync(ServerAddress);
                }
                finally
                {
                    ssl.Close();
                    client.Close();
                }
            }
        }

        protected override void ProcessMessage(byte[] data)
        {
            var message = ProtoBuf.OuterMessage.Parser.ParseFrom(data);
            if (_Logging) _Logger!.LogDebug($"Received message: {message}");

            if(message.Status != ProtoBuf.OuterMessage.Types.Status.Ok)
            {

               if (_Logging) _Logger!.LogError($"Error processing message: {message.Status}");
                return;
            }

            if (message.PairingRequestAck != null)
                SendPairingOptionMessage();
            else if (message.Options != null)
                SendConfigurationMessage();
            else if (message.ConfigurationAck != null) { }
            else if (message.SecretAck != null)
                _PairingEvent.Set();
            else
            {
                if (_Logging) _Logger!.LogError($"Unknown message type: {message}");
            }
        }

        private async Task<ProtoBuf.OuterMessage> ReceiveResponseMessageAsync(CancellationToken token)
        {
            var length = await _Stream!.ReadVarIntAsync(token);
            var bytes = await _Stream!.ReadBytesAsync((int)length, token);
            var msg = ProtoBuf.OuterMessage.Parser.ParseFrom(bytes);
            return msg;
        }

        private async Task SendPairingMessageAsync()
        {
            var msg = PairingMessageBuilder.BuildPairingMessage("atvremote", "demo");
            await _Stream!.WriteProtoBufMessageAsync(msg, _Cts.Token);
        }

        private void SendPairingOptionMessage()
        {
            var msg = PairingMessageBuilder.BuildPairingOptionMessage();
            _Stream!.WriteProtoBufMessageAsync(msg, _Cts.Token);
        }

        private void SendConfigurationMessage()
        {
            var msg = PairingMessageBuilder.BuildConfigurationMessage();
            _Stream!.WriteProtoBufMessageAsync(msg, _Cts.Token);
        }

        private async Task SendPairingSecretMessageAsync(byte[] secret)
        {
            var msg = PairingMessageBuilder.BuildPairingSecretMessage(secret);
            await _Stream!.WriteProtoBufMessageAsync(msg, _Cts.Token);
        }

        private byte[] CalculatePairingCodeHash(byte[] nonce)
        {
            (var c_modulus, var c_exponent) = GetModExp(ClientCertificate);
            (var s_modulus, var s_exponent) = GetModExp(ServerCertificate);

            /*
             * Not sure why the hash is different from the one calculated by the BouncyCastle library
            var data = Combine(c_modulus, c_exponent, s_modulus, s_exponent, nonce);
            System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create();
            var hash =  sha256.ComputeHash(data);
            */

            var instance = new Sha256Digest();
            instance.BlockUpdate(c_modulus, 0, c_modulus.Length);
            instance.BlockUpdate(c_exponent, 0, c_exponent.Length);
            instance.BlockUpdate(s_modulus, 0, s_modulus.Length);
            instance.BlockUpdate(s_exponent, 0, s_exponent.Length);
            instance.BlockUpdate(nonce, 0, nonce.Length);

            byte[] hash = new byte[instance.GetDigestSize()];
            instance.DoFinal(hash, 0);
            return hash;


            static byte[] Combine(byte[] cmod, byte[] cexp, byte[] smod, byte[] sexp, byte[] nonc)
            {
                byte[] ret = new byte[cmod.Length + cexp.Length + smod.Length + sexp.Length + nonc.Length + 2];
                Buffer.BlockCopy(cmod, 0, ret, 0, cmod.Length);
                Buffer.BlockCopy(cexp, 0, ret, cmod.Length, cexp.Length);
                Buffer.BlockCopy(smod, 0, ret, cmod.Length + 1 + cexp.Length, smod.Length);
                Buffer.BlockCopy(sexp, 0, ret, cmod.Length + 1 + cexp.Length + smod.Length, sexp.Length);
                Buffer.BlockCopy(nonc, 0, ret, cmod.Length + 1 + cexp.Length + smod.Length + 1 + sexp.Length, nonc.Length);
                return ret;
            }

            static (byte[] Mod, byte[] Exp) GetModExp(X509Certificate2 serverCertificate)
            {
                RSA? rsa = serverCertificate!.GetRSAPublicKey();
                RSAParameters srsaParameters = rsa.ExportParameters(false);
                return (srsaParameters.Modulus, srsaParameters.Exponent);
            }
        }
    }
}
