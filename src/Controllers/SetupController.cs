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
    internal class SetupController
    {
        private static PluginStateManager stateManager;

        // THIS PLUGIN ONLY WORKS WITH LINUX OR OSX OR WSL2 WINDOWS AS ROCKETPOOL IS NOT COMPATIBLE WITH WINDOWS OS
        [KeepixPluginFn("install")]
        public static async Task<bool> OnInstall(InstallInput input)
        {
            var isDockerRunning = SetupService.isDockerRunning();
            if (!isDockerRunning) {
                Console.WriteLine("Docker is not live on your device, please start it");
                return false;
            }
            stateManager = PluginStateManager.GetStateManager();
            if (stateManager.Step.Equals("INSTALLED")) {
                Console.WriteLine("The smart-node is already Installed.");
                return false;
            }
            // if (stateManager.State != PluginStateEnum.NO_STATE) {
            //     Console.WriteLine("The smart-node is already in a running state or the installation failed, please uninstall first before a new installation. USE WITH CAUTION.");
            //     return false;
            // }
            stateManager.DB.Store("STATE", PluginStateEnum.STARTING_INSTALLATION);
            //  SETUP CLI START //
            string error = string.Empty;
            if (!SetupService.isCliInstalled())
            {
                Console.WriteLine("Need to install RPL CLI to this machine...");
                stateManager.DB.Store("STATE", PluginStateEnum.INSTALLING_CLI);
                error = SetupService.DownloadCli();
            }

            if (!string.IsNullOrEmpty(error)) {
                Console.WriteLine("Issue while installing the RocketPool CLI on your machine: " + error);
                return false;
            }
            else Console.WriteLine("CLI successfully installed");
            // SETUP CLI END //

            // SETUP SMART NODE START //
            stateManager.DB.Store("STATE", PluginStateEnum.INSTALLING_NODE);
            error = SetupService.InstallSmartNode();
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("Error while trying to install the service for Rocketpool, make sure you have docker installed on your computer: " + error);
                 return false;
            }
            Console.WriteLine("RPL Service installed successfully");
            stateManager.DB.Store("STATE", PluginStateEnum.CONFIGURING_NODE);
            //Setting up smart node config before starting up the node
            error = SetupService.ConfigSmartNode(input, stateManager);
            if (string.IsNullOrEmpty(error)) {
                Console.WriteLine("Smartnode configured correctly with Nimbus and Nethermind, starting the smart node...");

                if (input.AutoStart == null || input.AutoStart == true) { // in case we do not want to auto-start the node after install.
                    stateManager.DB.Store("STATE", PluginStateEnum.STARTING_NODE);
                    error = SetupService.StartSmartNode(stateManager);
                    if (string.IsNullOrEmpty(error)) {
                        stateManager.DB.Store("STATE", PluginStateEnum.NODE_RUNNING);
                        Console.WriteLine("SmartNode successfully started, congratulations on installing your first Ethereum blockchain node!");
                        stateManager.DB.Store("INSTALL", input); //save install input data

                        if (input.WalletMnemonic != null) {
                            // waiting 10secs to make sure the node is correctly started
                            Thread.Sleep(TimeSpan.FromSeconds(10));
                            error = StateService.ImportWallet(input.WalletMnemonic, true);
                            if (!string.IsNullOrEmpty(error)) {
                                Console.WriteLine("An error occured while trying to import your wallet, please install the plugin again: " + error);
                                return false;
                            }
                            else 
                            {
                                Console.WriteLine("Mnemonic wallet successfully imported in the smart-node without validator keys.");
                                WalletImportState walletState = new WalletImportState() {
                                    Loaded = true, ImportedOnValidator = false
                                };
                                stateManager.DB.Store("WALLET_STATE", walletState);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error while trying to trying to start the smartnode: " + error);
                        return false;
                    }
                } else {
                     stateManager.DB.Store("STATE", PluginStateEnum.NODE_STOPPED);
                }
            }
            else
            {
                Console.WriteLine("Error while trying to config the rocketpool smartnode: " + error);
                return false;
            }
            // SETUP SMART NODE END //
            stateManager.DB.Store("STEP", "INSTALLED");
            return true;
        }

        [KeepixPluginFn("start")]
        public static async Task<bool> OnStart()
        {
            var isDockerRunning = SetupService.isDockerRunning();
            if (!isDockerRunning) {
                Console.WriteLine("Docker is not live on your device, please start it");
                return false;
            }
            try
            {
                stateManager = PluginStateManager.GetStateManager();
                  if (stateManager.State == PluginStateEnum.NODE_RUNNING &&
                   SetupService.IsNodeRunning()) {
                    Console.WriteLine("The smart-node is already running!");
                    return false;
                }

                var error = SetupService.StartSmartNode(stateManager);
                if (!string.IsNullOrEmpty(error)) {
                    Console.WriteLine("An error occured while trying to start your node, check manually or contact the Keepix team " + error);
                }
                else
                {
                      stateManager.DB.Store("STATE", PluginStateEnum.NODE_RUNNING);
                      Console.WriteLine("Smart-node successfully started");
                }
            }
            catch (Exception) 
            {
                return false;   
            }
            return true;
        }

        [KeepixPluginFn("restart")]
        public static async Task<bool> OnRestart()
        {
            var isDockerRunning = SetupService.isDockerRunning();
            if (!isDockerRunning) {
                Console.WriteLine("Docker is not live on your device, please start it");
                return false;
            }
            try
            {
                stateManager = PluginStateManager.GetStateManager();
                var isRunning = SetupService.IsNodeRunning();
                string error = string.Empty;
                stateManager.DB.Store("STATE", PluginStateEnum.NODE_RESTARTING);

                if (isRunning) {
                    error = SetupService.StopNode();
                    if (!string.IsNullOrEmpty(error)) {
                        Console.WriteLine("An error occured while trying to stop your node, check manually or contact the Keepix team " + error);
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                    // Wait 30 seconds to make sure it properly stopped the docker instance first
                }

                error = SetupService.StartSmartNode(stateManager);
                if (!string.IsNullOrEmpty(error)) {
                    Console.WriteLine("An error occured while trying to start your node, check manually or contact the Keepix team " + error);
                }
                else
                {
                      stateManager.DB.Store("STATE", PluginStateEnum.NODE_RUNNING);
                      Console.WriteLine("Smart-node successfully started");
                }
            }
            catch (Exception) 
            {
                return false;   
            }
            return true;
        }


        [KeepixPluginFn("stop")]
        public static async Task<bool> OnStop()
        {
            var isDockerRunning = SetupService.isDockerRunning();
            if (!isDockerRunning) {
                Console.WriteLine("Docker is not live on your device, please start it");
                return false;
            }
            try
            {
                stateManager = PluginStateManager.GetStateManager();
                  if (stateManager.State != PluginStateEnum.NODE_RUNNING && !SetupService.IsNodeRunning()) {
                    Console.WriteLine("The smart-node is not running!");
                    return false;
                }

                var error = SetupService.StopNode();
                if (!string.IsNullOrEmpty(error)) {
                    Console.WriteLine("An error occured while trying to stop your node, check manually or contact the Keepix team " + error);
                } 
                else
                {
                    stateManager.DB.Store("STATE", PluginStateEnum.NODE_STOPPED);
                    Console.WriteLine("Smart-node successfully stopped");
                }
            }
            catch (Exception) 
            {
                return false;   
            }
            return true;
        }

        [KeepixPluginFn("uninstall")]
        public static async Task<bool> OnUninstall()
        {
            var isDockerRunning = SetupService.isDockerRunning();
            if (!isDockerRunning) {
                Console.WriteLine("Docker is not live on your device, please start it");
                return false;
            }
            stateManager = PluginStateManager.GetStateManager();
            if (!stateManager.Step.Equals("INSTALLED")) {
                Console.WriteLine("The smart-node is not installed!");
                return true;
            }
            // if (stateManager.State == PluginStateEnum.NO_STATE) {
            //     Console.WriteLine("The smart-node is not installed!");
            //     return true;
            // }
            stateManager.DB.Store("STATE", PluginStateEnum.NO_STATE);
            string error = SetupService.RemoveNode(stateManager);
            if (!string.IsNullOrEmpty(error)) {
                Console.WriteLine("Some errors occured while trying to uninstall the Smartnode " + error);
                return false;
            }
            Console.WriteLine("Smart-node successfully uninstalled");
            return true;
        }

        [KeepixPluginFn("register-node")]
        public static async Task<bool> OnRegisterNode()
        {
            var isDockerRunning = SetupService.isDockerRunning();
            if (!isDockerRunning) {
                Console.WriteLine("Docker is not live on your device, please start it");
                return false;
            }
            stateManager = PluginStateManager.GetStateManager();
            bool isSync = StateService.IsNodeSynced();
            if (!isSync) {
                Console.WriteLine("Your node must be synchronized first before registration");
                return false;
            }

            string error = SetupService.RegisterNode();
            if (!string.IsNullOrEmpty(error)) {
                Console.WriteLine("Some errors occured while trying to register the node on network " + error);
                return false;
            }
            Console.WriteLine("Your node has been successfully registered on the network, you can now stake on the RocketPool Protocol.");
            stateManager.DB.Store("REGISTERED", true);
            return true;
        }

        [KeepixPluginFn("verify-installation")]
        public static async Task<bool> OnVerifyInstallation()
        {
            var isDockerRunning = SetupService.isDockerRunning();
            if (!isDockerRunning) {
                Console.WriteLine("Docker is not live on your device, please start it");
                return false;
            }
            stateManager = PluginStateManager.GetStateManager();
            return SetupService.VerifyInstallation(stateManager);
        }
    }
}
