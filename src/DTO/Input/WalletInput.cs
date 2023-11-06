using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keepix.SmartNodePlugin.DTO.Input
{
    internal class WalletInput
    {
        public string Mnemonic { get;set; }
        public bool SkipValidatorKey { get;set;}
    }
}
