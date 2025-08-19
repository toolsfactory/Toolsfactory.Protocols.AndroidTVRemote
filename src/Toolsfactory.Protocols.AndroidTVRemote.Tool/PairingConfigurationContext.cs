using System.Text.Json.Serialization;

namespace Toolsfactory.Protocols.AndroidTVRemote.Tool
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(PairingConfiguration))]
    internal partial class PairingConfigurationContext : JsonSerializerContext;
}
