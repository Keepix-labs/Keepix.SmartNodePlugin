using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keepix.PluginSystem;
using Keepix.SmartNodePlugin.DTO.Input;
using Keepix.SmartNodePlugin.Services;

namespace Keepix.SmartNodePlugin.Controllers
{
    internal class SetupController
    {
        // THIS PLUGIN ONLY WORKS WITH LINUX OR OSX AS ROCKETPOOL IS NOT COMPATIBLE WITH WINDOWS OS
        [KeepixPluginFn("install")]
        public static async Task<bool> OnInstall(InstallInput input)
        {

            //  SETUP CLI START //
            bool cliReady;
            if (!SetupService.isCliInstalled())
            {
                Console.WriteLine("Need to install RPL CLI to this machine...");
                cliReady = SetupService.DownloadCli();
                if (cliReady) {
                    Console.WriteLine("CLI successfully installed");
                }
            } else cliReady = true;

            if (!cliReady) {
                Console.WriteLine("Issue while installing the RocketPool CLI on your machine, please contact the Keepix Team or try again later.");
                return false;
            }
            // SETUP CLI END //


            // SETUP SMART NODE START //
            string error = SetupService.InstallSmartNode();
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("Error while trying to install the service for Rocketpool, make sure you have docker installed on your computer: " + error);
                return false;
            }
            Console.WriteLine("RPL Service installed sucessfully");
            //Setting up smart node config before starting up the node
            error = SetupService.ConfigSmartNode();
            if (string.IsNullOrEmpty(error)) {
                Console.WriteLine("Smartnode configured correctly with Nimbus and Nethermind, starting the smart node...");
                error = SetupService.StartSmartNode();
                if (string.IsNullOrEmpty(error)) {
                    Console.WriteLine("SmartNode successfully started, congratulations on installing your first blockchain node!");
                }
                else
                {
                    Console.WriteLine("Error while trying to trying to start the smartnode: " + error);
                }
            }
            else
            {
                Console.WriteLine("Error while trying to config the rocketpool smartnode: " + error);
            }
            // SETUP SMART NODE END //


            return true;
        }
    }
}
