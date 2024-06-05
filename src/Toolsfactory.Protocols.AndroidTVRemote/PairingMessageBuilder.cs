using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Tls;
using System;
using System.Linq;
using Toolsfactory.Protocols.AndroidTVRemote.ProtoBuf;

namespace Toolsfactory.Protocols.AndroidTVRemote
{
    public class PairingMessageBuilder
    {
        public static OuterMessage BuildPairingMessage(string serviceName, string clientName)
        {
            var msg = new OuterMessage();
            msg.ProtocolVersion = 2;
            msg.Status = OuterMessage.Types.Status.Ok;
            msg.PairingRequest = new PairingRequest();
            msg.PairingRequest.ServiceName = serviceName;
            msg.PairingRequest.ClientName = clientName;
            return msg;
        }

        public static OuterMessage BuildPairingOptionMessage()
        {

           var msg = new OuterMessage();
            msg.ProtocolVersion = 2;
            msg.Status = OuterMessage.Types.Status.Ok;
            msg.Options = new Options();
            msg.Options.PreferredRole = Options.Types.RoleType.Input;
            var enc = new Options.Types.Encoding();
            enc.Type = Options.Types.Encoding.Types.EncodingType.Hexadecimal;
            enc.SymbolLength = 6;
            msg.Options.InputEncodings.Add(enc);
            return msg;
        }

        public static OuterMessage BuildConfigurationMessage()
        {

            var msg = new OuterMessage();
            msg.ProtocolVersion = 2;
            msg.Status = OuterMessage.Types.Status.Ok;
            msg.Configuration = new Configuration();
            msg.Configuration.ClientRole = Options.Types.RoleType.Input;
            var enc = new Options.Types.Encoding();
            enc.Type = Options.Types.Encoding.Types.EncodingType.Hexadecimal;
            enc.SymbolLength = 6;
            msg.Configuration.Encoding = enc;
            return msg;
        }

        public static OuterMessage BuildPairingSecretMessage(byte[] secret)
        {
            var msg = new OuterMessage();
            msg.ProtocolVersion = 2;
            msg.Status = OuterMessage.Types.Status.Ok;
            msg.Secret = new Secret();
            msg.Secret.Secret_ = ByteString.CopyFrom(secret);
            return msg;
        }
    }
}
