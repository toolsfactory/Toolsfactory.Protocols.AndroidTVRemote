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

            if (!await EnsurePythonDependenciesAsync())
            {
                PauseReturnToMenu("[red]Python dependencies could not be installed.[/]");
                return;
            }
            
            if (!await ValidateHostAsync(host))
            {
                PauseReturnToMenu("[red]Invalid IP address or hostname. Please check your input.[/]");
                return;
            }

            try
            {
                var friendlyName = AnsiConsole.Ask<string>("Enter a friendly name for this device:");
                var deviceId = AnsiConsole.Ask<string>("Enter the device ID:");
                var file = string.IsNullOrWhiteSpace(outputFile)
                    ? AnsiConsole.Ask<string>("Enter the output file name (.apair):", "device.apair")
                    : outputFile;

                await RunPythonPairingProcess(host, friendlyName);
                await RunKeyRewriteProcess();
                await SavePairingConfiguration(friendlyName, deviceId, host, file);
            }
            catch (PairingAttemptsException)
            {
                PauseReturnToMenu();
            }
            catch (PairingException ex)
            {
                PauseReturnToMenu($"[red]Pairing error: {ex.Message}[/]");
            }
            catch (Exception ex)
            {
                PauseReturnToMenu($"[red]Unexpected pairing error: {ex.Message}[/]");
            }
            finally
            {
                CleanupTemporaryFiles();
            }
        }

        private static async Task RunPythonPairingProcess(string host, string friendlyName)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"-u \"{ScriptPath}\" {host} --certfile {CertPath} --keyfile {KeyPath} --name \"{friendlyName}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = false,
                CreateNoWindow = false
            };

            using var process = Process.Start(psi);
            if (process == null)
                throw new PairingException("Python pairing process could not be started.");

            var outputTask = HandleProcessOutput(process);
            var errorTask = HandleProcessErrors(process);

            await process.WaitForExitAsync();

            try
            {
                await Task.WhenAll(outputTask, errorTask);
            }
            catch
            {
                // Ignore exceptions from I/O tasks
            }

            if (process.ExitCode == 1)
                throw new PairingAttemptsException("Pairing was cancelled due to incorrect PIN attempts.");

            if (process.ExitCode != 0)
                throw new PairingException($"Python pairing failed (exit code {process.ExitCode})");
        }

        private static async Task HandleProcessOutput(Process process)
        {
            var buffer = new char[1];
            while (!process.StandardOutput.EndOfStream)
            {
                var read = await process.StandardOutput.ReadAsync(buffer, 0, 1);
                if (read > 0)
                    Console.Write(buffer[0]);
            }
        }

        private static async Task HandleProcessErrors(Process process)
        {
            var errorOutput = await process.StandardError.ReadToEndAsync();
            if (!string.IsNullOrWhiteSpace(errorOutput))
                Console.Write(errorOutput);
        }
        
        private static async Task<bool> ValidateHostAsync(string host)
        {
            if (!System.Net.IPAddress.TryParse(host, out _))
                return false;

            if (host.All(char.IsDigit)) // Prevent 32-bit integer inputs
                return false;

            try
            {
                var addresses = await System.Net.Dns.GetHostAddressesAsync(host);
                return addresses.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        private static async Task RunKeyRewriteProcess()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{RewritePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
                throw new PairingException("Failed to start Python process for key rewrite.");

            var stdout = await process.StandardOutput.ReadToEndAsync();
            var stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(stdout)) AnsiConsole.WriteLine(stdout.Trim());
            if (!string.IsNullOrWhiteSpace(stderr)) AnsiConsole.WriteLine(stderr.Trim());
            
            if (process.ExitCode != 0)
                throw new PairingException($"Key rewrite failed (exit code {process.ExitCode})");
        }

        private static async Task SavePairingConfiguration(string friendlyName, string deviceId, string host, string file)
        {
            var certPem = await File.ReadAllTextAsync(CertPath);
            var keyPem = await File.ReadAllTextAsync(KeyPath);

            var config = new PairingConfiguration(friendlyName, deviceId, host, certPem + Environment.NewLine + keyPem);
            var json = JsonSerializer.Serialize(config, PairingConfigurationContext.Default.PairingConfiguration);

            await File.WriteAllTextAsync(file, json);

            var fullPath = Path.GetFullPath(file);
            PauseReturnToMenu($"[green]Pairing configuration saved to {fullPath}[/]");
        }
    }
}
