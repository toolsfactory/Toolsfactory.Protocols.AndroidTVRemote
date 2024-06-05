using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Zeroconf;

namespace Toolsfactory.Protocols.AndroidTVRemote.Tool
{
    internal partial class Program
    {
        private static ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddDebug().AddFilter(null, LogLevel.Debug));
        static async Task Main(string[] args)
        {
            var logger = factory.CreateLogger("Program");

            RootCommand rootCommand = BuildCommandLine();
            // await rootCommand.InvokeAsync("interactivepairing");
            // await rootCommand.InvokeAsync("pair --host 172.16.14.142 --file c://temp//test.apair");
            // await rootCommand.InvokeAsync("scan");
            await rootCommand.InvokeAsync("interactive  --config wohnzimmer.apair ");
            // await rootCommand.InvokeAsync("sendkey DPAD_DOWN --config c://temp//test.apair ");
        }

        #region Build Command Line arguments
        private static RootCommand BuildCommandLine()
        {
            var rootCommand = new RootCommand("AndroidTV Command Line Tool");
            rootCommand.Add(BuildPairingCommand());
            rootCommand.Add(BuildSendKeyCommand());
            rootCommand.Add(BuildInteractiveCommand());
            rootCommand.Add(BuildInteractivePairingCommand());
            rootCommand.Add(BuildScanCommand());
            return rootCommand;
        }
        #endregion

        #region helpers
        private static async Task FindAndroidTVDevicesAsync()
        {
            var result = await ZeroconfResolver.ResolveAsync("_androidtvremote2._tcp.local.");
            if (result.Count == 0)
            {
                AnsiConsole.MarkupLine("[bold red]No devices found[/]");
                return;
            }
            foreach (var host in result)
            {
                var ip = (host.IPAddress + "      ").Substring(0, 15);
                AnsiConsole.MarkupLine($"[yellow]{ip}[/] - [green]{host.DisplayName}[/]");
            }
        }

        private static void WriteHeadline(string text)
        {
            Console.Clear();
            var rule = new Rule($"[bold red]{text}[/]");
            AnsiConsole.Write(rule);
            Console.WriteLine();
        }
        #endregion
    }
}