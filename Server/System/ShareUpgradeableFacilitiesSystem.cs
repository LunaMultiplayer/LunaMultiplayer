using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Log;
using Server.Server;
using Server.System.Scenario;

namespace Server.System
{
    public static class ShareUpgradeableFacilitiesSystem
    {
        public static void UpgradeReceived(ClientStructure client, ShareProgressFacilityUpgradeMsgData data)
        {
            LunaLog.Debug($"{client.PlayerName} Upgraded facility {data.FacilityId} To level: {data.Level}");

            //send the upgrade facility update to all other clients
            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
            ScenarioDataUpdater.WriteFacilityLevelDataToFile(data.FacilityId, data.NormLevel);
        }
    }
}
