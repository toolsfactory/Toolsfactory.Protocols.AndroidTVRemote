using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Toolsfactory.Protocols.AndroidTVRemote.Tool
{
    internal partial class Program
    {
        private static Command BuildPairingCommand()
        {
            var host = new Option<string>("--host", "The device (ip or hostname) to pair with") { IsRequired = true };
            var file = new Option<string>("--file", "The to store the configuration in") { IsRequired = true };
            var command = new Command("pair", "Pair a remote control with the Android device") { host, file };
            command.SetHandler(async (hostValue, fileValue) => await HandlePairingCommandAsync(hostValue, fileValue), host, file);
            return command;
        }

        private static async Task HandlePairingCommandAsync(string host, string file)
        {
            using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole().AddFilter(null, LogLevel.Debug).AddConsole());
            ILogger logger = factory.CreateLogger("Program");

            WriteHeadline("Manual Pairing");

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
                Console.Write("Please provide the secret: ");
                var input = Console.ReadLine();
                var status = await tvPairingClient.FinishPairingAsync(input);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[bold red]Pairing failed[/]");
                AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
                return;
            }

            AnsiConsole.MarkupLine("[green]Pairing successful[/]");

            if (!AnsiConsole.Confirm("Continue?"))
            {
                AnsiConsole.MarkupLine("Ok... :(");
                return;
            }
            var name = AnsiConsole.Ask<string>("Device name?");
            var id = AnsiConsole.Ask<string>("Device id?");

            var config = new PairingConfiguration(name, id, host, tvPairingClient.ClientCertificatePEM+Environment.NewLine+tvPairingClient.PrivateKeyPEM);
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(file, json);
            AnsiConsole.MarkupLine($"Certificate saved to [yellow]{file}[/]");
        }

    }
}
