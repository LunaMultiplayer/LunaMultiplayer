using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Log;
using Server.Server;
using Server.System.Scenario;

namespace Server.System
{
    public class ShareExperimentalPartSystem
    {
        public static void ExperimentalPartReceived(ClientStructure client, ShareProgressExperimentalPartMsgData data)
        {
            LunaLog.Debug($"Experimental part received: {data.PartName} Count: {data.Count}");

            //send the experimental part to all other clients
            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
            ScenarioDataUpdater.WriteExperimentalPartDataToFile(data);
        }
    }
}
