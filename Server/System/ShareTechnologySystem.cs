using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Log;
using Server.Server;
using Server.System.Scenario;

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
