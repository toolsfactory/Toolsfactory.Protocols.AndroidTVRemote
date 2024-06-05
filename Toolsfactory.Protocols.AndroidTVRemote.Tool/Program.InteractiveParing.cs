using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Zeroconf;

namespace Toolsfactory.Protocols.AndroidTVRemote.Tool
{
    internal partial class Program
    {
        private static Command BuildInteractivePairingCommand()
        {
            var command = new Command("interactivepairing", "Scan for Android TV devices");
            command.SetHandler(async () => await HandleInteractivePairingCommandAsync());
            return command;
        }

        private static async Task HandleInteractivePairingCommandAsync()
        {
            WriteHeadline("Interactive Pairing");
            AnsiConsole.WriteLine("Scanning for Android TV devices...");
            var result = await ZeroconfResolver.ResolveAsync("_androidtvremote2._tcp.local.");
            Console.WriteLine();
            if (result.Count == 0)
            {
                AnsiConsole.MarkupLine("[bold red]No devices found[/]");
                return;
            }

            var choices = new List<string>();
            foreach (var item in result)
            {
                var itemDesc = (item.IPAddress + "      ").Substring(0, 15) + " - " + item.DisplayName;
                choices.Add(itemDesc);
            }
            var device = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What device do you want to pair with?")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more devices)[/]")
                    .AddChoices(choices));

            var host = device.Substring(0,15).Trim();
            AnsiConsole.MarkupLine($"Pairing with [yellow]{host}[/]");
            var pairingOptions = new PairingClientOptions(host, null, LoggerFactory: factory);
            var tvPairingClient = new PairingClient(pairingOptions);
            try
            {
                await tvPairingClient.PreparePairingAsync();
                var ok = await tvPairingClient.StartPairingAsync ();
                if (!ok)
                {
                    AnsiConsole.MarkupLine("[bold red]Pairing Init failed[/]");
                    return;
                }
                Console.WriteLine("Pairing initiated.");
                Console.WriteLine();
                var input = AnsiConsole.Ask<string>("Please provide the secret: ");
                Console.WriteLine();
                var status = await tvPairingClient.FinishPairingAsync(input);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[bold red]Pairing failed[/]");
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                return;
            }

            AnsiConsole.MarkupLine("[green]Pairing successful[/]");
            Console.WriteLine();

            if (!AnsiConsole.Confirm("Save to file?"))
            {
                AnsiConsole.MarkupLine("Ok... :(");
                return;
            }
            var name = AnsiConsole.Ask<string>("Device name:");
            var id = AnsiConsole.Ask<string>("Device id:");
            var file = AnsiConsole.Ask<string>("File to save to:", "device.apair");
            Console.WriteLine();

            var config = new PairingConfiguration(name, id, host, tvPairingClient.ClientCertificatePEM+Environment.NewLine+tvPairingClient.PrivateKeyPEM);
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(file, json);
            var fullPath = Path.GetFullPath(file);
            AnsiConsole.MarkupLine($"Certificate saved to [yellow]{fullPath}[/]");
        }
    }
}
