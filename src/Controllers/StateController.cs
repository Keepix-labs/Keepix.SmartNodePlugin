using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Keepix.PluginSystem;
using Keepix.SmartNodePlugin.DTO.Input;
using Keepix.SmartNodePlugin.Services;
using Newtonsoft.Json;

namespace Keepix.SmartNodePlugin.Controllers
{
    internal class StateController
    {
        private static PluginStateManager stateManager;

        [KeepixPluginFn("wallet-import")]
        public static async Task<bool> OnWalletImport(WalletInput input)
        {
            var isDockerRunning = SetupService.isDockerRunning();
            if (!isDockerRunning) {
                Console.WriteLine("Docker is not live on your device, please start it");
                return false;
            }
            if (string.IsNullOrEmpty(input.Mnemonic)) {
                Console.WriteLine("Invalid wallet imported in the function call");
                return false;
            }
            var error = StateService.ImportWallet(input.Mnemonic, input.SkipValidatorKey);
            if (!string.IsNullOrEmpty(error)) {
                Console.WriteLine("An error occured while trying to import your wallet: " + error);
                return false;
            }
            await OnWalletFetch(); // add in cache the walletAddress
            Console.WriteLine("Wallet successfully imported.");
            return true;
        }

        
        [KeepixPluginFn("wallet-export")]
        public static async Task<bool> OnWalletExport()
        {
            var isDockerRunning = SetupService.isDockerRunning();
            if (!isDockerRunning) {
                Console.WriteLine("Docker is not live on your device, please start it");
                return false;
            }
            var result = StateService.ExportWallet();
            if (string.IsNullOrEmpty(result)) {
                Console.WriteLine("An error occured while trying to export your wallet.");
                return false;
            }
            Console.WriteLine("Wallet infos: " + result);
            return true;
        }

        [KeepixPluginFn("wallet-fetch")]
        public static async Task<string> OnWalletFetch()
        {
            stateManager = PluginStateManager.GetStateManager();
            var isDockerRunning = SetupService.isDockerRunning();
            if (!isDockerRunning) {
                string? address = stateManager.DB.Retrieve<string>("WalletAddress");
                if (!string.IsNullOrEmpty(address)) {
                    return JsonConvert.SerializeObject(new {
                        Exists = true,
                        Wallet = address
                    });
                }
                Console.WriteLine("Docker is not live on your device, please start it");
                return JsonConvert.SerializeObject(new {
                    Exists = false
                });
            }
            var wallet = StateService.FetchNodeWallet();
            if (string.IsNullOrEmpty(wallet)) {
                Console.WriteLine("You have no wallet loaded yet in your node, please use wallet-import function accordly.");
                return JsonConvert.SerializeObject(new {
                    Exists = false
                });
            }
            stateManager.DB.Store("WalletAddress", wallet);
            return JsonConvert.SerializeObject(new {
                Exists = true,
                Wallet = wallet
            });
        }

        [KeepixPluginFn("wallet-purge")]
        public static async Task<bool> OnWalletPurge()
        {
            stateManager = PluginStateManager.GetStateManager();
            var isDockerRunning = SetupService.isDockerRunning();
            if (!isDockerRunning) {
                Console.WriteLine("Docker is not live on your device, please start it");
                return false;
            }
            stateManager.DB.UnStore("WalletAddress");
            var error = StateService.PurgeWallet();
            if (!string.IsNullOrEmpty(error)) {
                Console.WriteLine("An error occured while trying to purge your wallet: " + error);
                return false;
            }
            return true;
        }

        //TODO: total eth staked, parse minipools amount value

        [KeepixPluginFn("rpl-fetch")]
        public static async Task<string> OnStakedRpl()
        {
            var amount = StateService.getTotalRPLStaked();
            return JsonConvert.SerializeObject(new {
                amount
            });
        }

        
        [KeepixPluginFn("minipool-fetch")]
        public static async Task<string> OnMiniPoolFetch()
        {
            var pools = StateService.getMiniPools();
            return JsonConvert.SerializeObject(new {
                pools
            });
        }

        
        
    }
}
