using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.System
{
    public static class ShareProgressSystem
    {
        public static void FundsReceived(ClientStructure client, ShareProgressFundsMsgData data)
        {
            LunaLog.Debug("Funds received: " + data.Funds + " - reason: " + data.Reason);

            //send the funds update to all other clients
            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
        }

        public static void ScienceReceived(ClientStructure client, ShareProgressScienceMsgData data)
        {
            LunaLog.Debug("Science received: " + data.Science + " - reason: " + data.Reason);

            //send the science update to all other clients
            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
        }

        public static void ReputationReceived(ClientStructure client, ShareProgressReputationMsgData data)
        {
            LunaLog.Debug("Reputation received: " + data.Reputation + " - reason: " + data.Reason);

            //send the reputation update to all other clients
            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
        }

        public static void TechnologyReceived(ClientStructure client, ShareProgressTechnologyMsgData data)
        {
            LunaLog.Debug("Technology unlocked: " + data.TechId);

            //send the technology update to all other clients
            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
        }

        public static void ContractsReceived(ClientStructure client, ShareProgressContractMsgData data)
        {
            LunaLog.Debug("Contract data received:");

            foreach (var item in data.Contracts)
            {
                LunaLog.Debug(item.ContractGuid.ToString());
            }

            //send the contract update to all other clients
            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
        }

        public static void MilestonesReceived(ClientStructure client, ShareProgressMilestoneMsgData data)
        {
            LunaLog.Debug("Milestone data received:");

            foreach (var item in data.Milestones)
            {
                LunaLog.Debug(item.Id);
            }

            //send the contract update to all other clients
            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
        }
    }
}
