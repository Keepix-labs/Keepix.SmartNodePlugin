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
        SETUP_ERROR_STATE,
    }

    internal class PluginStateManager
    {
        public PluginStateEnum State { get; set; }
        public JsonObjectStore DB { get; set; }

        public static PluginStateManager LoadStateManager()
        {
            string dataFolder = "./data";
            if (!Directory.Exists(dataFolder)) {
                try { Directory.CreateDirectory(dataFolder); } catch (Exception) {}
            }
            var currentDir = Directory.GetCurrentDirectory();
            string dbPath = Path.Combine(currentDir, "data/db.store");

            var stateManager = new PluginStateManager() {
                DB = new JsonObjectStore(dbPath)
            };
            try {
                stateManager.State = stateManager.DB.Retrieve<PluginStateEnum>("STATE");
            } 
            catch (Exception) {
                stateManager.State = PluginStateEnum.NO_STATE;
            }
            return stateManager;
        }

        public static PluginStateManager GetStateManager()
        {
            return LoadStateManager();
        }
    }
}
