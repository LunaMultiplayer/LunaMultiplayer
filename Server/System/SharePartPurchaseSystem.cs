using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Log;
using Server.Server;
using Server.System.Scenario;

namespace Server.System
{
    public class SharePartPurchaseSystem
    {
        public static void PurchaseReceived(ClientStructure client, ShareProgressPartPurchaseMsgData data)
        {
            LunaLog.Debug($"Part purchased: {data.PartName} Tech: {data.TechId}");

            //send the part purchase to all other clients
            MessageQueuer.RelayMessage<ShareProgressSrvMsg>(client, data);
            ScenarioDataUpdater.WritePartPurchaseDataToFile(data);
        }
    }
}
