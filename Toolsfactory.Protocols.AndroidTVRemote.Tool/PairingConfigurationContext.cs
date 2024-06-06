using System.Text.Json.Serialization;

namespace Toolsfactory.Protocols.AndroidTVRemote.Tool
{
    [JsonSerializable(typeof(PairingConfiguration))]
    public partial class PairingConfigurationContext : JsonSerializerContext
    {
    }
}