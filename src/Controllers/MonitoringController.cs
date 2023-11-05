using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Keepix.PluginSystem;
using Keepix.SmartNodePlugin.DTO.Input;
using Keepix.SmartNodePlugin.Services;
using Newtonsoft.Json;

namespace Keepix.SmartNodePlugin.Controllers
{
    internal class MonitoringController
    {
        private static PluginStateManager stateManager;
        [KeepixPluginFn("status")]
        public static async Task<bool> OnStatus(InstallInput input)
        {
            stateManager = PluginStateManager.GetStateManager();
            var isRunning = SetupService.IsNodeRunning();

            Console.WriteLine(JsonConvert.SerializeObject(new {
                NodeState = stateManager.State.ToString(),
                Alive = isRunning
            }));
            return true;
        }

        [KeepixPluginFn("sync-state")]
        public static async Task<bool> OnSyncState(InstallInput input)
        {
            stateManager = PluginStateManager.GetStateManager();

            Console.WriteLine(JsonConvert.SerializeObject(new {
                SyncState = "UNSYNCED",
                Progress = "X%"
            }));
            return true;
        }
    }
}
