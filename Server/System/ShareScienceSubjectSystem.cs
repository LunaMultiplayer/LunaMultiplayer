using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Log;
using Server.Server;
using Server.System.Scenario;

namespace Server.System
{
    public static class ShareScienceSubjectSystem
    {
        public static void ScienceSubjectReceived(ClientStructure client, ShareProgressScienceSubjectMsgData data)
        {
            LunaLog.Debug($"Science experiment received: {data.ScienceSubject.Id}");

            //send the science subject update to all other clients
            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
            ScenarioDataUpdater.WriteScienceSubjectDataToFile(data.ScienceSubject);
        }
    }
}
