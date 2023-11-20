using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        [KeepixPluginFn("installed")]
        public static async Task<bool> OnInstalled()
        {
            stateManager = PluginStateManager.GetStateManager();
            return stateManager.Installed;
        }

        [KeepixPluginFn("status")]
        public static async Task<string> OnStatus(InstallInput input)
        {
            bool isNodeRegistered = false;
            stateManager = PluginStateManager.GetStateManager();
            try { isNodeRegistered = stateManager.DB.Retrieve<bool>("REGISTERED"); } catch(Exception) {}
            return JsonConvert.SerializeObject(new {
                NodeState = stateManager.State.ToString(),
                Alive = SetupService.IsNodeRunning(),
                IsRegistered = isNodeRegistered
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

        [KeepixPluginFn("sync-2")]
        public static async Task<string> OnSync2()
        {
            stateManager = PluginStateManager.GetStateManager();
            var (executionSyncProgress, consensusSyncProgress) = StateService.GetPercentSync2();
            return JsonConvert.SerializeObject(new {
                IsSynced = executionSyncProgress == "100" && consensusSyncProgress == "100",
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

        [KeepixPluginFn("storage-path")]
        public static async Task<string> getStoragePath()
        {
            return PluginStateManager.GetStoragePath();
        }
    }
}
