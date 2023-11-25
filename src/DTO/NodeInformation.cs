using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keepix.SmartNodePlugin
{
    public class NodeInformation
    {
        public string ethWalletBalance { get;set; }
        public string rplWalletBalance { get;set; }
        public string nodeCreditBalance { get;set; }
        public bool isRegistered { get;set; }
        public string nodeRPLStakedBalance { get;set; }
        public string nodeRPLStakedEffectiveBalance { get;set; }
        public string nodeRPLStakedBorrowedETHPercentage { get;set; }
        public string nodeRPLStakedBondedETHPercentage { get;set; }
        public string nodeMinimumRPLStakeNeeded { get;set; }
        public string nodeMaximumRPLStakePossible { get;set; }
        public string rpcUrl { get;set; }
        public string ipv4 { get;set; }
    }
}
