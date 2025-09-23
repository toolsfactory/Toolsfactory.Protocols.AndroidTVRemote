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
        
        #region Interactive Command Handler
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

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        
                var configReceived = new TaskCompletionSource<bool>();
                rcClient.RemoteConfigurationChanged += (s, e) =>
                {
                    RcClient_RemoteConfigurationChanged(s, e);
                    configReceived.TrySetResult(true);
                };

                AnsiConsole.MarkupLine("[grey]Connecting...[/]");
                await rcClient.ConnectAsync(cts.Token);
        
                AnsiConsole.MarkupLine("[grey]Waiting for device configuration...[/]");
                await configReceived.Task.WaitAsync(cts.Token);
        
                AnsiConsole.MarkupLine("[green]Connected successfully![/]");
            }
            catch (OperationCanceledException)
            {
                PauseReturnToMenu("[red]Connection timeout. Please check if the device is reachable and the pairing file is valid.[/]");
                return;
            }
            catch (Exception ex)
            {
                PauseReturnToMenu($"[red]Connection failed: {ex.Message}[/]");
                return;
            }
            
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
                
                bool actionResult = cki.Key switch
                {
                    // arrows
                    ConsoleKey.UpArrow => await SendKeyAsync(rcClient, RCKeyCode.Key_DPAD_UP),
                    ConsoleKey.DownArrow => await SendKeyAsync(rcClient, RCKeyCode.Key_DPAD_DOWN),
                    ConsoleKey.LeftArrow => await SendKeyAsync(rcClient, RCKeyCode.Key_DPAD_LEFT),
                    ConsoleKey.RightArrow => await SendKeyAsync(rcClient, RCKeyCode.Key_DPAD_RIGHT),

                    // basic controls
                    ConsoleKey.Enter => await SendKeyAsync(rcClient, RCKeyCode.Key_DPAD_CENTER),
                    ConsoleKey.Backspace => await SendKeyAsync(rcClient, RCKeyCode.Key_BACK),
                    ConsoleKey.Q => await SendKeyAsync(rcClient, RCKeyCode.Key_POWER),
                    ConsoleKey.Home => await SendKeyAsync(rcClient, RCKeyCode.Key_HOME),

                    // media
                    ConsoleKey.Spacebar => await SendKeyAsync(rcClient, RCKeyCode.Key_MEDIA_PLAY_PAUSE),
                    ConsoleKey.R => await SendKeyAsync(rcClient, RCKeyCode.Key_MEDIA_REWIND),
                    ConsoleKey.F => await SendKeyAsync(rcClient, RCKeyCode.Key_MEDIA_FAST_FORWARD),
                    ConsoleKey.PageDown => await SendKeyAsync(rcClient, RCKeyCode.Key_MEDIA_PREVIOUS),
                    ConsoleKey.PageUp => await SendKeyAsync(rcClient, RCKeyCode.Key_MEDIA_NEXT),
                    ConsoleKey.C => await SendKeyAsync(rcClient, RCKeyCode.Key_MEDIA_RECORD),

                    // numbers
                    ConsoleKey.D0 => await SendKeyAsync(rcClient, RCKeyCode.Key_0),
                    ConsoleKey.D1 => await SendKeyAsync(rcClient, RCKeyCode.Key_1),
                    ConsoleKey.D2 => await SendKeyAsync(rcClient, RCKeyCode.Key_2),
                    ConsoleKey.D3 => await SendKeyAsync(rcClient, RCKeyCode.Key_3),
                    ConsoleKey.D4 => await SendKeyAsync(rcClient, RCKeyCode.Key_4),
                    ConsoleKey.D5 => await SendKeyAsync(rcClient, RCKeyCode.Key_5),
                    ConsoleKey.D6 => await SendKeyAsync(rcClient, RCKeyCode.Key_6),
                    ConsoleKey.D7 => await SendKeyAsync(rcClient, RCKeyCode.Key_7),
                    ConsoleKey.D8 => await SendKeyAsync(rcClient, RCKeyCode.Key_8),
                    ConsoleKey.D9 => await SendKeyAsync(rcClient, RCKeyCode.Key_9),

                    // app shortcuts
                    ConsoleKey.W => await SendKeyAsync(rcClient, RCKeyCode.Key_F4),
                    ConsoleKey.Y => await SendKeyAsync(rcClient, RCKeyCode.Key_F5),
                    ConsoleKey.T => await SendKeyAsync(rcClient, RCKeyCode.Key_TV),
                    ConsoleKey.P => await SendKeyAsync(rcClient, RCKeyCode.Key_GUIDE),
                    ConsoleKey.M => await SendKeyAsync(rcClient, RCKeyCode.Key_MENU),
                    ConsoleKey.N => await SendAppLaunchAsync(rcClient, "com.netflix.ninja"),

                    // help
                    ConsoleKey.H => ShowLegend(),

                    // unmapped keys
                    _ => ShowKeyNotMapped(cki.Key)
                };
                
                if (!actionResult) break;

            } while (cki.Key != ConsoleKey.Escape);
        }
        #endregion
        
        #region Helpers
        private static async Task<bool> SendKeyAsync(RemoteControlClient client, RCKeyCode key)
        {
            AnsiConsole.MarkupLine($"Sending key [yellow]{key}[/]");
            try
            {
                await client.PressKeyAsync(key);
                return true;
            }
            catch (Exception ex)
            {
                PauseReturnToMenu("[yellow]The connection to the device appears to be lost. Please ensure your " +
                "device is powered on and connected.[/]");
                AnsiConsole.WriteException(ex);

                return false;
            }
        }
        
        private static async Task<bool> SendAppLaunchAsync(RemoteControlClient client, string app)
        {
            AnsiConsole.MarkupLine($"Sending App Launch [yellow]{app}[/]");
            try
            {
                await client.SendLaunchAppAsync(app);
                return true;
            }
            catch (Exception ex)
            {
                PauseReturnToMenu("[yellow]The connection to the device appears to be lost. Please ensure your " +
                                  "device is powered on and connected.[/]");
                AnsiConsole.WriteException(ex);

                return false;
            }
        }
        #endregion

        #region Output Methods
        private static bool ShowLegend()
        {
            AnsiConsole.MarkupLine("[bold]Available Commands[/]");
            AnsiConsole.WriteLine("Arrows: DPAD_UP, DPAD_DOWN, DPAD_LEFT, DPAD_RIGHT");
            AnsiConsole.WriteLine("Numbers: Key_0 through Key_9");
            AnsiConsole.WriteLine("Enter: DPAD_CENTER");
            AnsiConsole.WriteLine("Backspace: BACK");
            AnsiConsole.WriteLine("Q: POWER");
            AnsiConsole.WriteLine("Home: HOME");
            AnsiConsole.WriteLine("Space: MEDIA_PLAY_PAUSE");
            AnsiConsole.WriteLine("R: MEDIA_REWIND");
            AnsiConsole.WriteLine("F: MEDIA_FAST_FORWARD");
            AnsiConsole.WriteLine("Page Down: MEDIA_PREVIOUS");
            AnsiConsole.WriteLine("Page Up: MEDIA_NEXT");
            AnsiConsole.WriteLine("C: MEDIA_RECORD");
            AnsiConsole.WriteLine("Y: YOUTUBE");
            AnsiConsole.WriteLine("W: WAIPUTHEK");
            AnsiConsole.WriteLine("T: TV");
            AnsiConsole.WriteLine("P: GUIDE");
            AnsiConsole.WriteLine("M: MENU");
            AnsiConsole.WriteLine("N: NETFLIX");
            AnsiConsole.WriteLine();

            return true;
        }
        
        private static bool ShowKeyNotMapped(ConsoleKey key)
        {
            AnsiConsole.MarkupLine($"[red]The key [yellow]{key}[/] is not mapped to any action.[/]");
            return true;
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
