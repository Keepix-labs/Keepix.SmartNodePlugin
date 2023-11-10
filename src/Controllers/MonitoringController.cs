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
        public static async Task<string> OnStatus(InstallInput input)
        {
            stateManager = PluginStateManager.GetStateManager();
            return JsonConvert.SerializeObject(new {
                NodeState = stateManager.State.ToString(),
                Alive = SetupService.IsNodeRunning()
            });
        }

        [KeepixPluginFn("sync-state")]
        public static async Task<string> OnSyncState()
        {
            stateManager = PluginStateManager.GetStateManager();
            var (executionSyncProgress, consensusSyncProgress) = StateService.PercentSync();
            return JsonConvert.SerializeObject(new {
                IsSynced = StateService.IsNodeSynced(),
                executionSyncProgress,
                consensusSyncProgress
            });
        }

        [KeepixPluginFn("logs-eth1")]
        public static async Task<string> OnLogsEth1()
        {
            var logs = StateService.getLogs(true);
            return JsonConvert.SerializeObject(new {
                logs
            });
        }

        
        [KeepixPluginFn("logs-eth2")]
        public static async Task<string> OnLogsEth2()
        {
            var logs = StateService.getLogs(false);
            return JsonConvert.SerializeObject(new {
                logs
            });
        }
    }
}
