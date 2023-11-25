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
using Keepix.SmartNodePlugin.Utils;
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
            var (executionSyncProgress, consensusSyncProgress, executionSyncProgressStepDescription, consensusSyncProgressStepDescription) = StateService.PercentSync();
            return JsonConvert.SerializeObject(new {
                IsSynced = executionSyncProgress == "100" && consensusSyncProgress == "100",
                executionSyncProgress,
                consensusSyncProgress,
                executionSyncProgressStepDescription,
                consensusSyncProgressStepDescription
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

        [KeepixPluginFn("ipv4")]
        public static async Task<string> getIpv4()
        {
            return OS.GetMainIPv4().ToString();
        }
    }
}
