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

    public static (string executionSyncProgress, string consensusSyncProgress, string executionSyncProgressStepDescription, string consensusSyncProgressStepDescription) ExtractPercent(string data)
        {
            string executionSyncProgressStepDescription = "";
            string consensusSyncProgressStepDescription = "";
            var executionSyncProgressSynced = false;
            var consensusSyncProgressSynced = false;
            if (data.Contains("Your primary execution client is fully synced.")) {
                executionSyncProgressSynced = true;
            }
            if (data.Contains("Your primary consensus client is fully synced.")) {
                consensusSyncProgressSynced = true;
            }
            string executionSyncProgressPattern = @"primary execution client is still syncing \((\d+\.\d+)%\)";
            string consensusSyncProgressPattern = @"primary consensus client is still syncing \((\d+\.\d+)%\)";

            var executionSyncProgressMatch = Regex.Match(data, executionSyncProgressPattern);
            var consensusSyncProgressMatch = Regex.Match(data, consensusSyncProgressPattern);

            string executionSyncProgress = executionSyncProgressSynced ? "100" : (executionSyncProgressMatch.Success ? executionSyncProgressMatch.Groups[1].Value : "0.00");
            string consensusSyncProgress = consensusSyncProgressSynced ? "100" : (consensusSyncProgressMatch.Success ? consensusSyncProgressMatch.Groups[1].Value : "0.00");

            if (executionSyncProgress == "0.00" && consensusSyncProgress == "100") {
                try {
                    //Downloaded   374,457 / 387,633 ( 96.60 %) |
                    var result = Shell.ExecuteCommand("docker container logs keepix_eth1");
                    if (result.Contains("Downloaded") || result.Contains("Old Headers") || result.Contains("Old Receipts")) {
                        if (result.Contains("Downloaded")) {
                            executionSyncProgressStepDescription = "Downloading of the Blocks";
                        }
                        if (result.Contains("Old Headers")) {
                            executionSyncProgressStepDescription = "Downloading of the Old Headers";
                        }
                        if (result.Contains("Old Receipts")) {
                            executionSyncProgressStepDescription = "Downloading of the Old Receipts";
                        }
                        string executionDownloadProgressPattern = @"\( \d+\.\d+ %\) \|";
                        MatchCollection matches = Regex.Matches(result, executionDownloadProgressPattern);
                        executionSyncProgress = (matches.Count > 0 ? matches[matches.Count - 1].Value.Replace("%", "").Replace("|", "").Replace("(", "").Replace(")", "").Trim() : "100");
                        if (executionSyncProgress == "100") {
                            // Since I'm taking the information from the logs, I'm restoring the state that rocketpool should have at that moment, i.e. 99.99%.
                            executionSyncProgress = "99.99";
                        }
                    }
                } catch (Exception) {}
            }

            //backfill: 00h45m (48.61%)
            if (executionSyncProgress == "99.99" && consensusSyncProgress == "100") {
                try {
                    //Downloaded   374,457 / 387,633 ( 96.60 %) |
                    var result = Shell.ExecuteCommand("docker container logs keepix_eth2");
                    if (result.Contains("backfill:")) {
                        consensusSyncProgressStepDescription = "Checking of the attestations.";
                        string concensusBackFillProgressPattern = @"m \(\d+\.\d+%\)";
                        MatchCollection matches = Regex.Matches(result, concensusBackFillProgressPattern);
                        consensusSyncProgress = (matches.Count > 0 ? matches[matches.Count - 1].Value.Replace("m", "").Replace("%", "").Replace("|", "").Replace("(", "").Replace(")", "").Trim() : "100.00");
                        if (((float)Convert.ToDouble(consensusSyncProgress)) < 1 || ((float)Convert.ToDouble(consensusSyncProgress)) > 90) {
                            // Since I'm taking the information from the logs, I'm restoring the state that rocketpool should have at that moment, i.e. 100%.
                            consensusSyncProgress = "100";
                        }
                    }
                } catch (Exception) {}
            }

            return (executionSyncProgress, consensusSyncProgress, executionSyncProgressStepDescription, consensusSyncProgressStepDescription);
        }

        public static (string executionSyncProgress, string consensusSyncProgress, string executionSyncProgressStepDescription, string consensusSyncProgressStepDescription) PercentSync()
        {
            var result = Shell.ExecuteCommand("~/bin/rocketpool --allow-root node sync");
            return ExtractPercent(result);
        }

        public static bool IsNodeSynced()
        {
            var (executionSyncProgress, consensusSyncProgress, ed, cd) = PercentSync();
            return executionSyncProgress == "100" && consensusSyncProgress == "100";
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
