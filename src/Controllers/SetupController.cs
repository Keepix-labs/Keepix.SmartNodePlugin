using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keepix.PluginSystem;
using Keepix.SmartNodePlugin.DTO.Input;

namespace Keepix.SmartNodePlugin.Controllers
{
    internal class SetupController
    {
        [KeepixPluginFn("install")]
        public static async Task<bool> OnInstall(InstallInput input)
        {
            Console.WriteLine("foo");
            //TODO: Setup RPL (Check if CLI pre-installed, if not install it WINDOWS/UNIX COMPATIBLE)    
            return true;
        }
    }
}
