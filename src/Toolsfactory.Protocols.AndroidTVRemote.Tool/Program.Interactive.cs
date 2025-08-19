using Spectre.Console;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Toolsfactory.Protocols.AndroidTVRemote.Tool
{
    internal static partial class Program
    {
        private static Command BuildInteractiveCommand()
        {
            var configOption = new Option<string>("--config", "The pairing config to be used") { IsRequired = true };
            var command = new Command("interactive", "Send a key event") { configOption };
            command.SetHandler(async (configValue) => await HandleInteractiveCommandAsync(configValue), configOption);
            return command;
        }
        
        [SuppressMessage("ReSharper", "SeparateLocalFunctionsWithJumpStatement")]
        private static async Task HandleInteractiveCommandAsync(string config)
        {
            // This method is now reserved for direct CLI use; delegate to Interact
            // when running via the `interactive` command without the menu
            await Interact(config);
        }

        private static async Task Interact(string? configPath = null)
        {
            while (true)
            {
                WriteHeadline("Interactive Mode");
                AnsiConsole.MarkupLine("[yellow]If you don't have any pairing files created, first use the pairing " +
                                       "features from the menu to create one.[/]");
                AnsiConsole.WriteLine();
                
                string file;
                if (string.IsNullOrWhiteSpace(configPath))
                {
                    var filePrompt = new TextPrompt<string>("Pairing file to be loaded").DefaultValue("device.apair");
                    file = AnsiConsole.Prompt(filePrompt);
                    AnsiConsole.WriteLine();
                }
                else
                {
                    file = configPath;
                    AnsiConsole.MarkupLine($"Using pairing file from [yellow]--config[/]: {file}");
                    AnsiConsole.WriteLine();
                }


                try
                {
                    var text = await File.ReadAllTextAsync(file);
                    var pairingConfig = JsonSerializer.Deserialize(
                        text,
                        PairingConfigurationContext.Default.PairingConfiguration
                    ) ?? throw new Exception("Invalid or missing configuration.");

                    await RunInteractiveLoopAsync(pairingConfig);
                    break;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error reading configuration file: {ex.Message}[/]");
                    PauseReturnToMenu();
                    break;
                }
            }
        }

        private static async Task RunInteractiveLoopAsync(PairingConfiguration pairingConfig)
        {
            WriteHeadline("Interactive Mode");
            AnsiConsole.MarkupLine($"Connecting with [yellow]{pairingConfig.DeviceName} - {pairingConfig.DeviceHostName}[/]");
            AnsiConsole.WriteLine();

            var rcOptions = new RemoteControlClientOptions(
                pairingConfig.DeviceHostName,
                CertificateBuilder.LoadCertificateFromPEM(pairingConfig.Certificate),
                LoggerFactory: Factory
            );
            var rcClient = new RemoteControlClient(rcOptions);

            var configReceived = new TaskCompletionSource<bool>();
            rcClient.RemoteConfigurationChanged += (s, e) =>
            {
                RcClient_RemoteConfigurationChanged(s, e);
                configReceived.TrySetResult(true);
            };

            await rcClient.ConnectAsync();
            await configReceived.Task;
            
            AnsiConsole.MarkupLine("[yellow]Press arrow keys, Enter, Backspace, or Home to navigate.[/]");
            AnsiConsole.MarkupLine("[yellow]Press H to see a full list of available commands.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press ESC at any time to return to the main menu[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine("Waiting for your input.");
            AnsiConsole.WriteLine();
            
            ConsoleKeyInfo cki;
            do
            {
                cki = Console.ReadKey(true);

                switch (cki.Key)
                {
                    // arrows
                    case ConsoleKey.UpArrow:    await SendKeyAsync(rcClient, RCKeyCode.Key_DPAD_UP);    break;
                    case ConsoleKey.DownArrow:  await SendKeyAsync(rcClient, RCKeyCode.Key_DPAD_DOWN);  break;
                    case ConsoleKey.LeftArrow:  await SendKeyAsync(rcClient, RCKeyCode.Key_DPAD_LEFT);  break;
                    case ConsoleKey.RightArrow: await SendKeyAsync(rcClient, RCKeyCode.Key_DPAD_RIGHT); break;

                    // basic controls
                    case ConsoleKey.Enter:      await SendKeyAsync(rcClient, RCKeyCode.Key_DPAD_CENTER); break;
                    case ConsoleKey.Backspace:  await SendKeyAsync(rcClient, RCKeyCode.Key_BACK);        break;
                    case ConsoleKey.Delete:     await SendKeyAsync(rcClient, RCKeyCode.Key_POWER);       break;
                    case ConsoleKey.Home:       await SendKeyAsync(rcClient, RCKeyCode.Key_HOME);        break;

                    // media
                    case ConsoleKey.Spacebar: await SendKeyAsync(rcClient, RCKeyCode.Key_MEDIA_PLAY_PAUSE);   break;
                    case ConsoleKey.R:        await SendKeyAsync(rcClient, RCKeyCode.Key_MEDIA_REWIND);       break;
                    case ConsoleKey.F:        await SendKeyAsync(rcClient, RCKeyCode.Key_MEDIA_FAST_FORWARD); break;
                    case ConsoleKey.Q:        await SendKeyAsync(rcClient, RCKeyCode.Key_MEDIA_PREVIOUS);     break;
                    case ConsoleKey.C:        await SendKeyAsync(rcClient, RCKeyCode.Key_MEDIA_RECORD);       break;
                    case ConsoleKey.Tab:      await SendKeyAsync(rcClient, RCKeyCode.Key_MEDIA_NEXT);         break;

                    // numbers
                    case ConsoleKey.D0: await SendKeyAsync(rcClient, RCKeyCode.Key_0); break;
                    case ConsoleKey.D1: await SendKeyAsync(rcClient, RCKeyCode.Key_1); break;
                    case ConsoleKey.D2: await SendKeyAsync(rcClient, RCKeyCode.Key_2); break;
                    case ConsoleKey.D3: await SendKeyAsync(rcClient, RCKeyCode.Key_3); break;
                    case ConsoleKey.D4: await SendKeyAsync(rcClient, RCKeyCode.Key_4); break;
                    case ConsoleKey.D5: await SendKeyAsync(rcClient, RCKeyCode.Key_5); break;
                    case ConsoleKey.D6: await SendKeyAsync(rcClient, RCKeyCode.Key_6); break;
                    case ConsoleKey.D7: await SendKeyAsync(rcClient, RCKeyCode.Key_7); break;
                    case ConsoleKey.D8: await SendKeyAsync(rcClient, RCKeyCode.Key_8); break;
                    case ConsoleKey.D9: await SendKeyAsync(rcClient, RCKeyCode.Key_9); break;

                    // app shortcuts
                    case ConsoleKey.W: await SendKeyAsync(rcClient, RCKeyCode.Key_F4);              break;
                    case ConsoleKey.L: await SendKeyAsync(rcClient, RCKeyCode.Key_F5);              break;
                    case ConsoleKey.T: await SendKeyAsync(rcClient, RCKeyCode.Key_TV);              break;
                    case ConsoleKey.P: await SendKeyAsync(rcClient, RCKeyCode.Key_GUIDE);           break;
                    case ConsoleKey.M: await SendKeyAsync(rcClient, RCKeyCode.Key_MENU);            break;
                    case ConsoleKey.N: await SendAppLaunchAsync(rcClient, "com.netflix.ninja"); break;

                    case ConsoleKey.H: ShowLegend(); break;

                    default: AnsiConsole.MarkupLine($"[red]No RCU Key Code connected with the key {cki.Key}[/]"); break;
                }
            } while (cki.Key != ConsoleKey.Escape);
        }

        #region Helpers
        private static async Task SendKeyAsync(RemoteControlClient client, RCKeyCode key)
        {
            AnsiConsole.MarkupLine($"Sending key [yellow]{key}[/]");
            try
            {
                await client.PressKeyAsync(key);
            }
            catch (Exception ex)
            {
                PauseReturnToMenu("[yellow]The connection to the device appears to be lost. Please ensure your " +
                "device is powered on and connected.[/]");
                AnsiConsole.WriteException(ex);
            }
        }
        
        private static async Task SendAppLaunchAsync(RemoteControlClient client, string app)
        {
            AnsiConsole.MarkupLine($"Sending App Launch [yellow]{app}[/]");
            try
            {
                await client.SendLaunchAppAsync(app);
            }
            catch (Exception ex)
            {
                PauseReturnToMenu("[yellow]The connection to the device appears to be lost. Please ensure your " +
                                  "device is powered on and connected.[/]");
                AnsiConsole.WriteException(ex);
            }
        }
        #endregion

        #region Output Methods
        private static void ShowLegend()
        {
            AnsiConsole.MarkupLine("[bold]Available Commands[/]");
            AnsiConsole.WriteLine("Arrows: DPAD_UP, DPAD_DOWN, DPAD_LEFT, DPAD_RIGHT");
            AnsiConsole.WriteLine("Numbers: Key_0 through Key_9");
            AnsiConsole.WriteLine("Enter: DPAD_CENTER");
            AnsiConsole.WriteLine("Backspace: BACK");
            AnsiConsole.WriteLine("Delete: POWER");
            AnsiConsole.WriteLine("Home: HOME");
            AnsiConsole.WriteLine("Space: MEDIA_PLAY_PAUSE");
            AnsiConsole.WriteLine("R: MEDIA_REWIND");
            AnsiConsole.WriteLine("F: MEDIA_FAST_FORWARD");
            AnsiConsole.WriteLine("Q: MEDIA_PREVIOUS");
            AnsiConsole.WriteLine("C: MEDIA_RECORD");
            AnsiConsole.WriteLine("Tab: MEDIA_NEXT");
            AnsiConsole.WriteLine("W: Key_F4");
            AnsiConsole.WriteLine("L: Key_F5");
            AnsiConsole.WriteLine("T: TV");
            AnsiConsole.WriteLine("P: GUIDE");
            AnsiConsole.WriteLine("M: MENU");
            AnsiConsole.WriteLine("N: NETFLIX");
            AnsiConsole.WriteLine();
        }

        private static void RcClient_RemoteConfigurationChanged(object? sender, EventArgs e)
        {
            var rcClient = (RemoteControlClient?)sender;
            if (rcClient?.RemoteDeviceInformation == null) return;

            AnsiConsole.MarkupLine("[bold]Remote Device Configuration[/]");
            AnsiConsole.MarkupLine($"Vendor:        {rcClient.RemoteDeviceInformation.Vendor}");
            AnsiConsole.MarkupLine($"Model:         {rcClient.RemoteDeviceInformation.Model}");
            AnsiConsole.MarkupLine($"Version:       {rcClient.RemoteDeviceInformation.Version}");
            AnsiConsole.MarkupLine($"PackageName:   {rcClient.RemoteDeviceInformation.PackageName}");
            AnsiConsole.MarkupLine($"AppVersion:    {rcClient.RemoteDeviceInformation.AppVersion}");
            Console.WriteLine();
        }
        #endregion
    }
}
