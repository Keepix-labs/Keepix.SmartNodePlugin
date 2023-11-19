using Keepix.SmartNodePlugin.Utils;
using System.Runtime.InteropServices;
using Keepix.SmartNodePlugin.DTO.Input;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace Keepix.SmartNodePlugin.Services
{
    internal class PoolService
    {
        public static string CreateMiniPool(bool smallPool)
        {
            int poolSize = smallPool ? 8 : 16;
            // smallPool true = 8ETH / smallPool false = 16 ETH
            string result = Shell.ExecuteCommand($"~/bin/rocketpool --allow-root node deposit --amount {poolSize} --yes");

            if (result.Contains("not enough to create a minipool")) {
                return $"You do not have a sufficient ETH balance on your wallet to create a {poolSize} ETH pool";
            }

            if (result.Contains("not staked enough RPL")) {
                return $"You did not staked enough RPL to create a minipool of {poolSize} please add more first, be sure to always over-collateralize your holding.";
            }
            return result;
        }

        public static string StakeRPL(decimal amount) 
        {
            string result;
            var cli = $"~/bin/rocketpool --allow-root node stake-rpl --amount {amount} --yes --swap";

            result = Shell.ExecuteCommand(cli);
            if (result.Contains("transfer amount exceeds balance")) {
                return $"You do not have {amount} RPL on the node wallet, transfer them or make a swap from ETH to RPL.";
            }

            if (result.Contains("Could not get can node stake RPL status") 
            || result.Contains("Error") || result.Contains("Could not")) 
                return result;

            if (result.Contains("Successfully staked")) {
                return string.Empty;
            }

            return result;
        }
    }
}