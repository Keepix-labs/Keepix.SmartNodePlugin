using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keepix.SmartNodePlugin
{
    internal class PluginState
    {
        public const string PLUGIN_STATE_IDLE = "IDLE";
        public const string PLUGIN_STATE_RUNNING = "RUNNING";
        public const string PLUGIN_STATE_INSTALL = "INSTALL";
        public const string PLUGIN_STATE_INSTALL_DOCKER = "INSTALL_DOCKER_IMAGES";

        public string? State { get; set; }
        public float? Progress { get; set; }

        public static PluginState GetNewEmptyState()
        {
            return new PluginState()
            {
                State = PLUGIN_STATE_IDLE,
                Progress = 0f
            };
        }
    }
}
