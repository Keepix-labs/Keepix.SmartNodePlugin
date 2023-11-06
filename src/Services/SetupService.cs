using Keepix.SmartNodePlugin.Utils;
using System.Runtime.InteropServices;
using Keepix.SmartNodePlugin.DTO.Input;

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
                result = Shell.ExecuteCommand("~/bin/rocketpool service start --yes", new List<ShellCondition>() {
                new ShellCondition() // LINUX
                {
                    content = "Press y when you understand the above warning",
                    answers = new string[] {"y", ""}
                }, // MAC 
                new ShellCondition()
                {
                    content = "Would you like to continue starting the service",
                    answers = new string[] {"y", ""}
                },
                 } );
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

        public static string ConfigSmartNode(InstallInput installInput)
        {
            string result = string.Empty;
            try
            {
                // EXECUTION CLIENT NETHERMIND
                // CONSENSUS CLIENT NIMBUS
                // MEV ENABLED
                // BEACON CHAIN SYNC POINT https://beaconstate-mainnet.chainsafe.io

                var network = installInput.Mainnet ? "mainnet" : "holesky";

                var cli = $"~/bin/rocketpool service config --smartnode-network {network} --smartnode-projectName keepix --smartnode-priorityFee 2 " + 
                " --executionClient nethermind --consensusClient nimbus";

                if (network == "mainnet")
                {
                    // add syncpoint url if we are on main net
                    cli += "  --consensusCommon-checkpointSyncUrl https://beaconstate-mainnet.chainsafe.io";
                }

                if (installInput.EnableMEV) {
                    cli += " --mevBoost-mode local --mevBoost-selectionMode profile --mevBoost-enableUnregulatedAllMev";
                }

                result = Shell.ExecuteCommand("");
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
            try { Shell.ExecuteCommand("mkdir ~/bin"); } catch (Exception) { }
            // Download RPL cli
            Shell.ExecuteCommand($"curl -L https://github.com/rocket-pool/smartnode-install/releases/latest/download/{build} -o ~/bin/rocketpool");

            // Set the CLI as executable
            Shell.ExecuteCommand("chmod +x ~/bin/rocketpool");

            return true;
        }

        public static bool RemoveNode()
        {

            Shell.ExecuteCommand("~/bin/rocketpool service stop --yes");
            Shell.ExecuteCommand("docker stop keepix_exporter keepix_api keepix_validator keepix_eth2 keepix_node keepix_eth1 keepix_watchtower keepix_mev-boost keepix_grafana keepix_prometheus");
            Shell.ExecuteCommand("docker rm keepix_exporter keepix_api keepix_validator keepix_eth2 keepix_node keepix_eth1 keepix_watchtower keepix_mev-boost keepix_grafana keepix_prometheus");
            
              ShellCondition conditions = new ShellCondition()
                {
                    content = "Are you sure you want to continue",
                    answers = new string[] {"y", ""}
                };
            Shell.ExecuteCommand("docker volume prune", new List<ShellCondition>() { conditions } );

            // Shell.ExecuteCommand("rm -rf ~/.rocketpool");
            Shell.ExecuteCommand("rm -rf ~/bin/rocketpool");
            return true;
        }

        public static bool IsNodeRunning()
        {
            var result = Shell.ExecuteCommand("docker ps");
            if (result.Contains("keepix_eth2")) {
                return true;
            }
            return false;
        }

        public static string StartNode()
        {
            var result = Shell.ExecuteCommand("~/bin/rocketpool service start");
            if (result.Contains("Your validator will miss up to 3 attestations when it starts")) {
                return string.Empty;
            }
            return result;
        }
        public static string StopNode()
        {
            var result = Shell.ExecuteCommand("~/bin/rocketpool service stop --yes");
            if (result.Contains("stop your validator")) {
                return string.Empty;
            }
            return result;
        }
    }
}
