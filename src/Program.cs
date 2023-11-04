using Keepix.PluginSystem;
using System.Reflection;
using Keepix.SmartNodePlugin;

namespace PluginProgram
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string arg = args.Count() > 0 ? args[0] : "";
            Task task = KeepixPlugin.Run(arg, Assembly.GetExecutingAssembly().GetTypes());
            task.Wait();
        }
    }
}