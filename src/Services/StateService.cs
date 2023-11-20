using Keepix.SmartNodePlugin.Utils;
using System.Runtime.InteropServices;
using Keepix.SmartNodePlugin.DTO.Input;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Keepix.SmartNodePlugin.Services
{
    internal class StateService
    {

        public static string ImportWallet(string mnemonic, bool skipValidatorKey)
        {
            var cli = $"~/bin/rocketpool --allow-root wallet recover --mnemonic '{mnemonic}' --password 'keepixgenerated'";
            if (skipValidatorKey) {
                cli += " --skip-validator-key-recovery";
            }

            var result = Shell.ExecuteCommand(cli);

            if (result.Contains("already initialized")) {
                return "The node wallet is already initialized.";
            }

            if (result.Contains("The node wallet was successfully recovered")) {
                return string.Empty;
            }
            return result;
        }

        public static string PurgeWallet()
        {
            var cli = $"~/bin/rocketpool --allow-root wallet purge";

            var result = Shell.ExecuteCommand(cli, new List<ShellCondition>()
             {
                new ShellCondition()
                {
                    content = "Do you want to continue?",
                    answers = new string[] {"y", ""}
                }
            });
            if (result.Contains("Purge complete.")) {
                return string.Empty;
            }
            return result;
        }

        public static string ExportWallet()
        {
            var result = Shell.ExecuteCommand("~/bin/rocketpool --allow-root wallet export", new List<ShellCondition>()
             {
                new ShellCondition()
                {
                    content = "Exporting a wallet will print sensitive information to your screen",
                    answers = new string[] {"y", ""}
                }
            });
            if (!result.Contains("private key")) return string.Empty;

            return result;
        }

        public static string FetchNodeWallet()
        {
            var result = Shell.ExecuteCommand("~/bin/rocketpool --allow-root wallet status");
            if (!result.Contains("The node wallet is initialized")) {
                return string.Empty;
            }
            string pattern = @"0x[a-fA-F0-9]{40}";
            Match match = Regex.Match(result, pattern);

            return match.Success ? match.Value : string.Empty;
        }

    public static (string executionSyncProgress, string consensusSyncProgress) ExtractPercent(string data)
        {
            string executionSyncProgressPattern = @"primary execution client is still syncing \((\d+\.\d+)%\)";
            string consensusSyncProgressPattern = @"primary consensus client is still syncing \((\d+\.\d+)%\)";

            var executionSyncProgressMatch = Regex.Match(data, executionSyncProgressPattern);
            var consensusSyncProgressMatch = Regex.Match(data, consensusSyncProgressPattern);

            string executionSyncProgress = executionSyncProgressMatch.Success ? executionSyncProgressMatch.Groups[1].Value : "0.00%";
            string consensusSyncProgress = consensusSyncProgressMatch.Success ? consensusSyncProgressMatch.Groups[1].Value : "0.00%";

            return (executionSyncProgress, consensusSyncProgress);
        }

        public static (string executionSyncProgress, string consensusSyncProgress) PercentSync()
        {
            var isSync = IsNodeSynced();
            if (isSync) {
                return ("100%", "100%");
            }
            var result = Shell.ExecuteCommand("~/bin/rocketpool --allow-root node sync");
            return ExtractPercent(result);
        }

        public static bool IsNodeSynced()
        {
            var result = Shell.ExecuteCommand("~/bin/rocketpool --allow-root node sync");
            if (result.Contains("Your primary execution client is fully synced.") && result.Contains("Your primary consensus client is fully synced.")) {
                return true;
            }
            return false;
        }

        public static (string executionSyncProgress, string consensusSyncProgress) GetPercentSync2()
        {
            return (GetPercentSyncEth1(), GetPercentSyncEth2());
        }

        public static string GetPercentSyncEth1()
        {
            try {
                var result = Shell.ExecuteCommand("docker container logs keepix_eth1");
                string pattern = @"[\d]+\.[\d]+ %\) \| queue";
                MatchCollection matches = Regex.Matches(result, pattern);
                if (matches.Count > 0)
                {
                    return matches[matches.Count - 1].Value.Replace("%", "").Replace("queue", "").Replace("|", "").Replace(")", "").Replace("(", "").Trim();
                }
                Console.WriteLine("No maches eth1");
            } catch (Exception error) {
                Console.WriteLine(error);
            }
            return "0";
        }

        public static string GetPercentSyncEth2()
        {
            try {
                var result = Shell.ExecuteCommand("docker container logs keepix_eth2");
                string pattern = @"[\d]+\.[\d]+%\)";
                MatchCollection matches = Regex.Matches(result, pattern);
                if (matches.Count > 0)
                {
                    return matches[matches.Count - 1].Value.Replace("%", "").Replace(")", "").Replace("(", "").Trim();
                }
                Console.WriteLine("No maches eth2");
            } catch (Exception error) {
                Console.WriteLine(error);
            }
            return "0";
        }

        public static string getLogs(bool eth1)
        {
            var cli = eth1 ?  "~/bin/rocketpool --allow-root service logs eth1" : " ~/bin/rocketpool --allow-root service logs eth2";
            var result = Shell.ExecuteCommand(cli, null, 5);

            return result;
        }

        public static string getTotalRPLStaked() {
            string amount = "0";
            try {
                var result = Shell.ExecuteCommand("~/bin/rocketpool --allow-root node status");
                string pattern = @"total stake of (\d+\.\d+) RPL";

                Match match = Regex.Match(result, pattern);
                if (match.Success)
                    return match.Groups[1].Value;
            }
            catch (Exception) {return "0";}
            return amount;
        }

        public static string getMiniPools() {
            string result = Shell.ExecuteCommand("~/bin/rocketpool --allow-root minipool status");
            return result;
        }
    }
}
