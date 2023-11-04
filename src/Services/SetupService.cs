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
                // exit code != 0 will throw an exception so we will know that the cli is not installed properly
                return res.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string StartSmartNode()
        {
            string result = string.Empty;
            try
            {
                // this will wait for this text prompted to move onto the next step
                ShellCondition conditions = new ShellCondition()
                {
                    content = "Press y when you understand the above warning",
                    answers = new string[] {"y", ""}
                };

                result = Shell.ExecuteCommand("~/bin/rocketpool service start --yes", new List<ShellCondition>() { conditions } );
                if (result.Contains("You currently have Doppelganger Protection enabled") || result.Contains("Running"))
                    return string.Empty;
                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }


        public static string InstallSmartNode()
        {
            string result = string.Empty;
            try
            {
                result = Shell.ExecuteCommand("~/bin/rocketpool service install -y -d");
                if (result.Length > 0) {
                    // exit code != 0 will throw an exception so we will know if the directory was not installed properly
                    var dir = Shell.ExecuteCommand("cd ~/.rocketpool");
                    return string.Empty;
                }
                else
                {
                    return "Error from rocketpool CLI, empty answer from the executable while installing the smart node";
                }
            }
            catch (Exception)
            {
                return result;
            }
        }

        public static string ConfigSmartNode()
        {
            string result = string.Empty;
            try
            {
                // EXECUTION CLIENT NETHERMIND
                // CONSENSUS CLIENT NIMBUS
                // MEV ENABLED
                // BEACON CHAIN SYNC POINT https://beaconstate-mainnet.chainsafe.io

                result = Shell.ExecuteCommand("~/bin/rocketpool service config --smartnode-network mainnet --smartnode-projectName keepix --smartnode-priorityFee 2 --executionClient nethermind --consensusClient nimbus --consensusCommon-checkpointSyncUrl https://beaconstate-mainnet.chainsafe.io --mevBoost-mode local --mevBoost-selectionMode profile --mevBoost-enableUnregulatedAllMev");
                if (result.Trim().Length > 0) {
                    return result;
                }
                return string.Empty;
            }
            catch (Exception)
            {
                return result;
            }
        }

        public static bool DownloadCli()
        {
            OSPlatform oSPlatform = OS.GetOS();
            bool isArm64 = RuntimeInformation.ProcessArchitecture == Architecture.Arm64;
            // windows detected means with WSL2 compability only
            string osValue = (oSPlatform == OSPlatform.Linux || oSPlatform == OSPlatform.Windows) ? "linux" : "darwin";
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
