using Keepix.SmartNodePlugin.Utils;
using System.Runtime.InteropServices;
using Keepix.SmartNodePlugin.DTO.Input;
using System.Xml.XPath;

namespace Keepix.SmartNodePlugin.Services
{
    internal class SetupService
    {

        public static bool isDockerRunning()
        {
            try
            {
                var res = Shell.ExecuteCommand("docker info");
                return res.Contains("Kernel Version");
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool isCliInstalled()
        {
            try
            {
                var res = Shell.ExecuteCommand("~/bin/rocketpool --allow-root");
                // exit code != 0 will throw an exception so we will know that the cli is not installed properly
                return res.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string StartSmartNode(PluginStateManager stateManager)
        {
            string result = string.Empty;
            try
            {
                OSPlatform oSPlatform = OS.GetOS();
                if (oSPlatform == OSPlatform.OSX) {
                        try {
                        // fix exporter issues on OSX at startup
                        string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        var path = $"{homeDirectory}/.rocketpool/override/exporter.yml";
                        string fileContent = File.ReadAllText(path);
                        if (!fileContent.Contains("/proc:/host/proc:ro")) {
                            File.AppendAllText(path, "\n    volumes: [\"/proc:/host/proc:ro\",\"/sys:/host/sys:ro\"]\n");
                        }
                        } catch (Exception error) { Console.WriteLine(error); }
                        stateManager.DB.Store("OSX_VOLUME_FIXED", true);
                }

                // this will wait for this text prompted to move onto the next step
                result = Shell.ExecuteCommand("~/bin/rocketpool --allow-root service start --yes", new List<ShellCondition>() {
                new ShellCondition()
                {
                    content = "You currently have Doppelganger Protection enabled",
                    answers = new string[] {"y", ""}
                },
                new ShellCondition() // first run
                {
                    content = "you can safely ignore this warning",
                    answers = new string[] {"y", ""}
                },
                 new ShellCondition() // first run
                {
                    content = "Press y when you understand the above warning",
                    answers = new string[] {"y", ""}
                },
                 new ShellCondition() // first run
                {
                    content = "Couldn't determine previous Smartnode version from backup settings",
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
                result = Shell.ExecuteCommand("~/bin/rocketpool --allow-root service install -y -d");
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

                var cli = $"~/bin/rocketpool --allow-root service config --smartnode-network {network} --smartnode-projectName keepix --smartnode-priorityFee 2 " + 
                " --executionClient nethermind --consensusClient nimbus --nimbus-additionalBnFlags '--web3-url=http://127.0.0.1:8551' --nethermind-containerTag nethermind/nethermind:latest";

                if (network == "mainnet")
                {
                    // add syncpoint url if we are on main net
                    cli += "  --consensusCommon-checkpointSyncUrl https://beaconstate-mainnet.chainsafe.io";
                }
                else // syncpoint for testnet
                    cli += "  --consensusCommon-checkpointSyncUrl https://holesky.beaconstate.info/";

                if (installInput.EnableMEV) {
                    cli += " --mevBoost-mode local --mevBoost-selectionMode profile --mevBoost-enableUnregulatedAllMev";
                }

                Console.WriteLine($"CLI Config Smart Node {cli}");

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

        public static bool VerifyInstallation(PluginStateManager stateManager)
        {
            string containersList = GetContainers(stateManager);
            string[] containers = containersList.Split(' ');

            bool dockersIsAlive = true;
            foreach (var containerKey in containers) {
                try {
                    var stdOut = Shell.ExecuteCommand($"docker container inspect {containerKey}");
                    if (stdOut.Contains("No such container")) {
                        dockersIsAlive = false;
                        Console.WriteLine($"Container {containerKey} not found.");
                        break ;
                    }
                } catch(Exception) {
                    dockersIsAlive = false;
                    Console.WriteLine($"Container {containerKey} Error.");
                    break ;
                }
            }

            string volumesList = GetVolumes();
            string[] volumes = volumesList.Split(' ');

            bool volumesIsAlive = true;
            foreach (var volumeKey in volumes) {
                try {
                    var stdOut = Shell.ExecuteCommand($"docker volume inspect {volumeKey}");
                    if (stdOut.Contains($"no such volume")) {
                        volumesIsAlive = false;
                        Console.WriteLine($"Volume {volumeKey} not found.");
                        break ;
                    }
                } catch(Exception) {
                    volumesIsAlive = false;
                    Console.WriteLine($"Volume {volumeKey} Error.");
                    break ;
                }
            }

            string networksList = GetNetworks();
            string[] networks = networksList.Split(' ');

            bool networksIsAlive = true;
            foreach (var networkKey in networks) {
                try {
                    var stdOut = Shell.ExecuteCommand($"docker network inspect {networkKey}");
                    if (stdOut.Contains($"network {networkKey} not found")) {
                        networksIsAlive = false;
                        Console.WriteLine($"Network {networkKey} not found.");
                        break ;
                    }
                } catch(Exception e) {
                    networksIsAlive = false;
                    Console.WriteLine($"Network {networkKey} Error.");
                    break ;
                }
            }
            return dockersIsAlive && volumesIsAlive && networksIsAlive;
        }

        public static string RemoveNode(PluginStateManager stateManager)
        {
            string result = string.Empty;
            try {
                result = StopNode();
                string containers = GetContainers(stateManager);
                string volumes = GetVolumes();
                string networks = GetNetworks();
                
                Console.WriteLine($"stop containers {containers}");
                try { Shell.ExecuteCommand($"docker stop {containers}"); } catch (Exception) {}
                Console.WriteLine($"rm containers {containers}");
                try { Shell.ExecuteCommand($"docker rm {containers}"); } catch (Exception) {}
                Console.WriteLine($"volume prune.");
                try {
                    ShellCondition conditions = new ShellCondition()
                    {
                        content = "This will remove anonymous local volumes",
                        answers = new string[] {"y", ""}
                    };
                    result = Shell.ExecuteCommand("docker volume prune", new List<ShellCondition>() { conditions } );
                    if (!string.IsNullOrEmpty(result) && result.Contains("Deleted Volumes")) {
                        result = string.Empty;
                    }
                } catch (Exception) {}
                Console.WriteLine($"volume rm {volumes}");
                try { Shell.ExecuteCommand($"docker volume rm {volumes}"); } catch (Exception) {}
                Console.WriteLine($"network rm {networks}");
                try { Shell.ExecuteCommand($"docker network rm {networks}"); } catch (Exception) {}
                Console.WriteLine($"rm ~/.rocketpool");
                try { Shell.ExecuteCommand("rm -rf ~/.rocketpool"); } catch (Exception) { }
                Console.WriteLine($"rm db.store");
                try { stateManager.DB.Clean(); } catch (Exception) { }
                Console.WriteLine($"rm ~/bin/rocketpool");
                try {
                    result = Shell.ExecuteCommand("rm -rf ~/bin/rocketpool");
                } catch(Exception) {}

            } catch (Exception) {
                return result;
            }
            return string.Empty;
        }

        public static bool IsNodeRunning()
        {
            try {
                var result = Shell.ExecuteCommand("docker ps");
                if (result.Contains("keepix_eth2")) {
                    return true;
                }
            } catch (Exception) { }
            return false;
        }


        public static string StopNode()
        {
            ShellCondition conditions = new ShellCondition()
            {
                content = "This is *intentional* and does not indicate a problem with your node",
                answers = new string[] {"y", ""}
            };
            var result = Shell.ExecuteCommand("~/bin/rocketpool --allow-root service stop --yes", new List<ShellCondition>() { conditions } );
            if (result.Contains("stop your validator")) {
                return string.Empty;
            }
            return result;
        }

        public static string ReSyncEth1Node()
        {
            ShellCondition conditions = new ShellCondition()
            {
                content = "Are you SURE you want to delete and resync your main ETH1 client from scratch? This cannot be undone!",
                answers = new string[] {"y", ""}
            };
            var result = Shell.ExecuteCommand("~/bin/rocketpool --allow-root service resync-eth1", new List<ShellCondition>() { conditions } );
            if (result.Contains("Done!")) {
                return string.Empty;
            }
            return result;
        }

        public static string ReSyncEth2Node()
        {
            ShellCondition conditions = new ShellCondition()
            {
                content = "Are you SURE you want to delete and resync your main ETH2 client from scratch? This cannot be undone!",
                answers = new string[] {"y", ""}
            };
            var result = Shell.ExecuteCommand("~/bin/rocketpool --allow-root service resync-eth2", new List<ShellCondition>() { conditions } );
            if (result.Contains("Done!")) {
                return string.Empty;
            }
            return result;
        }

        public static string RegisterNode()
        {
            string timezone = OS.GetTimeZone();
            if (string.IsNullOrEmpty(timezone)) {
                return "Can't find the timezone of your machine, please set it manually or contact support.";
            }

            var cli = $"~/bin/rocketpool --allow-root node register --timezone {timezone}";
            var result = Shell.ExecuteCommand(cli, new List<ShellCondition>() {
                new ShellCondition()
                {
                    content = "These prices include a maximum priority fee",
                    answers = new string[] {""}
                },
                new ShellCondition()
                {
                    content = "Using a max fee of",
                    answers = new string[] {"y", ""}
                }} );
            if (result.Contains("Invalid timezone location")) {
                return $"Invalid timezone {timezone} on your machine, please set it manually or contact support.";
            }

            if (result.Contains("clients not ready")) {
                return "Your node is not correctly synchronized, please restart, wait or reinstall it";
            }

            if (result.Contains("not enough to pay for this transaction")) {
                return "Please send ETH to your wallet to register your node on the network, your balance is not enough to cover the fees.";
            }

            if (result.Contains("The node was successfully registered with Rocket Pool")) {
                return string.Empty;
            }

            return result;
        }

        private static string GetContainers(PluginStateManager stateManager)
        {
            string containers = "keepix_exporter keepix_api keepix_validator keepix_eth2 keepix_node keepix_eth1 keepix_watchtower keepix_grafana keepix_prometheus";
                
            InstallInput? input = stateManager.DB.Retrieve<InstallInput>("INSTALL");
            //adding mev boost containers also if chosen in settings of installation
            if (input != null && input.EnableMEV && input.Mainnet) {
                containers += " keepix_mev-boost";
            }
            return containers;
        }

        private static string GetVolumes()
        {
            return "keepix_eth1clientdata keepix_eth2clientdata keepix_grafana-storage keepix_prometheus-data";
        }

        private static string GetNetworks()
        {
            return "keepix_monitor-net keepix_net";
        }
    }
}
