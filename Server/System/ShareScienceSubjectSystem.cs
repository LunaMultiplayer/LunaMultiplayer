using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Server;
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
            LunaLog.Debug($"Science experiment received: {data.ScienceSubject.Id} IsDelta={data.ScienceSubject.IsDelta}");

            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
            ScenarioDataUpdater.WriteScienceSubjectDataToFile(data.ScienceSubject);
        }

        public static void ScienceSubjectRevertReceived(ClientStructure client, ShareProgressScienceSubjectRevertMsgData data)
        {
            LunaLog.Debug($"Science subject revert received from {client.PlayerName}: {data.SubjectCount} subjects");

            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
            ScenarioDataUpdater.WriteScienceSubjectRevertToFile(data);
        }
    }
}
