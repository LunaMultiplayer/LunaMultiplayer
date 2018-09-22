using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Log;
using Server.Server;
using Server.System.Scenario;

namespace Server.System
{
    public static class ShareAchievementsSystem
    {
        public static void AchievementsReceived(ClientStructure client, ShareProgressAchievementsMsgData data)
        {
            LunaLog.Debug($"Achievements data received: {data.Id}");

            //send the achievements update to all other clients
            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
            ScenarioDataUpdater.WriteAchievementDataToFile(data);
        }
    }
}
