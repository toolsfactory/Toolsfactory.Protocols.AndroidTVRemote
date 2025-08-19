using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Parsing;

namespace Toolsfactory.Protocols.AndroidTVRemote.Tool
{
    internal static partial class Program
    {
        private static readonly ILoggerFactory Factory = LoggerFactory.Create(builder => builder.AddDebug().AddFilter(null, LogLevel.Debug));
        private const string AppDisplayName = "Toolsfactory Android TV Remote";
        static async Task Main(string[] args)
        {
            // ReSharper disable once UnusedVariable
            var logger = Factory.CreateLogger("Program");
            var parser = CreateCommandLineParser();
            await parser.InvokeAsync(args);
        }

        #region Build Command Line Arguments
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
                            .Prepend(_ => AnsiConsole.Write(new FigletText($"{AppDisplayName}"))
                    ));
            })
            .Build();
        }
        private static RootCommand BuildRootCommand()
        {
            var root = new RootCommand($"{AppDisplayName}\n\n" +
                                       "Commands:\n" +
                                       "  menu                Open the interactive menu\n" +
                                       "  scan                Discover Android TV devices on the network\n" +
                                       "  pair                Pair a remote with a device (.apair)\n" +
                                       "  interact            Control a paired device (interactive)\n" +
                                       "  help                Show detailed help\n");
            root.SetHandler(HandleMenuCommandAsync);
            root.Add(BuildMenuCommand());
            root.Add(BuildPairingCommand());
            root.Add(BuildInteractiveCommand());
            root.Add(BuildInteractivePairingCommand());
            root.Add(BuildScanCommand());
            return root;
        }
        #endregion
        
        #region Helpers
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
