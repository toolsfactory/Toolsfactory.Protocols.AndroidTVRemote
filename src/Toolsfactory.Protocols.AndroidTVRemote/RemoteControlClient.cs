using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Toolsfactory.Protocols.AndroidTVRemote.ProtoBuf;

namespace Toolsfactory.Protocols.AndroidTVRemote
{
    public class RemoteControlClient : ClientBase
    {
        public const UInt16 DefaultPort = 6466;
        public RemoteControlClient(string serverAddress, X509Certificate2 clientCertificate) 
            : this(new RemoteControlClientOptions(serverAddress, clientCertificate)) { }

        public RemoteControlClient(RemoteControlClientOptions options)
            : base(options.ServerAddress, options.ClientCertificate, options.LoggerFactory)
        {
            Port = options.Port;
            Protocol = options.Protocol;
            _Logger = options.LoggerFactory?.CreateLogger<RemoteControlClient>();
        }

        public async Task ConnectAsync() => await InitiateConnectionAsync();

        public async Task PressKeyAsync(RCKeyCode key, KeyEventType keType = KeyEventType.Press)
        {
            if (!Connected)
                return;

            var msg = new RemoteMessage { RemoteKeyInject = new RemoteKeyInject() };
            msg.RemoteKeyInject.KeyCode = (RemoteKeyCode) key;
            msg.RemoteKeyInject.Direction = (RemoteDirection) keType;
            await _Stream!.WriteProtoBufMessageAsync(msg, _Cts.Token);
        }

        public async Task SendLaunchAppAsync(string appLink)
        {
            if (!Connected)
                return;

            var msg = new RemoteMessage { RemoteAppLinkLaunchRequest = new RemoteAppLinkLaunchRequest() };
            msg.RemoteAppLinkLaunchRequest.AppLink = appLink;
            await _Stream!.WriteProtoBufMessageAsync(msg, _Cts.Token);
        }


        #region Message Processing
        protected override void ProcessMessage(byte[] data)
        {
            var message = ProtoBuf.RemoteMessage.Parser.ParseFrom(data);
            if (_Logging) _Logger!.LogDebug($"Received message: {message}");

            if (message.RemoteConfigure != null)
                ProcessRemoteConfigureMessage(message.RemoteConfigure);
            else if (message.RemoteSetActive != null)
                ProcessRemoteSetActiveMessage(message.RemoteSetActive);
            else if (message.RemoteStart != null)
                ProcessRemoteStart(message.RemoteStart);
            else if (message.RemoteImeKeyInject != null)
                ProcessRemoteImeKeyInject(message.RemoteImeKeyInject);
            else if (message.RemoteSetVolumeLevel != null)
                ProcessRemoteRemoteSetVolumeLevel(message.RemoteSetVolumeLevel);
            else if (message.RemotePingRequest != null)
                ProcessRemotePingRequest(message.RemotePingRequest);
            else
            {
                Debug.WriteLine($"Unknown message type: {message}");
            }
        }

        private void ProcessRemotePingRequest(RemotePingRequest remotePingRequest)
        {
            var msg = (new RemoteMessage { RemotePingResponse = new RemotePingResponse() });
            msg.RemotePingResponse.Val1 = remotePingRequest.Val1;
            SendMessage(msg);
        }

        void ProcessRemoteRemoteSetVolumeLevel(RemoteSetVolumeLevel remoteSetVolumeLevel)
        {
        }

        private void ProcessRemoteImeKeyInject(RemoteImeKeyInject remoteImeKeyInject)
        {
        }

        void ProcessRemoteConfigureMessage(RemoteConfigure remoteConfigure)
        {
            var msg = (new RemoteMessage { RemoteConfigure = new RemoteConfigure() { DeviceInfo = new RemoteDeviceInfo() } });
            msg.RemoteConfigure.Code1 = remoteConfigure.Code1;
            msg.RemoteConfigure.DeviceInfo.Unknown1 = 1;
            msg.RemoteConfigure.DeviceInfo.Unknown2 = "1";
            msg.RemoteConfigure.DeviceInfo.PackageName = "atvremote";
            msg.RemoteConfigure.DeviceInfo.AppVersion = "1.0.0";
            SendMessage(msg);
        }
        void ProcessRemoteSetActiveMessage(RemoteSetActive remoteSetActive)
        {
        }
        private void ProcessRemoteStart(RemoteStart remoteStart)
        {
        }

        private void SendMessage(RemoteMessage message)
        {
            if (_Logging) _Logger!.LogDebug($"Sending message: {message}");
            _Stream!.WriteMessageAsync(message.ToByteArray(), _Cts.Token).Wait();
        }
        #endregion
    }
}
