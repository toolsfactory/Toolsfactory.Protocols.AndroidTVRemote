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

            var choices = new List<string>() {"Interactive pairing", "Interact with device", "Exit"};
            var device = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What do you want to do?")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more devices)[/]")
                    .AddChoices(choices));
            var item = choices.FindIndex(x => x == device);
            switch (item)
            {
                case 0: await HandleInteractivePairingCommandAsync(); break;
                case 1: await HandleInteractiveAsync(); break;
                default: break;
            }
        }
        static async Task HandleInteractiveAsync()
        {
            var exists = false;
            Console.WriteLine();
            do
            {
                var file = AnsiConsole.Ask<string>("Config file to use:");
                exists = System.IO.File.Exists(file);
                await HandleInteractiveCommandAsync(file);
            } while (!exists);
        }
    }
}
