using Spectre.Console;
using System.CommandLine;

namespace Toolsfactory.Protocols.AndroidTVRemote.Tool
{
    internal static partial class Program
    {
        private static Command BuildMenuCommand()
        {
            var command = new Command("menu", "Open the interactive menu");
            command.SetHandler(async () => await HandleMenuCommandAsync());
            return command;
        }

        private static async Task HandleMenuCommandAsync()
        {
            bool exitSelected = false;

            while (!exitSelected)
            {
                AnsiConsole.Clear();
                WriteHeadline("Menu Mode");

                var choices = new List<string> { "Interactive pairing", "Pair with device", "Interact with device", "Show help", "Exit" };
                var device = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("What do you want to do?")
                        .PageSize(10)
                        .MoreChoicesText("[grey](Move up and down to reveal more devices)[/]")
                        .AddChoices(choices));

                var idx = choices.IndexOf(device);
                switch (idx)
                {
                    case 0: await HandleInteractivePairingCommandAsync(); break;
                    case 1: await PairWithDeviceFromMenuAsync(); break;
                    case 2: await Interact(); break;
                    case 3: await ShowHelpAsync(); break;
                    case 4: exitSelected = true; break;
                    default: AnsiConsole.MarkupLine("[red]Invalid item choice.[/]"); break;
                }
            }
        }
        
        private static async Task PairWithDeviceFromMenuAsync()
        {
            WriteHeadline("Pair with device");
            var host = AnsiConsole.Ask<string>("Enter the device [yellow]IP or hostname[/]:");
            await RunPairingScriptsAsync(host);
        }
        
        private static Task ShowHelpAsync()
        {
            WriteHeadline("Help");

            AnsiConsole.MarkupLine("[bold]How to use this tool[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("- [yellow]Interactive pairing:[/] Discover an Android TV device and create an .apair file with embedded TLS credentials for streamlined future connections.");
            AnsiConsole.MarkupLine("- [yellow]Pair with device:[/] Manually enter an IP/hostname and create an .apair file (no discovery step).");
            AnsiConsole.MarkupLine("- [yellow]Interact with device:[/] Use an existing .apair file to connect and send key events (navigation, media controls, app launches).");
            
            PauseReturnToMenu();
            
            return Task.CompletedTask;
        }
        
        private static void PauseReturnToMenu(string? message = null)
        {
            if (!string.IsNullOrWhiteSpace(message))
                AnsiConsole.MarkupLine(message);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press any key to return to the main menu[/]");
            Console.ReadKey(intercept: true);
        }
    }
}
