using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keepix.SmartNodePlugin.DTO.Input
{
    internal class InstallInput
    {
        public string? WalletMnemonic { get; set; }
        public bool? AutoStart { get; set;}
        public bool EnableMEV { get;set; }
        public bool Mainnet { get; set; }
    }
}
