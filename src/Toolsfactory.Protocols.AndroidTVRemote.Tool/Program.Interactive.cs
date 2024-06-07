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
        private static Command BuildInteractiveCommand()
        {
            var config = new Option<string>("--config", "The pairing config to be used") { IsRequired = true };
            var command = new Command("interactive", "Send a keyevents") { config };
            command.SetHandler(async (configvalue) => await HandleInteractiveCommandAsync(configvalue), config);
            return command;
        }

        private static async Task HandleInteractiveCommandAsync(string config)
        {
            PairingConfiguration? pairingConfig;
            WriteHeadline("Interactive Mode");

            try
            {
                var text = await System.IO.File.ReadAllTextAsync(config);
                pairingConfig = JsonSerializer.Deserialize<PairingConfiguration>(text, PairingConfigurationContext.Default.PairingConfiguration);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[bold red]Error reading configuration file[/]");
                return;
            }
            AnsiConsole.MarkupLine($"Connecting with [yellow]{pairingConfig!.DeviceName} - {pairingConfig!.DeviceHostName}[/]");

            var rcOptions = new RemoteControlClientOptions(pairingConfig!.DeviceHostName, CertificateBuilder.LoadCertificateFromPEM(pairingConfig.Certificate), LoggerFactory: factory);
            var rcClient = new RemoteControlClient(rcOptions);

            await rcClient.ConnectAsync();

            AnsiConsole.WriteLine("Press up, down, left, right, enter, backspace, del, home, space to navigate.");
            AnsiConsole.WriteLine("Press ESC to stop interactive mode.");
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine("Waiting for you input...");
            AnsiConsole.WriteLine();
            ConsoleKeyInfo cki;
            do
            {
                cki = Console.ReadKey();
                switch (cki.Key)
                {
                    case ConsoleKey.UpArrow: SendKey(RCKeyCode.Key_DPAD_UP); break;
                    case ConsoleKey.DownArrow: SendKey(RCKeyCode.Key_DPAD_DOWN); break;
                    case ConsoleKey.LeftArrow: SendKey(RCKeyCode.Key_DPAD_LEFT); break;
                    case ConsoleKey.RightArrow: SendKey(RCKeyCode.Key_DPAD_RIGHT); break;
                    case ConsoleKey.Enter: SendKey(RCKeyCode.Key_DPAD_CENTER); break;
                    case ConsoleKey.Backspace: SendKey(RCKeyCode.Key_BACK); break;
                    case ConsoleKey.Spacebar: SendKey(RCKeyCode.Key_MEDIA_PLAY_PAUSE); break;
                    case ConsoleKey.Home: SendKey(RCKeyCode.Key_HOME); break;
                    case ConsoleKey.Delete: SendKey(RCKeyCode.Key_POWER); break;
//                    case ConsoleKey.OemPlus: SendKey(RCKeyCode.Key_VOLUME_UP); break;
//                    case ConsoleKey.OemMinus: SendKey(RCKeyCode.Key_VOLUME_DOWN); break;
                    case ConsoleKey.D1: SendKey(RCKeyCode.Key_1); break;
                    case ConsoleKey.D2: SendKey(RCKeyCode.Key_2); break;
                    case ConsoleKey.D3: SendKey(RCKeyCode.Key_3); break;
                    case ConsoleKey.D4: SendKey(RCKeyCode.Key_4); break;
                    case ConsoleKey.D5: SendKey(RCKeyCode.Key_5); break;
                    case ConsoleKey.D6: SendKey(RCKeyCode.Key_6); break;
                    case ConsoleKey.D7: SendKey(RCKeyCode.Key_7); break;
                    case ConsoleKey.D8: SendKey(RCKeyCode.Key_8); break;
                    case ConsoleKey.D9: SendKey(RCKeyCode.Key_9); break;
                    case ConsoleKey.D0: SendKey(RCKeyCode.Key_0); break;
                    case ConsoleKey.N: SendAppLaunch("com.netflix.ninja"); break;
                    default: break;
                }
            } while (cki.Key != ConsoleKey.Escape);

            async void SendKey(RCKeyCode key)
            {
                AnsiConsole.MarkupLine($"Sending key [yellow]{key}[/]");
                await rcClient.PressKeyAsync(key);
            }

            async void SendAppLaunch(string app)
            {
                AnsiConsole.MarkupLine($"Sending App Launch [yellow]{app}[/]");
                await rcClient.SendLaunchAppAsync(app);
            }
        }

    }
}
