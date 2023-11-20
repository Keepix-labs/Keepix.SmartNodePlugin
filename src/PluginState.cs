using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keepix.SmartNodePlugin.Storage;
using Keepix.SmartNodePlugin.Utils;

namespace Keepix.SmartNodePlugin
{
    public enum PluginStateEnum {
        NO_STATE,
        STARTING_INSTALLATION,
        INSTALLING_CLI,
        INSTALLING_NODE,
        CONFIGURING_NODE,
        STARTING_NODE,
        NODE_RUNNING,
        NODE_STOPPED,
        NODE_RESTARTING,
        SETUP_ERROR_STATE,
    }

    internal class PluginStateManager
    {
        public PluginStateEnum State { get; set; }
        public bool Installed { get; set; }
        public JsonObjectStore DB { get; set; }

        public static string GetStoragePath()
        {
            string pluginsFolder = Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".keepix/plugins");
            if (!Directory.Exists(pluginsFolder)) {
                try { Directory.CreateDirectory(pluginsFolder); } catch (Exception) {}
            }
            string pluginFolder = Path.Combine(pluginsFolder, "smart-node-plugin");
            if (!Directory.Exists(pluginFolder)) {
                try { Directory.CreateDirectory(pluginFolder); } catch (Exception) {}
            }
            return pluginFolder;
        }

        public static PluginStateManager LoadStateManager()
        {
            string storageFolder = GetStoragePath();
            string dbPath = Path.Combine(storageFolder, "db.store");

            var stateManager = new PluginStateManager() {
                DB = new JsonObjectStore(dbPath)
            };
            try {
                stateManager.State = stateManager.DB.Retrieve<PluginStateEnum>("STATE");
            } 
            catch (KeyNotFoundException) {
                stateManager.State = PluginStateEnum.NO_STATE;
            }
            
            // for know the global steps.
            try {
                stateManager.Installed = stateManager.DB.Retrieve<bool>("INSTALLED");
            } 
            catch (KeyNotFoundException) {
                stateManager.Installed = false;
            }
            return stateManager;
        }

        public static PluginStateManager GetStateManager()
        {
            return LoadStateManager();
        }
    }
}
