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
        private static Command BuildMenuCommand()
        {
            var command = new Command("menu", "Send a keyevents");
            command.SetHandler(async (configvalue) => await HandleMenuCommandAsync());
            return command;
        }

        private static async Task HandleMenuCommandAsync()
        {
            PairingConfiguration? pairingConfig;
            WriteHeadline("Menu Mode");

            var choices = new List<string> {"Interactive pairing", "Interact with device", "Show help", "Exit"};
            var device = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What do you want to do?")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more devices)[/]")
                    .AddChoices(choices));

            var idx = choices.IndexOf(device);
            if (idx == -1)
                AnsiConsole.MarkupLine("[red]Invalid item choice.[/]");
            switch (idx)
            {
                case 0: await HandleInteractivePairingCommandAsync(); break;
                case 1: await Interact(); break;
                default: break;
            }

            async Task Interact()
            {
                var file = AnsiConsole.Ask<string>("File to save to:", "device.apair");
                await HandleInteractiveCommandAsync(file);
            }
        }

    }
}
