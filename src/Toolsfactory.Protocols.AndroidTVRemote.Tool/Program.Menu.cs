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
            /*
            var device = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What device do you want to pair with?")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more devices)[/]")
                    .AddChoices(choices));
            */
        }

    }
}
