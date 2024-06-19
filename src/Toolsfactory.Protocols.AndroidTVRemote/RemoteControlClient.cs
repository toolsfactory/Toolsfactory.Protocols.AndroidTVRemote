using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Toolsfactory.Protocols.AndroidTVRemote.Events;
using Toolsfactory.Protocols.AndroidTVRemote.ProtoBuf;

namespace Toolsfactory.Protocols.AndroidTVRemote
{
    public class RemoteControlClient : ClientBase
    {
        public const UInt16 DefaultPort = 6466;

        #region Properties
        public bool Activated { get; private set; }
        public RemoteFeatures SupportedFeatures { get; private set; }
        public DeviceInformation DeviceInformation { get; init; } = new("Toolsfactory", "DotNet", "AndroidTVRemote", "1.0.0", "1.0");
        public DeviceInformation? RemoteDeviceInformation { get; set; }

        #endregion

        #region Events
        public event EventHandler<Events.AppLaunchedEventArgs>? AppLaunched;
        protected virtual void OnAppLaunched(AppLaunchedEventArgs e) => AppLaunched?.Invoke(this, e);

        public event EventHandler<Events.ActivatedChangedEventArgs>? ConnectionActivated;
        protected virtual void OnActivatedChanged(ActivatedChangedEventArgs e) => ConnectionActivated?.Invoke(this, e);

        public event EventHandler<Events.PingRequestedEventArgs>? PingRequested;
        protected virtual void OnPingRequested(PingRequestedEventArgs e) => PingRequested?.Invoke(this, e);

        public event EventHandler<Events.VolumeChangedEventArgs>? VolumeChanged;
        protected virtual void OnVolumeChanged(VolumeChangedEventArgs e) => VolumeChanged?.Invoke(this, e);

        public event EventHandler<Events.StartedChangedEventArgs>? StartStarted;
        protected virtual void OnStartStarted(StartedChangedEventArgs e) => StartStarted?.Invoke(this, e);

        public event EventHandler RemoteConfigurationChanged;
        protected virtual void OnRemoteConfigurationChanged() => RemoteConfigurationChanged?.Invoke(this, new EventArgs());

        #endregion

        #region Constructors
        public RemoteControlClient(string serverAddress, X509Certificate2 clientCertificate)
            : this(new RemoteControlClientOptions(serverAddress, clientCertificate)) { }

        public RemoteControlClient(RemoteControlClientOptions options)
            : base(options.ServerAddress, options.ClientCertificate, options.LoggerFactory)
        {
            Port = options.Port;
            Protocol = options.Protocol;
            _Logger = options.LoggerFactory?.CreateLogger<RemoteControlClient>();
        }
        #endregion

        #region Connection management
        public async Task ConnectAsync() => await InitiateConnectionAsync();

        public override void Close()
        {
            Activated = false;
            SupportedFeatures = (RemoteFeatures) 0;
            base.Close();
        }
        #endregion

        #region Send commands
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
        #endregion


        #region Message Processing
        protected override void ProcessMessage(byte[] data)
        {
            var message = RemoteMessage.Parser.ParseFrom(data);
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
                Debug.WriteLine($"Unknown message type: {message}");
        }

        private void ProcessRemotePingRequest(RemotePingRequest remotePingRequest)
        {
            var msg = (new RemoteMessage { RemotePingResponse = new RemotePingResponse() });
            msg.RemotePingResponse.Val1 = remotePingRequest.Val1;
            SendMessage(msg);
            OnPingRequested(new(remotePingRequest.Val1));
        }

        void ProcessRemoteRemoteSetVolumeLevel(RemoteSetVolumeLevel remoteSetVolumeLevel)
        {
            OnVolumeChanged(new(remoteSetVolumeLevel.VolumeLevel, remoteSetVolumeLevel.VolumeMax, remoteSetVolumeLevel.VolumeMuted));
        }

        private void ProcessRemoteImeKeyInject(RemoteImeKeyInject remoteImeKeyInject)
        {
            var appInfo =remoteImeKeyInject.AppInfo;
            OnAppLaunched(new(remoteImeKeyInject.AppInfo?.AppPackage ?? ""));
        }

        void ProcessRemoteConfigureMessage(RemoteConfigure remoteConfigure)
        {
            var msg = (new RemoteMessage { RemoteConfigure = new RemoteConfigure() { DeviceInfo = new RemoteDeviceInfo() } });
            msg.RemoteConfigure.Code1 = remoteConfigure.Code1;
            msg.RemoteConfigure.DeviceInfo.Unknown1 = 1;
            msg.RemoteConfigure.DeviceInfo.Version = DeviceInformation.Version;
            msg.RemoteConfigure.DeviceInfo.PackageName = DeviceInformation.PackageName;
            msg.RemoteConfigure.DeviceInfo.AppVersion = DeviceInformation.AppVersion;
            msg.RemoteConfigure.DeviceInfo.Model = DeviceInformation.Model;
            msg.RemoteConfigure.DeviceInfo.Vendor = DeviceInformation.Vendor;
            SendMessage(msg);
            RemoteDeviceInformation = new(remoteConfigure.DeviceInfo.Vendor, 
                remoteConfigure.DeviceInfo.Model, 
                remoteConfigure.DeviceInfo.PackageName, 
                remoteConfigure.DeviceInfo.AppVersion,
                remoteConfigure.DeviceInfo.Version);
            OnRemoteConfigurationChanged();
        }

        void ProcessRemoteSetActiveMessage(RemoteSetActive remoteSetActive)
        {
            Activated = true;
            SupportedFeatures = (RemoteFeatures) remoteSetActive.Active;
            OnActivatedChanged(new(SupportedFeatures));
        }

        private void ProcessRemoteStart(RemoteStart remoteStart)
        {
            OnStartStarted(new(remoteStart.Started));
        }

        private void SendMessage(RemoteMessage message)
        {
            if (_Logging) _Logger!.LogDebug($"Sending message: {message}");
            _Stream!.WriteMessageAsync(message.ToByteArray(), _Cts.Token).Wait();
        }
        #endregion
    }

    public record DeviceInformation(string Vendor, string Model, string PackageName, string AppVersion, string Version );
}
