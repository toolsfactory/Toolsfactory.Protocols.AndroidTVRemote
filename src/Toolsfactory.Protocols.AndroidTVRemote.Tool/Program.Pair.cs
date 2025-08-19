using Spectre.Console;
using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;

namespace Toolsfactory.Protocols.AndroidTVRemote.Tool
{
    internal static partial class Program
    {
        private static Command BuildPairingCommand()
        {
            var host = new Option<string>("--host", "The device (IP or hostname) to pair with") { IsRequired = true };
            var file = new Option<string>("--file", "The file to store the pairing configuration (.apair) in") { IsRequired = true };
            var command = new Command("pair", "Pair a remote control with the Android device") { host, file };
            command.SetHandler(async (hostValue, fileValue) => await HandlePairingCommandAsync(hostValue, fileValue), host, file);
            return command;
        }

        private static async Task HandlePairingCommandAsync(string host, string file)
        {
            WriteHeadline("Manual Pairing");
            AnsiConsole.MarkupLine($"Pairing with [yellow]{host}[/]");

            await RunPairingScriptsAsync(host, file);
        }
        
        private static async Task RunPairingScriptsAsync(string host, string? outputFile = null)
        {
            if (!CheckPythonRequirements())
                return;

            try
            {
                var friendlyName = AnsiConsole.Ask<string>("Enter a friendly name for this device:");
                var deviceId     = AnsiConsole.Ask<string>("Enter the device ID:");
                var file         = string.IsNullOrWhiteSpace(outputFile)
                    ? AnsiConsole.Ask<string>("Enter the output file name (.apair):", "device.apair")
                    : outputFile;

                // 1) Run pair_android14.py
                var pairPsi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{ScriptPath}\" {host} --certfile {CertPath} --keyfile {KeyPath} --name \"{friendlyName}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    RedirectStandardInput = false,
                    CreateNoWindow = false
                };

                using (var pairProc = Process.Start(pairPsi))
                {
                    if (pairProc == null)
                        throw new PairingException("Failed to start Python process for pairing.");

                    await pairProc.WaitForExitAsync();
                    if (pairProc.ExitCode != 0)
                        throw new PairingException($"Python pairing failed (exit code {pairProc.ExitCode})");
                }

                // 2) Run rewrite_key_to_pkcs8.py
                var rewritePsi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{RewritePath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using (var rewriteProc = Process.Start(rewritePsi))
                {
                    if (rewriteProc == null)
                        throw new PairingException("Failed to start Python process for key rewrite.");

                    string stdout = await rewriteProc.StandardOutput.ReadToEndAsync();
                    string stderr = await rewriteProc.StandardError.ReadToEndAsync();
                    await rewriteProc.WaitForExitAsync();

                    if (!string.IsNullOrWhiteSpace(stdout)) AnsiConsole.WriteLine(stdout.Trim());
                    if (!string.IsNullOrWhiteSpace(stderr)) AnsiConsole.WriteLine(stderr.Trim());
                    if (rewriteProc.ExitCode != 0)
                        throw new PairingException($"Key rewrite failed (exit code {rewriteProc.ExitCode})");
                }

                var certPem = await File.ReadAllTextAsync(CertPath);
                var keyPem  = await File.ReadAllTextAsync(KeyPath);

                var config = new PairingConfiguration(friendlyName, deviceId, host, certPem + Environment.NewLine + keyPem);
                var json = JsonSerializer.Serialize(config, PairingConfigurationContext.Default.PairingConfiguration);

                await File.WriteAllTextAsync(file, json);

                var fullPath = Path.GetFullPath(file);
                PauseReturnToMenu($"[green]Pairing configuration saved to {fullPath}[/]");
            }
            finally
            {
                // Always delete temp files, even if an exception occurs above
                CleanupTemporaryFiles();
            }
        }
    }
}
