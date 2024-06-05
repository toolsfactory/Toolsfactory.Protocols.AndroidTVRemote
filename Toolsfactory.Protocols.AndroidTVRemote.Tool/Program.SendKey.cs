using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolsfactory.Protocols.AndroidTVRemote.Tool
{
    internal partial class Program
    {
        private static Command BuildSendKeyCommand()
        {
            var config = new Option<string>("--config", "The pairong config to be used") { IsRequired = true };
            var keyname = new Argument<string>(name: "key", description: "The key to send", getDefaultValue: () => "ok");
            var command = new Command("sendkey", "Send a single keyevent") { keyname, config };
            command.SetHandler(async (keynamevalue, configvalue) => await HandleSendKeyCommandAsync(configvalue, keynamevalue), keyname, config);
            return command;
        }

        private static async Task HandleSendKeyCommandAsync(string config, string keyname)
        {
            /*
            PairingConfiguration? pairingConfig;
            try
            {
                pairingConfig = JsonSerializer.Deserialize<PairingConfiguration>(await File.ReadAllTextAsync(config));
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[bold red]Failed to read pairing config[/]");
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                return;
            }

            KeyCode keyCode;
            try
            {
                keyCode = Enum.Parse<KeyCode>("Key_" + keyname.ToUpperInvariant(), true);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[bold red]Failed to parse keyname[/]");
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                return;
            }

            AnsiConsole.MarkupLine($"Sending key [yellow]{keyname}[/] to [yellow]{pairingConfig.DeviceHostName}[/]");
            var client = new RCClient(pairingConfig.DeviceHostName, pairingConfig.Certificate);

            await client.PressKeyAsync(KeyEventType.Press, keyCode);

            var conf = client.GetConfiguration();
            Console.WriteLine($"Configuration: {conf}");
            */
        }
    }
}
