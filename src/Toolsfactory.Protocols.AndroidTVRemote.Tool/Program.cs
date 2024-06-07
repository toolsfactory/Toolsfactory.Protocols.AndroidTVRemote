using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Zeroconf;

namespace Toolsfactory.Protocols.AndroidTVRemote.Tool
{
    internal partial class Program
    {
        private static ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddDebug().AddFilter(null, LogLevel.Debug));
        static async Task Main(string[] args)
        {
            var logger = factory.CreateLogger("Program");
            var parser = CreateCommandLineParser();
            await parser.InvokeAsync(args);

            // await parser.InvokeAsync("--help");
            // await parser.InvokeAsync("interactivepairing");
            // await parser.InvokeAsync("pair --host 172.16.14.142 --file c://temp//test.apair");
            // await parser.InvokeAsync("scan");
            // await parser.InvokeAsync("interactive  --config teststicklab.apair ");
            // await parser.InvokeAsync("sendkey DPAD_DOWN --config c://temp//test.apair ");
        }

        #region Build Command Line arguments
        private static Parser CreateCommandLineParser()
        {
            RootCommand rootCommand = BuildRootCommand();
            return new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .UseHelp(ctx =>
            {
                ctx.HelpBuilder.CustomizeLayout(
                    _ =>
                        HelpBuilder.Default
                            .GetLayout()
                            .Prepend(_ => AnsiConsole.Write(new FigletText("Michael's AndroidTV Tool"))
                    ));
            })
            .Build();
        }
        private static RootCommand BuildRootCommand()
        {
            var rootCommand = new RootCommand();
            rootCommand.SetHandler(HandleMenuCommandAsync);
            rootCommand.Add(BuildMenuCommand());
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