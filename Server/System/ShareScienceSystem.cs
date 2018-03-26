using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Log;
using Server.Server;

namespace Server.System
{
    public static class ShareScienceSystem
    {
        public static void ScienceReceived(ClientStructure client, ShareProgressScienceMsgData data)
        {
            LunaLog.Debug("Science received: " + data.Science + " - reason: " + data.Reason);

            //send the science update to all other clients
            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
        }
    }
}
