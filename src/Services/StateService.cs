using Keepix.SmartNodePlugin.Utils;
using System.Runtime.InteropServices;
using Keepix.SmartNodePlugin.DTO.Input;
using System.Xml.XPath;

namespace Keepix.SmartNodePlugin.Services
{
    internal class StateService
    {

        public static string ImportWallet(string mnemonic, bool skipValidatorKey)
        {
            var cli = $"~/bin/rocketpool wallet recover --mnemonic '{mnemonic}' --password 'keepixgenerated'";
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
            var result = Shell.ExecuteCommand("~/bin/rocketpool wallet export", new List<ShellCondition>()
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
    }
}
