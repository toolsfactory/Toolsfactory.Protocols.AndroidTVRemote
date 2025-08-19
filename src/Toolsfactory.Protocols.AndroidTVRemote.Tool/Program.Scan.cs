using System.CommandLine;
using Spectre.Console;
using Zeroconf;

namespace Toolsfactory.Protocols.AndroidTVRemote.Tool
{
    internal static partial class Program
    {
        private static Command BuildScanCommand()
        {
            var command = new Command("scan", "Scan for Android TV devices");
            command.SetHandler(async () => await HandleScanCommandAsync());
            return command;
        }

        private static async Task HandleScanCommandAsync()
        {
            await FindAndroidTVDevicesAsync();
        }
        
        private static async Task<IReadOnlyList<(string IPAddress, string DisplayName)>> ScanAndroidTVDevicesAsync()
        {
            AnsiConsole.WriteLine("Scanning for Android TV devices...");
            var hosts = await ZeroconfResolver.ResolveAsync("_androidtvremote2._tcp.local.");

            var list = hosts
                .Select(h => (IPAddress: h.IPAddress ?? string.Empty, DisplayName: h.DisplayName ?? string.Empty))
                .ToList();

            return list;
        }
        
        private static async Task FindAndroidTVDevicesAsync()
        {
            var result = await ScanAndroidTVDevicesAsync();
            
            if (result.Count == 0)
                AnsiConsole.MarkupLine("[bold red]No devices found[/]");
            
            foreach (var host in result)
            {
                AnsiConsole.MarkupLine($"[yellow]{host.IPAddress}[/] - [green]{host.DisplayName}[/]");
            }
        }
    }
}
