using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Toolsfactory.Protocols.AndroidTVRemote.Events;
using Toolsfactory.Protocols.AndroidTVRemote.Extensions;
using Toolsfactory.Protocols.AndroidTVRemote.ProtoBuf;

namespace Toolsfactory.Protocols.AndroidTVRemote
{
    public class RemoteControlClient : ClientBase
    {
        #region Properties
        public const UInt16 DefaultPort = 6466;
        private RemoteFeatures SupportedFeatures { get; set; }
        private DeviceInformation DeviceInformation { get; init; } = new("Toolsfactory", "DotNet", "AndroidTVRemote", "1.0.0", "1.0");
        public DeviceInformation? RemoteDeviceInformation { get; private set; }
        #endregion

        #region Events
        public event EventHandler<AppLaunchedEventArgs>? AppLaunched;
        protected virtual void OnAppLaunched(AppLaunchedEventArgs e) => AppLaunched?.Invoke(this, e);

        public event EventHandler<ActivatedChangedEventArgs>? ConnectionActivated;
        protected virtual void OnActivatedChanged(ActivatedChangedEventArgs e) => ConnectionActivated?.Invoke(this, e);

        public event EventHandler<PingRequestedEventArgs>? PingRequested;
        protected virtual void OnPingRequested(PingRequestedEventArgs e) => PingRequested?.Invoke(this, e);

        public event EventHandler<VolumeChangedEventArgs>? VolumeChanged;
        protected virtual void OnVolumeChanged(VolumeChangedEventArgs e) => VolumeChanged?.Invoke(this, e);

        public event EventHandler<StartedChangedEventArgs>? StartStarted;
        protected virtual void OnStartStarted(StartedChangedEventArgs e) => StartStarted?.Invoke(this, e);

        public event EventHandler? RemoteConfigurationChanged;
        protected virtual void OnRemoteConfigurationChanged() => RemoteConfigurationChanged?.Invoke(this, EventArgs.Empty);
        #endregion

        #region Constructors
        public RemoteControlClient(string serverAddress, X509Certificate2 clientCertificate)
            : this(new RemoteControlClientOptions(serverAddress, clientCertificate)) { }

        public RemoteControlClient(RemoteControlClientOptions options)
            : base(options.ServerAddress, options.ClientCertificate, options.LoggerFactory)
        {
            Port = options.Port;
            Protocol = options.Protocol;
            Logger = options.LoggerFactory?.CreateLogger<RemoteControlClient>();
            ValidateServerCertificate = options.ValidateServerCertificate;
            PinnedServerCertificateThumbprint = options.PinnedServerCertificateThumbprint;
        }
        #endregion

        #region Connection management
        public async Task ConnectAsync() => await InitiateConnectionAsync();

        protected override void Close()
        {
            SupportedFeatures = 0;
            base.Close();
        }
        #endregion

        #region Send commands
        public async Task PressKeyAsync(RCKeyCode key, KeyEventType keyType = KeyEventType.Press)
        {
            if (!Connected)
                return;

            var msg = new RemoteMessage { 
                RemoteKeyInject = new RemoteKeyInject
                {
                    KeyCode = (RemoteKeyCode) key,
                    Direction = (RemoteDirection) keyType
                }
            };
            await Stream!.WriteProtoBufMessageAsync(msg, Cts!.Token);
        }

        public async Task SendLaunchAppAsync(string appLink)
        {
            if (!Connected)
                return;

            var msg = new RemoteMessage { 
                RemoteAppLinkLaunchRequest = new RemoteAppLinkLaunchRequest
                {
                    AppLink = appLink
                }
            };
            await Stream!.WriteProtoBufMessageAsync(msg, Cts!.Token);
        }
        #endregion

        #region Message Processing
        protected override void ProcessMessage(byte[] data)
        {
            var message = RemoteMessage.Parser.ParseFrom(data);
            if (Logging) Logger!.LogDebug($"Received message: {message}");

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
            var msg = (new RemoteMessage { 
                RemotePingResponse = new RemotePingResponse
                {
                    Val1 = remotePingRequest.Val1
                }
            });
            _ = SendMessageAsync(msg);
            OnPingRequested(new PingRequestedEventArgs(remotePingRequest.Val1));
        }

        void ProcessRemoteRemoteSetVolumeLevel(RemoteSetVolumeLevel remoteSetVolumeLevel)
        {
            OnVolumeChanged(new VolumeChangedEventArgs(remoteSetVolumeLevel.VolumeLevel, remoteSetVolumeLevel.VolumeMax, remoteSetVolumeLevel.VolumeMuted));
        }

        private void ProcessRemoteImeKeyInject(RemoteImeKeyInject remoteImeKeyInject)
        {
            OnAppLaunched(new AppLaunchedEventArgs(remoteImeKeyInject.AppInfo?.AppPackage ?? ""));
        }

        void ProcessRemoteConfigureMessage(RemoteConfigure remoteConfigure)
        {
            var msg = (new RemoteMessage { 
                RemoteConfigure = new RemoteConfigure
                {
                    DeviceInfo = new RemoteDeviceInfo(),
                    Code1 = remoteConfigure.Code1
                }
            });
            msg.RemoteConfigure.DeviceInfo.Unknown1 = 1;
            msg.RemoteConfigure.DeviceInfo.Version = DeviceInformation.Version;
            msg.RemoteConfigure.DeviceInfo.PackageName = DeviceInformation.PackageName;
            msg.RemoteConfigure.DeviceInfo.AppVersion = DeviceInformation.AppVersion;
            msg.RemoteConfigure.DeviceInfo.Model = DeviceInformation.Model;
            msg.RemoteConfigure.DeviceInfo.Vendor = DeviceInformation.Vendor;
            
            _ = SendMessageAsync(msg);
            
            RemoteDeviceInformation = new DeviceInformation(remoteConfigure.DeviceInfo.Vendor, 
                remoteConfigure.DeviceInfo.Model, 
                remoteConfigure.DeviceInfo.PackageName, 
                remoteConfigure.DeviceInfo.AppVersion,
                remoteConfigure.DeviceInfo.Version);
            
            OnRemoteConfigurationChanged();
        }

        void ProcessRemoteSetActiveMessage(RemoteSetActive remoteSetActive)
        {
            SupportedFeatures = (RemoteFeatures) remoteSetActive.Active;
            OnActivatedChanged(new ActivatedChangedEventArgs(SupportedFeatures));
        }

        private void ProcessRemoteStart(RemoteStart remoteStart)
        {
            OnStartStarted(new StartedChangedEventArgs(remoteStart.Started));
        }

        private async Task SendMessageAsync(RemoteMessage message)
        {
            if (Logging) Logger!.LogDebug($"Sending message: {message}");
            await Stream!.WriteMessageAsync(message.ToByteArray(), Cts!.Token).ConfigureAwait(false);
        }
        #endregion
    }

    public record DeviceInformation(string Vendor, string Model, string PackageName, string AppVersion, string Version );
}
