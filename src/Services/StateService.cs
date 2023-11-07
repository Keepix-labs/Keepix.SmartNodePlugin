using Keepix.SmartNodePlugin.Utils;
using System.Runtime.InteropServices;
using Keepix.SmartNodePlugin.DTO.Input;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using System.Text.Json;

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
    }
}
