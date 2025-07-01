using Spectre.Console;
using System.CommandLine;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Zeroconf;

namespace Toolsfactory.Protocols.AndroidTVRemote.Tool
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    internal partial class Program
    {
        #region Private Properties
        // Locate the Python pairing script
        private static readonly string ScriptRoot = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "PairingScripts");
        private static readonly string ScriptPath = Path.GetFullPath(Path.Combine(ScriptRoot, "pair_android14.py"));
        private static readonly string RewritePath = Path.GetFullPath(Path.Combine(ScriptRoot, "rewrite_key_to_pkcs8.py"));

        private const string CertPath = "temp-cert.pem";
        private const string KeyPath  = "temp-key.pem";
        #endregion
        
        #region Interactive Pairing Command
        private static Command BuildInteractivePairingCommand()
        {
            var command = new Command("interactivepairing", "Scan for Android TV devices and pair");
            command.SetHandler(async () => await HandleInteractivePairingCommandAsync());
            return command;
        }

        private static async Task HandleInteractivePairingCommandAsync()
        {
            WriteHeadline("Interactive Pairing");
            AnsiConsole.WriteLine("Scanning for Android TV devices...");
            var result = await ZeroconfResolver.ResolveAsync("_androidtvremote2._tcp.local.");
            Console.WriteLine();
            
            if (result.Count == 0)
            {
                AnsiConsole.MarkupLine("[bold red]No devices found[/]");
                return;
            }

            var choices = new List<string>();
            foreach (var item in result)
            {
                var itemDesc = string.Concat((item.IPAddress + "      ").AsSpan(0, 15), " - ", item.DisplayName);
                choices.Add(itemDesc);
            }

            var device = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What device do you want to pair with?")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more devices)[/]")
                    .AddChoices(choices));

            var host = device.Substring(0, 15).Trim();
            AnsiConsole.MarkupLine($"Pairing with [yellow]{host}[/]");

            var name = AnsiConsole.Ask<string>("Enter a friendly name for this device:");

            if (!CheckPythonRequirements()) return;

            // Invoke the Python script
            var psi = new ProcessStartInfo
            {
                FileName = "python", 
                Arguments = $"\"{ScriptPath}\" {host} --certfile temp-cert.pem --keyfile temp-key.pem --name \"{name}\"",
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                RedirectStandardInput = false,
                CreateNoWindow = false
            };

            var process = Process.Start(psi);
            if (process == null)
            {
                AnsiConsole.MarkupLine("[red]Error:[/] Failed to start Python process.");
                return;
            }

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                AnsiConsole.MarkupLine($"[bold red]Python pairing failed (exit code {process.ExitCode})[/]");
                return;
            }

            AnsiConsole.MarkupLine("[green]Python pairing succeeded![/]");
            
            // Rewrite private key to PKCS#8 format (needed for .NET)
            var fixKey = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{RewritePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                CreateNoWindow = true
            };

            var fixProc = Process.Start(fixKey);
            if (fixProc == null)
                AnsiConsole.MarkupLine("[yellow]Warning:[/] Could not start rewrite_key_to_pkcs8.py.");
            else
            {
                await fixProc.WaitForExitAsync();
                AnsiConsole.MarkupLine(fixProc.ExitCode != 0
                    ? "[yellow]Warning:[/] Rewriting key to PKCS#8 failed. .NET may not accept the .apair file."
                    : "[green]Private key converted to PKCS#8 format.[/]");
            }

            // Read generated PEM files
            if (!File.Exists(CertPath) || !File.Exists(KeyPath))
            {
                AnsiConsole.MarkupLine("[red]Error:[/] Certificate files not found after pairing.");
                return;
            }

            var certPem = await File.ReadAllTextAsync(CertPath);
            var keyPem  = await File.ReadAllTextAsync(KeyPath);

            if (!AnsiConsole.Confirm("Save pairing configuration to .apair file?"))
                return;

            var id   = AnsiConsole.Ask<string>("Device ID:");
            var file = AnsiConsole.Ask<string>("File to save to", "device.apair");
            Console.WriteLine();

            var config = new PairingConfiguration(name, id, host, certPem + Environment.NewLine + keyPem);
            var json   = JsonSerializer.Serialize(config, typeof(PairingConfiguration), new JsonSerializerOptions
            {
                WriteIndented    = true,
                TypeInfoResolver = PairingConfigurationContext.Default
            });

            await File.WriteAllTextAsync(file, json);
            var fullPath = Path.GetFullPath(file);
            AnsiConsole.MarkupLine($"Pairing configuration saved to [green]{fullPath}[/]");

            CleanupTemporaryFiles();
        }
        #endregion

        #region Helper Functions
        private static bool CheckPythonRequirements()
        {
            if (!File.Exists(ScriptPath))
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] Cannot find pairing script at '{ScriptPath}'. Please ensure the script exists in the expected location.");
                return false;
            }

            if (!File.Exists(RewritePath))
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] Cannot find rewrite script at '{RewritePath}'. Please ensure the script exists in the expected location.");
                return false;
            }

            if (!IsPythonAvailable())
            {
                AnsiConsole.MarkupLine("[red]Python is not available in PATH. Please install Python 3 and ensure 'python' is accessible from the terminal.[/]");
                return false;
            }

            return true;
        }
        
        private static bool IsPythonAvailable()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                process!.WaitForExit();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
        
        private static void CleanupTemporaryFiles()
        {
            try
            {
                File.Delete(CertPath);
                File.Delete(KeyPath);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[yellow]Warning:[/] Failed to delete temporary files: {ex.Message}");
            }
        }
        #endregion
    }
}
