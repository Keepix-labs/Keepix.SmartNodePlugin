using Keepix.SmartNodePlugin.Utils;
using System.Runtime.InteropServices;

namespace Keepix.SmartNodePlugin.Services
{
    internal class SetupService
    {
        public static bool isCliInstalled()
        {
            try
            {
                var res = Shell.ExecuteCommand("~/bin/rocketpool");
                return res.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool DownloadCli()
        {
            OSPlatform oSPlatform = OS.GetOS();
            bool isArm64 = RuntimeInformation.ProcessArchitecture == Architecture.Arm64;
            // string osValue = (oSPlatform == OSPlatform.Linux) ? "linux" : "darwin";
            string osValue = "linux";
            string arch = isArm64 ? "arm64" : "amd64";
            string build = $"rocketpool-cli-{osValue}-{arch}";

            Console.WriteLine($"Download RPL CLI {build}...");
            // Create binary directory if it does not already exist
            Shell.ExecuteCommand("mkdir ~/bin");
            // Download RPL cli
            Shell.ExecuteCommand($"curl -L https://github.com/rocket-pool/smartnode-install/releases/latest/download/{build} -o ~/bin/rocketpool");

            // Set the CLI as executable
            Shell.ExecuteCommand("chmod +x ~/bin/rocketpool");

            return true;
        }
    }
}
