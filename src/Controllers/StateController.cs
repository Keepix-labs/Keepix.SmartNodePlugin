using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Keepix.PluginSystem;
using Keepix.SmartNodePlugin.DTO.Input;
using Keepix.SmartNodePlugin.Services;

namespace Keepix.SmartNodePlugin.Controllers
{
    internal class StateController
    {

        [KeepixPluginFn("wallet-import")]
        public static async Task<bool> OnWalletImport(WalletInput input)
        {
            if (string.IsNullOrEmpty(input.Mnemonic)) {
                Console.WriteLine("Invalid wallet imported in the function call");
                return false;
            }
            var error = StateService.ImportWallet(input.Mnemonic, input.SkipValidatorKey);
            if (!string.IsNullOrEmpty(error)) {
                Console.WriteLine("An error occured while trying to import your wallet: " + error);
                return false;
            }

            Console.WriteLine("Wallet successfully imported.");
            return true;
        }

        
        [KeepixPluginFn("wallet-export")]
        public static async Task<bool> OnWalletExport()
        {

            var result = StateService.ExportWallet();
            if (string.IsNullOrEmpty(result)) {
                Console.WriteLine("An error occured while trying to export your wallet.");
                return false;
            }

            Console.WriteLine("Wallet infos: " + result);
            return true;
        }
        
    }
}
