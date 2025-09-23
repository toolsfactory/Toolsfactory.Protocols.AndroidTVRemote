using Spectre.Console;
using System.CommandLine;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Toolsfactory.Protocols.AndroidTVRemote.Tool
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    internal static partial class Program
    {
        #region Private Properties
        private static string ScriptPath  => ResolveScriptPath("pair_device.py");
        private static string RewritePath => ResolveScriptPath("rewrite_key_to_pkcs8.py");
        private const string CertPath = "temp-cert.pem";
        private const string KeyPath  = "temp-key.pem";
        
        private sealed record DeviceItem(string IP, string Name);
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
            var result = await ScanAndroidTVDevicesAsync();
            
            if (result.Count == 0)
            {
                AnsiConsole.MarkupLine("[bold red]No devices found[/]");
                PauseReturnToMenu();
                return;
            }

            var devices = result
                .Select(item => new DeviceItem(item.IPAddress, item.DisplayName))
                .ToList();

            devices.Add(new DeviceItem("", "Go back to main menu"));

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<DeviceItem>()
                    .Title("What device do you want to pair with?")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more devices)[/]")
                    .AddChoices(devices)
                    .UseConverter(d => $"{d.IP} - {d.Name}")
            );

            if (string.IsNullOrEmpty(selected.IP)) return;

            var host = selected.IP;
            AnsiConsole.MarkupLine($"Pairing with [yellow]{host}[/]");
            
            await RunPairingScriptsAsync(host);
        }
        #endregion

        #region Helper Functions
        private static string ResolveScriptPath(string filename)
        {
            foreach (var dir in GetScriptSearchPaths())
            {
                var candidate = Path.Combine(dir, filename);
                if (File.Exists(candidate))
                    return Path.GetFullPath(candidate);
            }

            // Build a helpful message showing all places we checked
            var searched = string.Join(
                Environment.NewLine + "  - ",
                GetScriptSearchPaths().Select(p => Path.GetFullPath(p).ToList())
            );

            throw new PairingException(
                $"Cannot find '{filename}'. Looked in:{Environment.NewLine}  - {searched}{Environment.NewLine}" +
                "Optionally set the TF_ATV_SCRIPTS environment variable to the directory that contains the scripts.");
        }

        private static IEnumerable<string> GetScriptSearchPaths()
        {
            var baseDir = AppContext.BaseDirectory;
            var cwd = Directory.GetCurrentDirectory();
            var env = Environment.GetEnvironmentVariable("TF_ATV_SCRIPTS");

            if (!string.IsNullOrWhiteSpace(env))
                yield return env;
            
            var legacyRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "PairingScripts"));
            yield return legacyRoot;

            // Other sensible locations (fallbacks)
            yield return Path.Combine(baseDir, "PairingScripts");
            yield return baseDir;
            yield return Path.Combine(cwd, "PairingScripts");
            yield return cwd;
        }
        
        private static bool CheckPythonRequirements()
        {
            try
            {
                // Check if python is available
                var psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                process?.WaitForExit();
        
                if (process?.ExitCode != 0)
                {
                    PauseReturnToMenu("[red]Python is not installed or not available in PATH.[/]");
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                PauseReturnToMenu("[red]Python could not be found.[/]");
                return false;
            }
        }
        
        private static async Task<bool> EnsurePythonDependenciesAsync()
        {
            try
            {
                // Check if androidtvremote2 module is available
                var checkPsi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "-c \"import androidtvremote2; print('androidtvremote2 is available')\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var checkProc = Process.Start(checkPsi);
                await checkProc!.WaitForExitAsync();

                if (checkProc.ExitCode == 0)
                    return true;

                AnsiConsole.MarkupLine("[yellow]Installing Python dependencies...[/]");

                // // Check if pip module is available
                var pipCheckPsi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "-m pip --version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var pipCheck = Process.Start(pipCheckPsi);
                await pipCheck!.WaitForExitAsync();

                if (pipCheck.ExitCode != 0)
                {
                    AnsiConsole.MarkupLine("[red]pip is not available. Please install pip.[/]");
                    return false;
                }

                // Install androidtvremote2
                var installPsi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "-m pip install androidtvremote2",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = false
                };

                using var installProc = Process.Start(installPsi);
                await installProc!.WaitForExitAsync();

                if (installProc.ExitCode != 0)
                {
                    var stderr = await installProc.StandardError.ReadToEndAsync();
                    AnsiConsole.MarkupLine($"[red]Installation failed: {stderr}[/]");
                    return false;
                }

                AnsiConsole.MarkupLine("[green]Python dependencies installed successfully.[/]");
                return true;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error checking/installing dependencies: {ex.Message}[/]");
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
