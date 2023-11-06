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
            //TODO: Check if docker is up and ready
            stateManager = PluginStateManager.GetStateManager();
            if (stateManager.State != PluginStateEnum.NO_STATE) {
                Console.WriteLine("The smart-node is already in a running state or the installation failed, please uninstall first before a new installation. USE WITH CAUTION.");
                return false;
            }

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
                stateManager.DB.Store("STATE", PluginStateEnum.STARTING_NODE);
                error = SetupService.StartSmartNode();
                if (string.IsNullOrEmpty(error)) {
                    stateManager.DB.Store("STATE", PluginStateEnum.NODE_RUNNING);
                    Console.WriteLine("SmartNode successfully started, congratulations on installing your first Ethereum blockchain node!");
                    stateManager.DB.Store("INSTALL", input); //save install input data
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

        [KeepixPluginFn("start")]
        public static async Task<bool> OnStart()
        {
            try
            {
                stateManager = PluginStateManager.GetStateManager();
                  if (stateManager.State == PluginStateEnum.NODE_RUNNING &&
                   SetupService.IsNodeRunning()) {
                    Console.WriteLine("The smart-node is already running!");
                    return false;
                }

                var error = SetupService.StartSmartNode();
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

                error = SetupService.StartSmartNode();
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

                stateManager = PluginStateManager.GetStateManager();
                  if (stateManager.State == PluginStateEnum.NO_STATE) {
                    Console.WriteLine("The smart-node is not installed!");
                    return false;
                }

                stateManager.DB.Store("STATE", PluginStateEnum.NO_STATE);
                string error = SetupService.RemoveNode(stateManager);
                if (!string.IsNullOrEmpty(error)) {
                    Console.WriteLine("Some errors occured while trying to uninstall the Smartnode " + error);
                    return false;
                }
                Console.WriteLine("Smart-node successfully uninstalled");
            
            return true;
        }
    }
}
