using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Log;
using Server.Server;

namespace Server.System
{
    public static class ShareTechnologySystem
    {
        public static void TechnologyReceived(ClientStructure client, ShareProgressTechnologyMsgData data)
        {
            LunaLog.Debug($"Technology unlocked: {data.TechNode.Id}");

            //Send the technology update to all other clients
            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
            ScenarioDataUpdater.WriteTechnologyDataToFile(data);
        }
    }
}
