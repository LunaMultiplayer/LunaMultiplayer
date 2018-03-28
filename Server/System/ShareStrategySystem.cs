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
    public static class ShareStrategySystem
    {
        public static void StrategyReceived(ClientStructure client, ShareProgressStrategyMsgData data)
        {
            LunaLog.Debug($"strategy changed: {data.Strategy.Name}");

            //Send the strategy update to all other clients
            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
        }
    }
}
