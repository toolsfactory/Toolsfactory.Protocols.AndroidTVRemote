using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolsfactory.Protocols.AndroidTVRemote.Tool
{
    internal partial class Program
    {
        private static Command BuildScanCommand()
        {
            var command = new Command("scan", "Scan for Android TV devices");
            command.SetHandler(async () => await HandleScanCommandAsync());
            return command;
        }

        private static async Task HandleScanCommandAsync()
        {
            WriteHeadline("Scanning for Android TV devices");
            await FindAndroidTVDevicesAsync();
        }
    }
}
