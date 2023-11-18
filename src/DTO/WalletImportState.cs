using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keepix.SmartNodePlugin
{
    internal class WalletImportState
    {
        public bool Loaded { get;set; }
        public bool ImportedOnValidator{get;set;}
    }
}
