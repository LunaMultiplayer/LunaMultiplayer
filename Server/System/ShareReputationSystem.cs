using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Log;
using Server.Server;

namespace Server.System
{
    public static class ShareReputationSystem
    {
        public static void ReputationReceived(ClientStructure client, ShareProgressReputationMsgData data)
        {
            LunaLog.Debug("Reputation received: " + data.Reputation + " - reason: " + data.Reason);

            //send the reputation update to all other clients
            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
        }
    }
}
