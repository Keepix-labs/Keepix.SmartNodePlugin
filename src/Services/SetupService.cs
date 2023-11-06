using Keepix.SmartNodePlugin.Utils;
using System.Runtime.InteropServices;
using Keepix.SmartNodePlugin.DTO.Input;
using System.Xml.XPath;

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
                    content = "You currently have Doppelganger Protection enabled",
                    answers = new string[] {"y", ""}
                }
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

        public static string ConfigSmartNode(InstallInput installInput, PluginStateManager stateManager)
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

                result = Shell.ExecuteCommand(cli);
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

        public static string DownloadCli()
        {
            string result = string.Empty;
            try {
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
                result = Shell.ExecuteCommand($"curl -L https://github.com/rocket-pool/smartnode-install/releases/latest/download/{build} -o ~/bin/rocketpool");

                // Set the CLI as executable
                result = Shell.ExecuteCommand("chmod +x ~/bin/rocketpool");
            }
            catch (Exception) {
                return result;
            }

            return string.Empty;
        }

        public static string RemoveNode(PluginStateManager stateManager)
        {
            string result = string.Empty;
            try {
                result = Shell.ExecuteCommand("~/bin/rocketpool service stop --yes");
                string containers = "keepix_exporter keepix_api keepix_validator keepix_eth2 keepix_node keepix_eth1 keepix_watchtower keepix_grafana keepix_prometheus";
                InstallInput? input = stateManager.DB.Retrieve<InstallInput>("INSTALL");
                //adding mev boost containers also if chosen in settings of installation
                if (input != null && input.EnableMEV) {
                    containers += " keepix_mev-boost";
                }

                result = Shell.ExecuteCommand($"docker stop {containers}");
                result = Shell.ExecuteCommand($"docker rm {containers}");
                
                ShellCondition conditions = new ShellCondition()
                    {
                        content = "This will remove anonymous local volumes",
                        answers = new string[] {"y", ""}
                    };
                result = Shell.ExecuteCommand("docker volume prune", new List<ShellCondition>() { conditions } );

                // Shell.ExecuteCommand("rm -rf ~/.rocketpool");
                result = Shell.ExecuteCommand("rm -rf ~/bin/rocketpool");
            } catch (Exception) {
                return result;
            }
            return string.Empty;
        }

        public static bool IsNodeRunning()
        {
            var result = Shell.ExecuteCommand("docker ps");
            if (result.Contains("keepix_eth2")) {
                return true;
            }
            return false;
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
