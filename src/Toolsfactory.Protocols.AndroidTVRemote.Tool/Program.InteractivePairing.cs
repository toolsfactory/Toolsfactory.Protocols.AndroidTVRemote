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
        private static string ScriptPath  => ResolveScriptPath("pair_android14.py");
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

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<DeviceItem>()
                    .Title("What device do you want to pair with?")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more devices)[/]")
                    .AddChoices(devices)
                    .UseConverter(d => $"{d.IP} - {d.Name}")
            );

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
                string unused  = ScriptPath;   // may throw if not found
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
                return false;
            }

            try
            {
                string unused = RewritePath;  // may throw if not found
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
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
                process?.WaitForExit();
                return process != null && process.ExitCode == 0;
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
