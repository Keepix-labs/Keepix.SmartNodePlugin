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
        private static PluginStateManager stateManager;

        // THIS PLUGIN ONLY WORKS WITH LINUX OR OSX OR WSL2 WINDOWS AS ROCKETPOOL IS NOT COMPATIBLE WITH WINDOWS OS
        [KeepixPluginFn("install")]
        public static async Task<bool> OnInstall(InstallInput input)
        {
            stateManager = PluginStateManager.GetStateManager();
            if (stateManager.State != PluginStateEnum.NO_STATE) {
                Console.WriteLine("The smart-node is already in a running state or the installation failed, please uninstall first before a new installation. USE WITH CAUTION.");
                return false;
            }

            stateManager.DB.Store("STATE", PluginStateEnum.STARTING_INSTALLATION);

            //  SETUP CLI START //
            bool cliReady;
            if (!SetupService.isCliInstalled())
            {
                Console.WriteLine("Need to install RPL CLI to this machine...");
                stateManager.DB.Store("STATE", PluginStateEnum.INSTALLING_CLI);
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
            stateManager.DB.Store("STATE", PluginStateEnum.INSTALLING_NODE);
            string error = SetupService.InstallSmartNode();
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("Error while trying to install the service for Rocketpool, make sure you have docker installed on your computer: " + error);
                return false;
            }
            Console.WriteLine("RPL Service installed sucessfully");
            stateManager.DB.Store("STATE", PluginStateEnum.CONFIGURING_NODE);
            //Setting up smart node config before starting up the node
            error = SetupService.ConfigSmartNode();
            if (string.IsNullOrEmpty(error)) {
                Console.WriteLine("Smartnode configured correctly with Nimbus and Nethermind, starting the smart node...");
                stateManager.DB.Store("STATE", PluginStateEnum.STARTING_NODE);
                error = SetupService.StartSmartNode();
                if (string.IsNullOrEmpty(error)) {
                    stateManager.DB.Store("STATE", PluginStateEnum.NODE_RUNNING);
                    Console.WriteLine("SmartNode successfully started, congratulations on installing your first Ethereum blockchain node!");
                }
                else
                {
                    Console.WriteLine("Error while trying to trying to start the smartnode: " + error);
                    return false;
                }
            }
            else
            {
                Console.WriteLine("Error while trying to config the rocketpool smartnode: " + error);
                return false;
            }
            // SETUP SMART NODE END //


            return true;
        }

        [KeepixPluginFn("uninstall")]
        public static async Task<bool> OnUninstall()
        {
            try
            {
                stateManager = PluginStateManager.GetStateManager();
                  if (stateManager.State == PluginStateEnum.NO_STATE) {
                    Console.WriteLine("The smart-node is not installed!");
                    return false;
                }

                SetupService.RemoveNode();
                stateManager.DB.Store("STATE", PluginStateEnum.NO_STATE);
                Console.WriteLine("Smart-node successfully uninstalled");
            }
            catch (Exception) 
            {
                Console.WriteLine("Error while trying to uninstall the Smartnode, please do it manually or contact Keepix team.");
                return false;   
            }
            return true;
        }
    }
}
