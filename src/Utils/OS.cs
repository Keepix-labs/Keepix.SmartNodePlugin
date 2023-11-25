using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Linq;
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
        
        public static string GetTimeZone()
        {
            string timezone;
            try { timezone = Shell.ExecuteCommand("cat /etc/timezone"); } catch (Exception) { return string.Empty; }
            return timezone.Trim();
        }

        public static bool IsIPv4(IPAddress ipa) => ipa.AddressFamily == AddressFamily.InterNetwork;

        public static IPAddress GetMainIPv4() => NetworkInterface.GetAllNetworkInterfaces()
            .Select((ni)=>ni.GetIPProperties())
            .Where((ip)=> ip.GatewayAddresses.Where((ga) => IsIPv4(ga.Address)).Count() > 0)
            .FirstOrDefault()?.UnicastAddresses?
            .Where((ua) => IsIPv4(ua.Address))?.FirstOrDefault()?.Address;
    }
}
