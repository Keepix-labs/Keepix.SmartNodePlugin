using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Keepix.SmartNodePlugin.Utils
{
    public static class OS
    {
        public static OSPlatform GetOS()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OSPlatform.OSX;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OSPlatform.Linux;
            }
            else
            {
                return OSPlatform.Windows;
            }
        }
    }
}
