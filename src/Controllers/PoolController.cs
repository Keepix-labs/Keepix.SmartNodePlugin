using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Keepix.PluginSystem;
using Keepix.SmartNodePlugin.DTO.Input;
using Keepix.SmartNodePlugin.Services;
using Newtonsoft.Json;

namespace Keepix.SmartNodePlugin.Controllers
{
    internal class PoolController
    {
        private static PluginStateManager stateManager;

        [KeepixPluginFn("stake-rpl")]
        public static async Task<bool> OnStakingRpl(StakeRplInput input)
        {
            stateManager = PluginStateManager.GetStateManager();
            bool isRegistered = false;
            bool isAlive = SetupService.IsNodeRunning();
            bool isSync = StateService.IsNodeSynced();
            
            try { isRegistered = stateManager.DB.Retrieve<bool>("REGISTERED"); } catch (Exception) {}
            if (!isRegistered) {
                Console.WriteLine("Please first register your node on the network to create a pool.");
                return false;
            }
            if (!isAlive) {
                Console.WriteLine("Please start the node before trying to stake RPL");
                return false;
            }
            if (!isSync) {
                Console.WriteLine("Your ETH node is not synced yet on the network, please try later.");
                return false;
            }
            string error = PoolService.StakeRPL(input.Amount);
            if (!string.IsNullOrEmpty(error)) {
                if (error.Contains("Validator pubkey is in use")) {
                    Console.WriteLine($"Wallet Already used on another Registered RocketPool Node. error: {error}");
                    return false;
                }
                Console.WriteLine($"An error occured while trying to stake RPL: {error}");
                return false;
            }

            Console.WriteLine($"You successfully staked {input.Amount} RPL.");
            return true;
        }

        [KeepixPluginFn("unstake-rpl")]
        public static async Task<bool> OnUnStakingRpl(StakeRplInput input)
        {
            stateManager = PluginStateManager.GetStateManager();
            bool isRegistered = false;
            bool isAlive = SetupService.IsNodeRunning();
            bool isSync = StateService.IsNodeSynced();
            
            try { isRegistered = stateManager.DB.Retrieve<bool>("REGISTERED"); } catch (Exception) {}
            if (!isRegistered) {
                Console.WriteLine("Please first register your node on the network.");
                return false;
            }
            if (!isAlive) {
                Console.WriteLine("Please start the node before trying to stake RPL");
                return false;
            }
            if (!isSync) {
                Console.WriteLine("Your ETH node is not synced yet on the network, please try later.");
                return false;
            }
            string error = PoolService.UnStakeRPL(input.Amount);
            if (!string.IsNullOrEmpty(error)) {
                Console.WriteLine($"An error occured while trying to unstake RPL: {error}");
                return false;
            }

            Console.WriteLine($"You successfully unstaked {input.Amount} RPL.");
            return true;
        }

        [KeepixPluginFn("create-pool")]
        public static async Task<bool> OnPoolCreation(CreatePoolInput input)
        {
            stateManager = PluginStateManager.GetStateManager();
            int poolSize = input.SmallPool ? 8 : 16;
            bool isRegistered = false;
            bool isAlive = SetupService.IsNodeRunning();
            bool isSync = StateService.IsNodeSynced();
            try { isRegistered = stateManager.DB.Retrieve<bool>("REGISTERED"); } catch (Exception) {}
            if (!isRegistered) {
                Console.WriteLine("Please first register your node on the network to create a pool.");
                return false;
            }
            if (!isAlive) {
                Console.WriteLine("Please start the node before trying to stake RPL");
                return false;
            }
            if (!isSync) {
                Console.WriteLine("Your ETH node is not synced yet on the network, please try later.");
                return false;
            }
            var error = PoolService.CreateMiniPool(input.SmallPool);
            if (!string.IsNullOrEmpty(error)) {
                Console.WriteLine($"An error occured while trying to stake ETH: {error}");
                return false;
            }

            Console.WriteLine($"You successfully staked {poolSize} ETH on the Ethereum network, congratulations!");
            return true;
        }
    }
}
